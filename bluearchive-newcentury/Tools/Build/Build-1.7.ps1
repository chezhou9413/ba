param([string]$GameRoot = 'E:\steam\steamapps\common\RimWorld', [string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$mod = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
#只编译1.7程序集，定义独立维护；贴图更新单独运行Sync-GameTextures.ps1。
& dotnet build (Join-Path $mod '1.7\Source\BANWlLib\BANWlLib.csproj') -c $Configuration --nologo "-p:RimWorldDir=$GameRoot"
if ($LASTEXITCODE -ne 0) { throw 'BlueArchive 1.7 编译失败。' }
& dotnet build (Join-Path $mod 'Source\MuiceLib\MuiceLib.csproj') -c $Configuration --nologo "-p:RimWorldGamePath=$GameRoot"
if ($LASTEXITCODE -ne 0) { throw 'MuiceLib 1.7 编译失败。' }
