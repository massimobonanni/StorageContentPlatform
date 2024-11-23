@description('The location')
param location string

@description('The prefix for the resources')
param resourcesPrefix string

var applicationInsightsName = toLower('${resourcesPrefix}-ai')

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

output appInsightName string = applicationInsights.name
