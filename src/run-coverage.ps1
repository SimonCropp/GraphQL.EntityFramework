# PowerShell script to run tests with coverage and generate HTML report

$resultsDirectory = "../TestResults/coverage"
if (Test-Path $resultsDirectory) {
    Remove-Item $resultsDirectory -Recurse -Force
}

# TUnit runs on Microsoft.Testing.Platform, so coverage comes from Microsoft.Testing.Extensions.CodeCoverage
Write-Host "Running tests with code coverage..." -ForegroundColor Cyan
dotnet test --solution GraphQL.EntityFramework.slnx --configuration Release --coverage --coverage-settings coverage.config --results-directory $resultsDirectory

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nGenerating HTML coverage report..." -ForegroundColor Cyan
reportgenerator -reports:"$resultsDirectory/*.cobertura.xml" -targetdir:"../coverage-report" -reporttypes:"Html;HtmlSummary"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nCoverage report generated successfully!" -ForegroundColor Green
    Write-Host "Open ..\coverage-report\index.html to view the report" -ForegroundColor Yellow

    # Open the report in default browser
    Start-Process "..\coverage-report\index.html"
}
else {
    Write-Host "Failed to generate coverage report!" -ForegroundColor Red
    exit $LASTEXITCODE
}
