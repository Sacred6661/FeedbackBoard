
Write-Host "Checking prerequisites..." -ForegroundColor Cyan
Write-Host ""

$allOk = $true

# Docker
$docker = Get-Command docker -ErrorAction SilentlyContinue
if ($docker) {
    Write-Host "  Docker: INSTALLED" -ForegroundColor Green
} else {
    Write-Host "  Docker: NOT FOUND - install from https://www.docker.com/products/docker-desktop/" -ForegroundColor Red
    $allOk = $false
}

# Azure CLI
$az = Get-Command az -ErrorAction SilentlyContinue
if ($az) {
    $azVersion = az version --query "azure-cli" -o tsv 2>$null
    Write-Host "  Azure CLI: INSTALLED (v$azVersion)" -ForegroundColor Green
} else {
    Write-Host "  Azure CLI: NOT FOUND install  from https://azcliprod.blob.core.windows.net/msi/azure-cli-2.60.0-x64.msi" -ForegroundColor Red
    $allOk = $false
}

# .NET SDK
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnet) {
    $dotnetVersion = dotnet --version 2>$null
    Write-Host "  .NET SDK: INSTALLED (v$dotnetVersion)" -ForegroundColor Green
} else {
    Write-Host "  .NET SDK: NOT FOUND - install from https://dotnet.microsoft.com/download" -ForegroundColor Red
    $allOk = $false
}

Write-Host ""
if ($allOk) {
    Write-Host "All prerequisites met! Run .\scripts\setup-local.ps1" -ForegroundColor Green
} else {
    Write-Host "Please install missing prerequisites and try again." -ForegroundColor Red
}