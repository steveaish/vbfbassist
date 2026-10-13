<#
    CopyUploads.ps1

    Copies the uploaded documents from the Laravel app (storage\app\uploads) to the folder the API
    serves them from (UploadsPath in VBFB_API.dat). Only new/changed files are copied and nothing is
    deleted, so it is safe to re-run - run it again at cut-over to pick up anything uploaded through
    Laravel in the meantime.

    .\CopyUploads.ps1 -Source C:\DevPlay\Laravel_version\storage\app\uploads -Destination C:\DevPlay\VBFBData\uploads
#>
param(
    [string]$Source = 'C:\DevPlay\Laravel_version\storage\app\uploads',
    [string]$Destination = 'C:\DevPlay\VBFBData\uploads'
)

if (-not (Test-Path $Source)) { throw "Source folder not found: $Source" }
New-Item -ItemType Directory -Force -Path $Destination | Out-Null

# /E subfolders, /XO skip older, /R:2 /W:2 short retries, no /MIR (never delete)
robocopy $Source $Destination /E /XO /R:2 /W:2 /NP /NJH
if ($LASTEXITCODE -ge 8) { throw "robocopy failed with exit code $LASTEXITCODE" }

$count = (Get-ChildItem $Destination -File -Recurse).Count
Write-Host "$count files in $Destination"
