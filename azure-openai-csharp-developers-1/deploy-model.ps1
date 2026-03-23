param (
    [Parameter(Mandatory)][string]$subscriptionId,
    [Parameter(Mandatory)][string]$resourceGroupName,
    [Parameter(Mandatory)][string]$azureOpenAIName,
    [Parameter(Mandatory)][string]$modelName,
    [Parameter(Mandatory)][string]$modelVersion,
    [Parameter(Mandatory)][string]$skuName,
    [Parameter(Mandatory)][int]$skuCapacity,
    [Parameter(Mandatory)][string]$deploymentName
)

# Connect to Azure account
Write-Host "Connecting to Azure account..."
Connect-AzAccount

Set-AzContext -SubscriptionId $subscriptionId

$ModelProperties = @{
   Model = @{
       Format = "OpenAI"
       Name = $modelName
       Version = $modelVersion
   }
}
$Sku = @{
   Name = $skuName
   Capacity = $skuCapacity
}

# Deploy the model to Azure OpenAI Service
Write-Host "Deploying model '$modelName' version '$modelVersion' to Azure OpenAI Service '$azureOpenAIName'..."
New-AzCognitiveServicesAccountDeployment `
   -ResourceGroupName $rsourceGroupName `
   -AccountName $azureOpenAIName `
   -Name $deploymentName `
   -Properties $ModelProperties `
   -Sku $Sku