@description('The location')
param location string

@description('The prefix for the resources')
param resourcesPrefix string

@description('The name of the storage account to retrieve contents from')
param contentStorageAccountName string

@description('The name of the storage account to retrieve contents from')
param managementFuncAppName string

var eventGridTopicName = toLower('${resourcesPrefix}-data-topic')

resource contentStorage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: contentStorageAccountName
}

resource managementFuncApp 'Microsoft.Web/sites/functions@2023-12-01'existing = {
  name: managementFuncAppName
}

resource eventGridTopic 'Microsoft.EventGrid/systemTopics@2024-06-01-preview' = {
  name: eventGridTopicName
  location: location
  properties: {
    source:contentStorage.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

resource eventGridBlobDeletedSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2024-06-01-preview' = {
  parent: eventGridTopic
  name: 'BlobDeletedCompletedFunctionSub'
  properties: {
    destination: {
      endpointType: 'AzureFunction'
      properties: {
        resourceId:'${managementFuncApp.id}/functions/BlobDeletedCompleted'
        maxEventsPerBatch: 1
        preferredBatchSizeInKilobytes: 64
      }
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobDeleted'
      ]
    }
  }
}

resource eventGridManageInventorySubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2024-06-01-preview' = {
  parent: eventGridTopic
  name: 'InventoryCompletedFunctionSub'
  properties: {
    destination: {
      endpointType: 'AzureFunction'
      properties: {
        resourceId:'${managementFuncApp.id}/functions/ManageInventoryCompleted'
        maxEventsPerBatch: 1
        preferredBatchSizeInKilobytes: 64
      }
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobInventoryPolicyCompleted'
      ]
    }
  }
}
