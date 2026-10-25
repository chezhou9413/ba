param([string]$GameRoot = 'E:\steam\steamapps\common\RimWorld')
$ErrorActionPreference = 'Stop'
$mod = Split-Path $PSScriptRoot -Parent
if (Get-Process RimWorld,RimWorldWin64 -ErrorAction SilentlyContinue) { throw '请先关闭 RimWorld，部署不会自动结束游戏进程。' }
if (!(Test-Path -LiteralPath (Join-Path $GameRoot 'Assembly-CSharp.dll'))) { throw '游戏目录无效。' }
foreach ($file in @('1.7\Assemblies\BANWlLib.dll', '1.7\Data\RimUnity\catalog.json', '1.7\Textures\Ability\Icon\Airy\Airy_EX.png', '1.7\Textures\UI\StarIcon.png')) {
    if (!(Test-Path -LiteralPath (Join-Path $mod $file))) { throw "缺少 1.7 构建产物：$file" }
}
$id = Get-Date -Format 'yyyyMMdd-HHmmss'
$mods = [IO.Path]::GetFullPath((Join-Path $GameRoot 'Mods'))
$destination = [IO.Path]::GetFullPath((Join-Path $mods 'bluearchive-newcentury'))
$stage = [IO.Path]::GetFullPath("E:\ModdevMics\Outputs\BlueArchive\Deploy-$id")
$backup = [IO.Path]::GetFullPath("E:\ModdevMics\Backups\BlueArchive\Deploy-$id")
if ((Split-Path $destination -Parent) -ne $mods -or $destination -eq [IO.Path]::GetFullPath($mod)) { throw '部署目标越界。' }
if (Test-Path -LiteralPath $stage) { throw '暂存目录已经存在。' }
[IO.Directory]::CreateDirectory($stage) | Out-Null
#只复制1.7运行内容和共享资源，不发布源码与编译缓存。
foreach ($folder in @('About', 'Common', 'Sounds', 'Docs')) {
    Copy-Item -LiteralPath (Join-Path $mod $folder) -Destination $stage -Recurse
}
foreach ($folder in @('Assemblies', 'Data', 'Defs', 'Languages', 'Patches', 'UI', 'Textures')) {
    $target = Join-Path $stage '1.7'
    [IO.Directory]::CreateDirectory($target) | Out-Null
    Copy-Item -LiteralPath (Join-Path $mod "1.7\$folder") -Destination $target -Recurse
}
Copy-Item -LiteralPath (Join-Path $mod 'LoadFolders.xml') -Destination $stage
if (Get-Process RimWorld,RimWorldWin64 -ErrorAction SilentlyContinue) { throw "游戏已启动，暂存保留在：$stage" }
#替换前确认绝对路径，并保留原部署以便恢复。
if (!(Split-Path $backup -Parent).Equals('E:\ModdevMics\Backups\BlueArchive', [StringComparison]::OrdinalIgnoreCase)) { throw '备份路径越界。' }
if (Test-Path -LiteralPath $destination) {
    [IO.Directory]::CreateDirectory((Split-Path $backup -Parent)) | Out-Null
    Move-Item -LiteralPath $destination -Destination $backup
}
try { Move-Item -LiteralPath $stage -Destination $destination }
catch {
    if (Test-Path -LiteralPath $backup) { Move-Item -LiteralPath $backup -Destination $destination }
    throw
}
foreach ($relative in @('About\About.xml', 'LoadFolders.xml', '1.7\Assemblies\BANWlLib.dll', '1.7\Data\RimUnity\catalog.json')) {
    if ((Get-FileHash -LiteralPath (Join-Path $mod $relative)).Hash -ne (Get-FileHash -LiteralPath (Join-Path $destination $relative)).Hash) {
        throw "部署文件校验失败：$relative"
    }
}
Write-Output "已部署：$destination"
Write-Output "旧版备份：$backup"
