@description('The location')
param location string

@description('The prefix for the resources')
param resourcesPrefix string

@description('The name of the storage account to retrieve contents from')
param contentStorageAccountName string

@description('The name of the application insight tied to the front end')
param applicationInsightName string

var functionAppStorageAccountName = toLower(substring('${resourcesPrefix}contgen${uniqueString(resourceGroup().id)}',0,23))
var funcHostingPlanName = toLower('${resourcesPrefix}-contgen-plan')
var functionAppName = toLower('${resourcesPrefix}-contgen-func')
var containerName = '${resourcesPrefix}documents'

resource contentStorage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: contentStorageAccountName
}

resource applicationInsight 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightName
}

resource functionAppStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: functionAppStorageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource funcHostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: funcHostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: funcHostingPlan.id
    siteConfig: {
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

resource basicPublishingCredentials 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: functionApp
  name: 'scm'
  properties: {
    allow: true
  }
}

resource appSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'appsettings'
  parent: functionApp
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsight.properties.InstrumentationKey
    AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${functionAppStorageAccount.listKeys().keys[0].value}'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${functionAppStorageAccount.listKeys().keys[0].value}'
    WEBSITE_CONTENTSHARE: toLower(functionAppName)
    WEBSITE_RUN_FROM_PACKAGE: '1'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    WEBSITE_NODE_DEFAULT_VERSION: '~10'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    BlobMaximumSizeInKb: '10000'
    BlobMinimumSizeInKb: '5000'
    ContentGeneratorTimer: '0 0 * * * *'
    ContentsSizeForGenerationInKb: '100000'
    StorageConnectionString: 'DefaultEndpointsProtocol=https;AccountName=${contentStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${contentStorage.listKeys().keys[0].value}'
    StorageContainerName: containerName
  }
}
