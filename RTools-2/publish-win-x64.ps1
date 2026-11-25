$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = (Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
if (-not $msbuild -and (Test-Path $vswhere)) {
    $msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $msbuild) { throw 'Install Visual Studio 2022 17.8+ or Build Tools with .NET desktop development and the .NET Framework 4.7.2 targeting pack.' }
& $msbuild RTools.sln /m /p:Configuration=Release '/p:Platform=Any CPU' /nologo
if ($LASTEXITCODE -ne 0) { throw "MSBuild failed ($LASTEXITCODE)." }
& .\RTools.Tests\bin\Release\RTools.Tests.exe
if ($LASTEXITCODE -ne 0) { throw "Regression tests failed ($LASTEXITCODE)." }
# Exercise the actual Release GUI executable, not only the test assembly.
$exe = Join-Path $PSScriptRoot 'RTools\bin\Release\RTools.exe'
$process = Start-Process -FilePath $exe -ArgumentList '--startup-test' -PassThru
if (-not $process.WaitForExit(30000)) {
    $process.Kill()
    throw 'Release GUI startup test timed out.'
}
if ($process.ExitCode -ne 0) {
    throw "Release GUI startup test failed ($($process.ExitCode)). Check $env:LOCALAPPDATA\RTools\startup-error.log."
}
$dest = Join-Path $PSScriptRoot 'publish\win-x64'
New-Item -ItemType Directory -Force $dest | Out-Null
Copy-Item -Path @('.\RTools\bin\Release\RTools.exe', '.\RTools\bin\Release\RTools.exe.config') -Destination $dest -Force
Write-Host "Published to $dest (requires .NET Framework 4.7.2 or later; Windows 11 includes 4.8/4.8.1)."
