param location string

param resourcesPrefix string

param secondaryResourcesPrefix string

var storageName = substring('${resourcesPrefix}datastore${uniqueString(resourceGroup().id)}',0,23)

var containerName = '${resourcesPrefix}documents'
var secondaryContainerName = '${secondaryResourcesPrefix}documents'
var inventoryContainerName = 'inventory'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_RAGRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: true
      days: 7
    }
    changeFeed: {
      enabled: true
    }
    containerDeleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: true
      days: 7
    }
    isVersioningEnabled: true
  }
}

resource primaryContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: containerName
  parent: blobService
  properties: {
    publicAccess: 'None'
    metadata: {
      containerType: 'documents'
    }
  }
}

resource secondaryContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: secondaryContainerName
  parent: blobService
  properties: {
    publicAccess: 'None'
    metadata: {
      containerType: 'documents'
    }
  }
}

resource inventoryContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: inventoryContainerName
  parent: blobService
  properties: {
    publicAccess: 'None'
    metadata: {}
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2022-09-01' = {
  name: 'default'
  parent:storageAccount
}

resource inventoryTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2022-09-01' = {
  name: 'inventorystatistics'
  parent:tableService
}

resource lifeCycleRules 'Microsoft.Storage/storageAccounts/managementPolicies@2021-09-01' = {
  name: 'default'
  parent: storageAccount
  dependsOn:[
    primaryContainer
    secondaryContainer
    inventoryContainer
  ]
  properties: {
    policy: {
      rules: [
        {
          enabled: true
          name: 'InventoryRule'
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                delete: {
                  daysAfterCreationGreaterThan: 30
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                '${inventoryContainerName}/'
              ]
            }
          }
        }
        {
          enabled: true
          name: 'LocalDocumentsLifecycleRule'
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterCreationGreaterThan: 10
                }
                tierToArchive: {
                  daysAfterCreationGreaterThan: 30
                }
                delete: {
                  daysAfterCreationGreaterThan: 365
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                '${containerName}/'
              ]
            }
          }
        }
        {
          enabled: true
          name: 'RemoteDocumentsLifecycleRule'
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterCreationGreaterThan: 10
                }
                tierToArchive: {
                  daysAfterCreationGreaterThan: 30
                }
                delete: {
                  daysAfterCreationGreaterThan: 365
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                '${secondaryContainerName}/'
              ]
            }
          }
        }
      ]
    }
  }
}

resource inventoryRules 'Microsoft.Storage/storageAccounts/inventoryPolicies@2021-09-01' = {
  name: 'default'
  parent: storageAccount
  dependsOn:[
    inventoryContainer
  ]
  properties: {
    policy: {
      enabled: true
      type: 'Inventory'
      rules: [
        {
          destination: inventoryContainerName
          enabled: true
          name: 'LocalDocumentsInventoryRule'
          definition: {
            format: 'Csv'
            schedule: 'Daily'
            objectType: 'Blob'
            schemaFields: [
              'Name'
              'Creation-Time'
              'Last-Modified'
              'ETag'
              'Content-Length'
              'Content-Type'
              'Content-Encoding'
              'Content-Language'
              'Content-CRC64'
              'Content-MD5'
              'Cache-Control'
              'Content-Disposition'
              'BlobType'
              'AccessTier'
              'AccessTierChangeTime'
              'AccessTierInferred'
              'Metadata'
              'LastAccessTime'
              'LeaseStatus'
              'LeaseState'
              'LeaseDuration'
              'ServerEncrypted'
              'CustomerProvidedKeySha256'
              'RehydratePriority'
              'ArchiveStatus'
              'EncryptionScope'
              'CopyId'
              'CopyStatus'
              'CopySource'
              'CopyProgress'
              'CopyCompletionTime'
              'CopyStatusDescription'
              'ImmutabilityPolicyUntilDate'
              'ImmutabilityPolicyMode'
              'LegalHold'
              'Tags'
              'TagCount'
            ]
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                '${containerName}/'
              ]
            }
          }
        }
      ]
    }
  }
}

output storageAccountName string = storageAccount.name
