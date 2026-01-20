$ErrorActionPreference = "Stop"

$paths = @(
    "bin",
    "obj",
    "artifacts",
    "publish",
    "AppPackages"
)

foreach ($path in $paths) {
    $fullPath = Join-Path $PSScriptRoot $path
    if (Test-Path $fullPath) {
        Write-Host "Removing $fullPath" -ForegroundColor Cyan
        Remove-Item $fullPath -Recurse -Force
    }
}

Write-Host "Clean complete." -ForegroundColor Green
