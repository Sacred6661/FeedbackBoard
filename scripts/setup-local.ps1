# setup-local.ps1
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  FeedbackBoard Azure - Local Environment Setup" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$miniblueEndpoint = "http://localhost:4566"
$subscriptionId = "00000000-0000-0000-0000-000000000000"
$apiVersion = "2022-09-01"

# Функція для виклику REST API
function Invoke-MiniblueRest {
    param(
        [string]$Method = "GET",
        [string]$Path,
        [string]$Body = $null,
        [switch]$SkipError = $false
    )
    
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $params = @{
        Uri = "$miniblueEndpoint$Path"
        Method = $Method
        Headers = $headers
        ErrorAction = "Stop"
    }
    
    if ($Body) {
        $params["Body"] = $Body
    }
    
    try {
        $response = Invoke-RestMethod @params
        return $response
    } catch {
        if (-not $SkipError) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            Write-Host "  REST $Method $Path -> $statusCode" -ForegroundColor DarkGray
        }
        return $null
    }
}

# ============================================
# 1. WAIT FOR MINIBLUE
# ============================================
Write-Host ""
Write-Host "Waiting for miniblue to be ready..." -ForegroundColor Yellow

$maxAttempts = 30
$attempt = 0
$miniblueReady = $false

do {
    $attempt++
    try {
        $response = Invoke-WebRequest -Uri "$miniblueEndpoint/subscriptions?api-version=$apiVersion" -Method GET -TimeoutSec 2 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            $miniblueReady = $true
        }
    } catch {
        Write-Host "  Attempt $attempt/$maxAttempts - miniblue not ready yet..." -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
} while (-not $miniblueReady -and $attempt -lt $maxAttempts)

if (-not $miniblueReady) {
    Write-Host "ERROR: miniblue failed to start!" -ForegroundColor Red
    exit 1
}

Write-Host "Miniblue is ready!" -ForegroundColor Green
Start-Sleep -Seconds 2

# ============================================
# 2. CREATING RESOURCES
# ============================================
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  CREATING AZURE RESOURCES" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# --- 1. Resource Group ---
Write-Host "[1/9] Creating Resource Group..." -ForegroundColor Yellow
Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourcegroups/feedbackboard-rg?api-version=$apiVersion" `
    -Body '{"location": "localdev"}'
Write-Host "      Resource Group created!" -ForegroundColor Green

# --- 2. Storage Account ---
Write-Host "[2/9] Creating Storage Account..." -ForegroundColor Yellow
Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.Storage/storageAccounts/feedbackboardstorage?api-version=2023-01-01" `
    -Body '{"sku": {"name": "Standard_LRS"}, "kind": "StorageV2", "location": "localdev"}'

# Standard key for miniblue/Azurite
$STORAGE_KEY = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="

# Creating containers using the Azurite API
$blobHeaders = @{
    "x-ms-version" = "2023-01-01"
    "x-ms-date" = (Get-Date -Format "R")
}

$containers = @("feedback-images", "feedback-attachments")
foreach ($container in $containers) {
    try {
        Invoke-RestMethod -Uri "$miniblueEndpoint/feedbackboardstorage/$container?restype=container" `
            -Method PUT -Headers $blobHeaders -ErrorAction SilentlyContinue | Out-Null
        Write-Host "      Container '$container' created" -ForegroundColor Gray
    } catch {
        Write-Host "      Container '$container' - already exists" -ForegroundColor DarkGray
    }
}

# Creating the AuditLogs table
Write-Host "Creating Table Storage table..." -ForegroundColor Yellow
$tableHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

try {
    Invoke-RestMethod -Uri "$miniblueEndpoint/table/feedbackboardstorage/Tables" `
        -Method POST `
        -Headers $tableHeaders `
        -Body '{"TableName": "AuditLogs"}' `
        -ErrorAction Stop | Out-Null
    Write-Host "  Table 'AuditLogs' created" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 409) {
        Write-Host "  Table 'AuditLogs' already exists" -ForegroundColor DarkGray
    } else {
        Write-Host "  WARNING: Could not create table 'AuditLogs'" -ForegroundColor Yellow
    }
}

Write-Host "      Storage Account configured!" -ForegroundColor Green

# --- 3. Key Vault (via the miniblue direct API: /keyvault/{vault}/secrets/{name}) ---
Write-Host "[3/9] Creating Key Vault..." -ForegroundColor Yellow

$kvSecrets = @(
    @{name="FeedbackBoard--CosmosDb--ConnectionString"; value="AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;"},
    @{name="FeedbackBoard--ServiceBus--ConnectionString"; value="Endpoint=sb://localhost:5672;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=RootManageSharedAccessKey;UseDevelopmentEmulator=true;"},
    @{name="FeedbackBoard--Storage--ConnectionString"; value="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1;"},
    @{name="FeedbackBoard--SqlServer--ConnectionString"; value="Server=localhost,1433;Database=FeedbackBoard;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"},
    @{name="FeedbackBoard--Redis--ConnectionString"; value="localhost:6379"}
)

$kvHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

$secretCount = 0
foreach ($secret in $kvSecrets) {
    try {
        $body = @{ value = $secret.value } | ConvertTo-Json -Compress
        Invoke-RestMethod -Uri "$miniblueEndpoint/keyvault/feedbackboard-kv/secrets/$($secret.name)" `
            -Method PUT `
            -Headers $kvHeaders `
            -Body $body `
            -ErrorAction Stop | Out-Null
        $secretCount++
        Write-Host "      Secret '$($secret.name)' stored" -ForegroundColor Gray
    } catch {
        Write-Host "      WARNING: Could not store '$($secret.name)'" -ForegroundColor Yellow
    }
}

if ($secretCount -gt 0) {
    Write-Host "      Key Vault configured with $secretCount secrets!" -ForegroundColor Green
} else {
    Write-Host "      WARNING: Key Vault setup failed" -ForegroundColor Yellow
}

# --- 4. Service Bus ---
Write-Host "[4/9] Creating Service Bus..." -ForegroundColor Yellow
Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb?api-version=2021-11-01" `
    -Body '{"location": "localdev", "sku": {"name": "Standard"}}'

Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb/queues/feedback-submitted?api-version=2021-11-01" `
    -Body '{"properties": {"maxDeliveryCount": 10}}'

Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb/queues/feedback-processed?api-version=2021-11-01" `
    -Body '{"properties": {"maxDeliveryCount": 10}}'

Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb/topics/feedback-events?api-version=2021-11-01"

Invoke-MiniblueRest -Method PUT -SkipError `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb/topics/feedback-events/subscriptions/analytics-subscription?api-version=2021-11-01" `
    -Body '{"properties": {"maxDeliveryCount": 10}}'

Write-Host "      Service Bus configured!" -ForegroundColor Green

# --- 5. Cosmos DB ---
Write-Host "[5/9] Creating Cosmos DB..." -ForegroundColor Yellow
Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.DocumentDB/databaseAccounts/feedbackboard-cosmos?api-version=2023-04-15" `
    -Body '{"location": "localdev", "kind": "GlobalDocumentDB", "properties": {"databaseAccountOfferType": "Standard"}}'

Write-Host "      Cosmos DB configured!" -ForegroundColor Green

# --- 6. App Configuration ---
Write-Host "[6/9] Creating App Configuration..." -ForegroundColor Yellow
Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.AppConfiguration/configurationStores/feedbackboard-config?api-version=2023-03-01" `
    -Body '{"location": "localdev", "sku": {"name": "Free"}}'

Write-Host "      App Configuration configured!" -ForegroundColor Green

# --- 7. Event Grid ---
Write-Host "[7/9] Creating Event Grid..." -ForegroundColor Yellow
Invoke-MiniblueRest -Method PUT `
    -Path "/subscriptions/$subscriptionId/resourceGroups/feedbackboard-rg/providers/Microsoft.EventGrid/topics/feedback-events?api-version=2023-12-15-preview" `
    -Body '{"location": "localdev"}'

Write-Host "      Event Grid configured!" -ForegroundColor Green

# ============================================
# 3. SUMMARY
# ============================================
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  SETUP COMPLETE!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  All services available at: $miniblueEndpoint" -ForegroundColor White
Write-Host ""
Write-Host "  Created Services:" -ForegroundColor White
Write-Host "    - Resource Group: feedbackboard-rg" -ForegroundColor Gray
Write-Host "    - Storage Account: feedbackboardstorage" -ForegroundColor Gray
Write-Host "    - Key Vault: feedbackboard-kv ($secretCount secrets)" -ForegroundColor Gray
Write-Host "    - Service Bus: feedbackboard-sb" -ForegroundColor Gray
Write-Host "    - Cosmos DB: feedbackboard-cosmos" -ForegroundColor Gray
Write-Host "    - App Configuration: feedbackboard-config" -ForegroundColor Gray
Write-Host "    - Event Grid: feedback-events" -ForegroundColor Gray
Write-Host ""
Write-Host "  Default Storage Key: $STORAGE_KEY" -ForegroundColor White
Write-Host ""
Write-Host "================================================" -ForegroundColor Green

# Перевіряємо Key Vault
Write-Host ""
Write-Host "Verifying Key Vault secrets:" -ForegroundColor Yellow
try {
    $secretsList = Invoke-RestMethod -Uri "$miniblueEndpoint/keyvault/feedbackboard-kv/secrets?api-version=7.4" -Headers @{"Accept"="application/json"}
    $secretsList | ForEach-Object { Write-Host "  - $($_.id)" -ForegroundColor Gray }
} catch {
    Write-Host "  Could not list secrets (this is OK for miniblue)" -ForegroundColor DarkGray
}