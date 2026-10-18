param()
$ErrorActionPreference = 'Stop'
$mod = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$source = Join-Path (Split-Path $mod -Parent) 'UnityProject\Assets\Data\Archive.NewWorld\Textures'
$target = Join-Path $mod '1.7\Textures'
$common = Join-Path $mod 'Common\Textures'
if (!(Test-Path -LiteralPath $source -PathType Container)) { throw "缺少普通贴图源目录：$source" }
#普通 Def 贴图保留原版 ContentFinder 路径；Common 中直接维护的图片优先，不复制 Unity 元数据。
$count = 0
foreach ($file in Get-ChildItem -LiteralPath $source -Recurse -File) {
    if ($file.Extension -notin @('.png', '.jpg', '.jpeg')) { continue }
    $relative = [IO.Path]::GetRelativePath($source, $file.FullName)
    if (Test-Path -LiteralPath (Join-Path $common $relative)) { continue }
    $destination = Join-Path $target $relative
    [IO.Directory]::CreateDirectory((Split-Path $destination -Parent)) | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
    $count++
}
Write-Output "已同步普通游戏贴图：$count 张。"
