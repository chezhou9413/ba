param([string]$UnityExe = $env:UNITY_EDITOR_PATH)

#调用同目录的统一构建入口，生成所选类别的资源包。
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Build-AssetBundles.ps1') -Target Pawns512 -UnityEditor $UnityExe
