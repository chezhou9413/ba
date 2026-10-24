param([string]$GameRoot = 'E:\steam\steamapps\common\RimWorld')
$ErrorActionPreference = 'Stop'
$mod = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
#1.7 定义已独立维护，编译不覆盖原生特效和公告类型；贴图更新单独运行 Sync-GameTextures.ps1。
& dotnet build (Join-Path $mod '1.7\Source\BANWlLib\BANWlLib.csproj') -c Release --nologo "-p:RimWorldDir=$GameRoot"
if ($LASTEXITCODE -ne 0) { throw 'BlueArchive 1.7 编译失败。' }
