param(
    [string]$UnityEditor = $env:UNITY_EDITOR_PATH,
    [string]$UnityProject = (Join-Path (Split-Path -Parent $PSScriptRoot) 'UnityProject'),
    [string]$BuildLog,
    [switch]$Update512
)

#从相邻 Unity 工程调用统一构建工具，将同名替换包输出到本目录。
$ErrorActionPreference = 'Stop'
$builder = Join-Path $UnityProject 'Build-AssetBundles.ps1'
if (-not (Test-Path -LiteralPath $builder -PathType Leaf)) { throw "Unity 构建入口不存在：$builder" }
$target = if ($Update512) { 'PawnsBoth' } else { 'Pawns1024' }
& $builder -Target $target -UnityEditor $UnityEditor -BuildLog $BuildLog -Pawn1024Output $PSScriptRoot
