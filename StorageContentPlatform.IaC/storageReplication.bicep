param resourcesPrefix string

param secondaryResourcesPrefix string

param secondaryResourceGroupName string

param ruleName string

var storageName = '${resourcesPrefix}datastore'
var secondaryStorageName = '${secondaryResourcesPrefix}datastore'

var containerName = '${resourcesPrefix}documents'
var secondaryContainerName = '${secondaryResourcesPrefix}documents'

var secondaryStorageAccountId = resourceId(secondaryResourceGroupName, 'Microsoft.Storage/storageAccounts', secondaryStorageName)

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing =  {
  name: storageName
}

resource objectReplicationRules 'Microsoft.Storage/storageAccounts/objectReplicationPolicies@2022-09-01' = {
  name: ruleName
  parent: storageAccount
  properties: {
    destinationAccount: secondaryStorageAccountId
    rules: [
      {
        ruleId: ruleName
        destinationContainer: secondaryContainerName
        filters: {
          minCreationTime: '1601-01-01T00:00:00Z'
        }
        sourceContainer: containerName
      }
    ]
    sourceAccount: storageAccount.name
  }
}

