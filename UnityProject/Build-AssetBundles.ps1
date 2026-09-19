param(
    [ValidateSet('Compile', 'Pawns512', 'Pawns1024', 'PawnsBoth', 'UI', 'Textures', 'Effects')]
    [string]$Target = 'Pawns512',
    [string]$UnityEditor = $env:UNITY_EDITOR_PATH,
    [string]$BuildLog,
    [string]$Pawn1024Output
)

#根据脚本位置定位工程并调用所选资源构建入口，输出留在仓库内。
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$repositoryRoot = Split-Path -Parent $projectRoot
$modRoot = Join-Path $repositoryRoot 'bluearchive-newcentury'
if (-not (Test-Path -LiteralPath (Join-Path $modRoot 'About/About.xml'))) {
    throw "找不到同级 Mod，请保留仓库目录结构：$modRoot"
}
if ([string]::IsNullOrWhiteSpace($UnityEditor)) {
    throw '请通过 -UnityEditor 指定 Unity.exe，或设置 UNITY_EDITOR_PATH 环境变量。'
}
if (-not (Test-Path -LiteralPath $UnityEditor -PathType Leaf)) {
    throw "Unity 编辑器不存在：$UnityEditor"
}
$UnityEditor = (Resolve-Path -LiteralPath $UnityEditor).Path
if ([string]::IsNullOrWhiteSpace($BuildLog)) {
    $BuildLog = Join-Path $projectRoot "Logs/build-$Target.log"
}
$BuildLog = [IO.Path]::GetFullPath($BuildLog)
New-Item -ItemType Directory -Path (Split-Path -Parent $BuildLog) -Force | Out-Null

#编译入口不调用资源打包函数，其余目标按职责选择对应的 Unity 编辑器方法。
$methods = @{
    Pawns512 = 'BAPawnBundleBuilder.BuildAllFromCommandLine'
    Pawns1024 = 'BAPawn1024BundleBuilder.Build'
    PawnsBoth = 'BAPawn1024BundleBuilder.BuildBoth'
    UI = 'BAUIImageBundleBuilder.Build'
    Textures = 'BAFolderTextureBundleBuilder.BuildAllFromCommandLine'
    Effects = 'BAEffectBundleBuilder.BuildAllFromCommandLine'
}
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $projectRoot + '"'),
    '-logFile', ('"' + $BuildLog + '"'))
if ($Target -ne 'Compile') {
    $arguments += @('-executeMethod', $methods[$Target])
}
if (-not [string]::IsNullOrWhiteSpace($Pawn1024Output)) {
    if ($Target -notin @('Pawns1024', 'PawnsBoth')) { throw '只有小人 1024 构建支持指定输出目录。' }
    $arguments += @('-baPawn1024Output', ('"' + [IO.Path]::GetFullPath($Pawn1024Output) + '"'))
}
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
#只等待编辑器进程，避免后台编译服务延迟退出时阻塞构建结果。
$process.WaitForExit()
if ($process.ExitCode -ne 0) {
    throw "Unity 构建失败，退出码 $($process.ExitCode)。日志：$BuildLog"
}
Write-Host "Unity $Target 完成。日志：$BuildLog"
