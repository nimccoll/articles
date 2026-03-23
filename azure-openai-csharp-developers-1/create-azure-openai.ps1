param (
    [Parameter(Mandatory)][string]$subscriptionId,
    [Parameter(Mandatory)][string]$resourceGroupName,
    [Parameter(Mandatory)][string]$location,
    [Parameter(Mandatory)][string]$azureOpenAIName,
    [Parameter(Mandatory)][string]$sku
)

# Connect to Azure account
Write-Host "Connecting to Azure account..."
Connect-AzAccount

Set-AzContext -SubscriptionId $subscriptionId

# Create Azure OpenAI Service
Write-Host "Creating Azure OpenAI Service '$azureOpenAIName'..."
New-AzResource -ResourceGroupName $resourceGroupName `
                -Location $location `
                -ResourceName $azureOpenAIName `
                -ResourceType "Microsoft.CognitiveServices/accounts" `
                -Kind "OpenAI" `
                -Sku @{ Name = $sku } `
                -Properties @{ apiProperties = @{} } `
                -Force