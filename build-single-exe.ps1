param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDirectory = "dist"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$output = Join-Path $root $OutputDirectory
$project = Join-Path $root "ScreenExposure.App\ScreenExposure.App.csproj"

if (Test-Path $output) {
    Remove-Item -LiteralPath $output -Recurse -Force
}

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $output

$exe = Join-Path $output "ScreenExposure.App.exe"
if (-not (Test-Path $exe)) {
    throw "Single-file executable was not produced: $exe"
}

$hash = Get-FileHash -Algorithm SHA256 -LiteralPath $exe
"$($hash.Hash)  ScreenExposure.App.exe" | Set-Content -LiteralPath (Join-Path $output "SHA256SUMS.txt") -Encoding ascii

Get-Item -LiteralPath $exe | Select-Object FullName, Length, LastWriteTime
