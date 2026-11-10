$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$expected = 'CA65CD364CC0E800E6686C6EFA3C88BA50B740516C2E9995DF76154C439A4628'
if ((Get-FileHash -LiteralPath 'bdcli.exe' -Algorithm SHA256).Hash -ne $expected) { throw 'Unexpected bundled CLI checksum.' }
New-Item -ItemType Directory -Path 'dist' -Force | Out-Null
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $csc /nologo /target:winexe /optimize+ /win32icon:Discord.ico /out:dist\Discord-BD.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /resource:bdcli.exe,Bundled.bdcli.exe /resource:LICENSE-bdcli.txt,Bundled.LICENSE Program.cs AssemblyInfo.cs
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

