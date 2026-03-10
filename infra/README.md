# Infrastructure — Azure Trusted Signing

This directory contains Bicep templates for provisioning Azure Trusted Signing resources used to sign the NuGet package.

> **Note:** This documentation contains no secrets. All sensitive values (endpoints, client IDs, certificate profile names) are stored as GitHub Encrypted Secrets.

## Prerequisites

- Azure subscription with Trusted Signing available in your region
- Azure CLI installed and authenticated (`az login`)
- Contributor access to the target resource group

## Setup Steps

### 1. Deploy the Signing Account

```powershell
az group create --name rg-oura-mcp-signing --location westeurope
az deployment group create \
  --resource-group rg-oura-mcp-signing \
  --template-file infra/main.bicep \
  --parameters signingAccountName=oura-mcp-signing
```

### 2. Complete Identity Validation

1. Go to Azure Portal → your Trusted Signing Account
2. Navigate to **Objects → Identity Validations**
3. Click **New Identity Validation**
4. Fill in organization or individual details
5. Wait for Microsoft approval (can take minutes to days)

### 3. Create a Certificate Profile

1. In the Trusted Signing Account, go to **Objects → Certificate Profiles**
2. Click **New Certificate Profile**
3. Select **Public Trust** for open source distribution
4. Note the profile name for GitHub Secrets

### 4. Assign IAM Roles

Assign these roles to the GitHub Actions service principal (or managed identity):
- `Trusted Signing Certificate Profile Signer`

### 5. Configure GitHub Secrets

Add these secrets to the GitHub repository settings:

| Secret | Description |
|--------|-------------|
| `SIGNING_ENDPOINT` | Trusted Signing account endpoint URL |
| `SIGNING_ACCOUNT` | Signing account name |
| `SIGNING_CERT_PROFILE` | Certificate profile name |
| `AZURE_CLIENT_ID` | Service principal client ID (for OIDC) |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `NUGET_API_KEY` | NuGet.org API key for publishing |
