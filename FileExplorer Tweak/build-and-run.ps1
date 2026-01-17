$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "FileExplorer Tweak.csproj"

Write-Host "Building..." -ForegroundColor Cyan
dotnet build $project

Write-Host "Running..." -ForegroundColor Cyan
dotnet run --project $project
