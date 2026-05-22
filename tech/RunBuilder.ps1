# ================================================================
# NgLib - Build and Package Script
# Compiles all projects and generates NuGet packages
# ================================================================

$ErrorActionPreference = "Stop"

$SOLUTION = Join-Path $PSScriptRoot "..\dev\NglibOpenSource.sln"
$OUTPUT_DIR = Join-Path $PSScriptRoot "..\publish"
$CONFIGURATION = "Release"

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "NgLib Build and Package" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

try {
    # Clean previous builds
    Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean $SOLUTION --configuration $CONFIGURATION
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }

    # Restore dependencies
    Write-Host ""
    Write-Host "[2/4] Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore $SOLUTION
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    # Build solution
    Write-Host ""
    Write-Host "[3/4] Building solution in $CONFIGURATION mode..." -ForegroundColor Yellow
    dotnet build $SOLUTION --configuration $CONFIGURATION --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    # Create output directory
    if (-not (Test-Path $OUTPUT_DIR)) {
        New-Item -ItemType Directory -Path $OUTPUT_DIR | Out-Null
    }

    # Package projects (not tests)
    Write-Host ""
    Write-Host "[4/4] Creating NuGet packages..." -ForegroundColor Yellow
    
    $projects = @(
        "..\dev\Nglib\Nglib.csproj",
        "..\dev\Nglib.Data\Nglib.Data.csproj",
        "..\dev\Nglib.Formula\Nglib.Formula.csproj"
    )

    foreach ($project in $projects) {
        $projectPath = Join-Path $PSScriptRoot $project
        Write-Host "  - Packing $(Split-Path $projectPath -Leaf)..." -ForegroundColor Gray
        dotnet pack $projectPath --configuration $CONFIGURATION --no-build --output $OUTPUT_DIR
        if ($LASTEXITCODE -ne 0) { throw "Pack failed for $project" }
    }

    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "NuGet packages are available in: $OUTPUT_DIR" -ForegroundColor Green
    Write-Host ""
    
    Get-ChildItem $OUTPUT_DIR -Filter *.nupkg | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor White
    }
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Red
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    Write-Host "================================================================" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    exit 1
}

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
