$base = "http://localhost:4566"

Write-Host "`n=== 1. Resource Group ===" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$base/subscriptions/00000000-0000-0000-0000-000000000000/resourcegroups/feedbackboard-rg?api-version=2022-09-01" -Method GET | ConvertTo-Json

Write-Host "`n=== 2. Storage Account (Blob upload test) ===" -ForegroundColor Cyan
# Правильний шлях для miniblue: /blob/{account}/{container}/{blob}
$testContent = "FeedbackBoard test content"
$testBytes = [System.Text.Encoding]::UTF8.GetBytes($testContent)

try {
    # Створюємо контейнер через правильний шлях
    Invoke-RestMethod -Uri "$base/blob/feedbackboardstorage/feedback-images" `
        -Method PUT -ErrorAction SilentlyContinue | Out-Null
    Write-Host "  Container created/verified" -ForegroundColor Gray
    
    # Завантажуємо файл
    Invoke-RestMethod -Uri "$base/blob/feedbackboardstorage/feedback-images/test-check.txt" `
        -Method PUT `
        -Body $testBytes `
        -ContentType "text/plain"
    Write-Host "  Storage: File uploaded successfully!" -ForegroundColor Green
    
    # Читаємо назад
    $readBack = Invoke-RestMethod -Uri "$base/blob/feedbackboardstorage/feedback-images/test-check.txt" -Method GET
    Write-Host "  Storage: File content: $readBack" -ForegroundColor Green
} catch {
    Write-Host "  Storage: $_" -ForegroundColor Yellow
    Write-Host "  Trying alternative path..." -ForegroundColor Gray
    try {
        # Альтернативний шлях (Azurite-style)
        $storageHeaders = @{
            "x-ms-version" = "2023-01-01"
            "x-ms-date" = (Get-Date -Format "R")
            "x-ms-blob-type" = "BlockBlob"
        }
        Invoke-RestMethod -Uri "$base/feedbackboardstorage/feedback-images/test-check.txt" `
            -Method PUT -Headers $storageHeaders -Body $testBytes -ContentType "text/plain"
        Write-Host "  Storage: Upload successful with alternative path!" -ForegroundColor Green
    } catch {
        Write-Host "  Storage: Both paths failed - may need different endpoint" -ForegroundColor Red
    }
}

Write-Host "`n=== 3. Key Vault - Secrets ===" -ForegroundColor Cyan
try {
    $secrets = Invoke-RestMethod -Uri "$base/keyvault/feedbackboard-kv/secrets?api-version=7.4" -Method GET
    Write-Host "  Key Vault: $($secrets.value.Count) secrets found" -ForegroundColor Green
    $secrets.value | ForEach-Object { Write-Host "    - $($_.id)" -ForegroundColor Gray }
} catch {
    Write-Host "  Key Vault: $_" -ForegroundColor Yellow
}

Write-Host "`n=== 4. Service Bus (ARM API) ===" -ForegroundColor Cyan
$sbApiVersion = "2021-11-01"
$sbBase = "$base/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb"

try {
    $queues = Invoke-RestMethod -Uri "$sbBase/queues?api-version=$sbApiVersion" -Method GET
    Write-Host "  Queues:" -ForegroundColor Green
    $queues.value | ForEach-Object { Write-Host "    - $($_.name)" -ForegroundColor Gray }
} catch {
    Write-Host "  Queues: $_" -ForegroundColor Yellow
}

try {
    $topics = Invoke-RestMethod -Uri "$sbBase/topics?api-version=$sbApiVersion" -Method GET
    Write-Host "  Topics:" -ForegroundColor Green
    $topics.value | ForEach-Object { Write-Host "    - $($_.name)" -ForegroundColor Gray }
} catch {
    Write-Host "  Topics: $_" -ForegroundColor Yellow
}

Write-Host "`n=== 5. Cosmos DB ===" -ForegroundColor Cyan
try {
    $cosmos = Invoke-RestMethod -Uri "$base/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/feedbackboard-rg/providers/Microsoft.DocumentDB/databaseAccounts/feedbackboard-cosmos?api-version=2023-04-15" -Method GET
    Write-Host "  Cosmos DB: $($cosmos.name) ($($cosmos.properties.provisioningState))" -ForegroundColor Green
} catch {
    Write-Host "  Cosmos DB: $_" -ForegroundColor Yellow
}

Write-Host "`n=== 6. App Configuration ===" -ForegroundColor Cyan
try {
    $appcfg = Invoke-RestMethod -Uri "$base/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/feedbackboard-rg/providers/Microsoft.AppConfiguration/configurationStores/feedbackboard-config?api-version=2023-03-01" -Method GET
    Write-Host "  App Config: $($appcfg.name) ($($appcfg.properties.provisioningState))" -ForegroundColor Green
} catch {
    Write-Host "  App Config: $_" -ForegroundColor Yellow
}

Write-Host "`n=== 7. Event Grid ===" -ForegroundColor Cyan
try {
    $evg = Invoke-RestMethod -Uri "$base/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/feedbackboard-rg/providers/Microsoft.EventGrid/topics/feedback-events?api-version=2023-12-15-preview" -Method GET
    Write-Host "  Event Grid: $($evg.name) ($($evg.properties.provisioningState))" -ForegroundColor Green
} catch {
    Write-Host "  Event Grid: $_" -ForegroundColor Yellow
}

Write-Host "`n================================================" -ForegroundColor Green
Write-Host "  CHECK COMPLETE" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green