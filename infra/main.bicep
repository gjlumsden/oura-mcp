@description('Name of the Trusted Signing account')
param signingAccountName string

@description('Azure region for the signing account')
param location string = resourceGroup().location

@description('SKU for the signing account')
@allowed(['Basic', 'Premium'])
param skuName string = 'Basic'

resource signingAccount 'Microsoft.CodeSigning/codeSigningAccounts@2024-09-30-preview' = {
  name: signingAccountName
  location: location
  properties: {
    sku: {
      name: skuName
    }
  }
}

output signingAccountId string = signingAccount.id
output signingAccountName string = signingAccount.name
