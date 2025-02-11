@description('The location')
param location string

@description('The prefix for the resources')
param resourcesPrefix string

@description('The name of the storage account to retrieve contents from')
param contentStorageAccountName string

@description('The name of the application insight tied to the front end')
param applicationInsightName string

var appServiceName = toLower('${resourcesPrefix}-fe')
var appServicePlanName = toLower('${resourcesPrefix}-fe-plan')

resource contentStorage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: contentStorageAccountName
}

resource applicationInsight 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightName
}

resource frontEndAppServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: false
  }
  sku: {
    name:'F1'
  }
  kind: 'windows'
}

resource frontEndAppService 'Microsoft.Web/sites@2021-02-01' = {
  name: appServiceName
  location: location
  kind: 'app'
  properties: {
    httpsOnly: true
    serverFarmId: frontEndAppServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource basicPublishingCredentials 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: frontEndAppService
  name: 'scm'
  properties: {
    allow: true
  }
}

resource appSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'appsettings'
  parent: frontEndAppService
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsight.properties.InstrumentationKey
    APPINSIGHTS_CONNECTION_STRING: 'InstrumentationKey=${applicationInsight.properties.InstrumentationKey}'
    ContainerTypes: 'documents'
    DeployRegion: location
    StatisticTableName: 'inventorystatistics'
    StorageConnectionString: 'DefaultEndpointsProtocol=https;AccountName=${contentStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${contentStorage.listKeys().keys[0].value}'
  }
}
