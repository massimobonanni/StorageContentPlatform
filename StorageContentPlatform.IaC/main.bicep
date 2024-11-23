targetScope = 'subscription'

@description('The prefix of the resource group name that contains all the resources')
param resourceGroupNamePrefix string = 'StorageContentPlatform'

@description('The primary location of the resources')
param primaryLocation string = deployment().location

@description('The secondary location of the resources')
param secondaryLocation string

@description('The prefix for the resources in the primary location')
param primaryResourcesPrefix string

@description('The prefix for the resources in the secondary location')
param secondaryResourcesPrefix string

@description('Deploy the event grid resources')
param deployEventGrid bool = false

var primaryResourceGroupName = '${resourceGroupNamePrefix}-${primaryLocation}-rg'
var secondaryResourceGroupName = '${resourceGroupNamePrefix}-${secondaryLocation}-rg'

resource primaryResourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: primaryResourceGroupName
  location: primaryLocation
}

resource secondaryResourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: secondaryResourceGroupName
  location: secondaryLocation
}

module primaryAppInsight 'applicationinsight.bicep' = {
  scope: primaryResourceGroup
  name: 'primaryAppInsight'
  params: {
    location: primaryLocation
    resourcesPrefix: primaryResourcesPrefix
  }
}

module secondaryAppInsight 'applicationinsight.bicep' = {
  scope: secondaryResourceGroup
  name: 'secondaryAppInsight'
  params: {
    location: secondaryLocation
    resourcesPrefix: secondaryResourcesPrefix
  }
}

module primaryFrontEnd 'frontEnd.bicep' = {
  scope: primaryResourceGroup
  name: 'primaryFrontEnd'
  params: {
    location: primaryLocation
    resourcesPrefix: primaryResourcesPrefix
    contentStorageAccountName: primaryStorage.outputs.storageAccountName
    applicationInsightName: primaryAppInsight.outputs.appInsightName
  }
}

module secondaryFrontEnd 'frontEnd.bicep' = {
  scope: secondaryResourceGroup
  name: 'secondaryFrontEnd'
  params: {
    location: secondaryLocation
    resourcesPrefix: secondaryResourcesPrefix
    contentStorageAccountName: secondaryStorage.outputs.storageAccountName
    applicationInsightName: secondaryAppInsight.outputs.appInsightName
  }
}

module primaryContentGeneratorFunction 'contentgeneratorfunction.bicep' = {
  scope: primaryResourceGroup
  name: 'primaryContentGeneratorFunction'
  params: {
    location: primaryLocation
    resourcesPrefix: primaryResourcesPrefix
    contentStorageAccountName: primaryStorage.outputs.storageAccountName
    applicationInsightName: primaryAppInsight.outputs.appInsightName
  }
}

module secondaryContentGeneratorFunction 'contentgeneratorfunction.bicep' = {
  scope: secondaryResourceGroup
  name: 'secondaryContentGeneratorFunction'
  params: {
    location: secondaryLocation
    resourcesPrefix: secondaryResourcesPrefix
    contentStorageAccountName: secondaryStorage.outputs.storageAccountName
    applicationInsightName: secondaryAppInsight.outputs.appInsightName
  }
}

module primaryManagementFunction 'managementfunction.bicep' = {
  scope: primaryResourceGroup
  name: 'primaryManagementFunction'
  params: {
    location: primaryLocation
    resourcesPrefix: primaryResourcesPrefix
    contentStorageAccountName: primaryStorage.outputs.storageAccountName
    applicationInsightName: primaryAppInsight.outputs.appInsightName
  }
}

module secondaryManagementFunction 'managementfunction.bicep' = {
  scope: secondaryResourceGroup
  name: 'secondarymanagementFunction'
  params: {
    location: secondaryLocation
    resourcesPrefix: secondaryResourcesPrefix
    contentStorageAccountName: secondaryStorage.outputs.storageAccountName
    applicationInsightName: secondaryAppInsight.outputs.appInsightName
  }
}

module primaryStorage 'storage.bicep' = {
  scope: primaryResourceGroup
  name: 'primaryStorage'
  params: {
    location: primaryLocation
    resourcesPrefix: primaryResourcesPrefix
    secondaryResourcesPrefix: secondaryResourcesPrefix
  }
}

module secondaryStorage 'storage.bicep' = {
  scope: secondaryResourceGroup
  name: 'secondaryStorage'
  params: {
    location: secondaryLocation
    resourcesPrefix: secondaryResourcesPrefix
    secondaryResourcesPrefix: primaryResourcesPrefix
  }
}


//module primaryObjectReplication 'storageReplication.bicep' = {
//  scope: primaryResourceGroup
//  name: 'primaryObjectReplication'
//  dependsOn: [
//    primaryStorage
//    secondaryStorage
//  ]
//  params: {
//    resourcesPrefix: primaryResourcesPrefix
//    secondaryResourcesPrefix: secondaryResourcesPrefix
//    secondaryResourceGroupName: secondaryResourceGroupName
//     ruleName: '7f917681-5360-4013-a1c3-608ba77e8120'
//  }
//}

//module secondaryObjectReplication 'storageReplication.bicep' = {
//  scope: secondaryResourceGroup
//  name: 'secondaryObjectReplication'
//  dependsOn: [
//    primaryStorage
//    secondaryStorage
//  ]
//  params: {
//    resourcesPrefix: secondaryResourcesPrefix
//    secondaryResourcesPrefix: primaryResourcesPrefix
//    secondaryResourceGroupName: primaryResourceGroupName
//    ruleName: 'b2aa23d1-5aa9-4097-bb1b-d34f8923b5c2'
//  }
//}

module primaryEventGrid 'eventgrid.bicep'  = if (deployEventGrid) {
  scope: primaryResourceGroup
  name: 'primaryEventGrid'
  params: {
    location: primaryLocation
    resourcesPrefix: primaryResourcesPrefix
    contentStorageAccountName: primaryStorage.outputs.storageAccountName
    managementFuncAppName: primaryManagementFunction.outputs.functionAppName
  }
}

module secondaryEventGrid 'eventgrid.bicep'  = if (deployEventGrid) {
  scope: secondaryResourceGroup
  name: 'secondaryEventGrid'
  params: {
    location: secondaryLocation
    resourcesPrefix: secondaryResourcesPrefix
    contentStorageAccountName: secondaryStorage.outputs.storageAccountName
    managementFuncAppName: secondaryManagementFunction.outputs.functionAppName
  }
}
