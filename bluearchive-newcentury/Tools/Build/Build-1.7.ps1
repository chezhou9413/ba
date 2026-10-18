param([string]$GameRoot = 'E:\steam\steamapps\common\RimWorld')
$ErrorActionPreference = 'Stop'
$mod = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
& (Join-Path $PSScriptRoot 'Sync-GameTextures.ps1')
#1.7 共享玩法定义和界面配置，不加载 1.6 的 Unity 程序集与资源包。
foreach ($folder in @('Defs', 'Languages', 'Patches', 'UI')) {
    $target = Join-Path $mod "1.7\$folder"
    [IO.Directory]::CreateDirectory($target) | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $mod "1.6\$folder") -Force |
        Copy-Item -Destination $target -Recurse -Force
}
$announcement = Join-Path $mod '1.7\Defs\UP.xml'
$xml = [IO.File]::ReadAllText($announcement, [Text.Encoding]::UTF8).Replace('Notice.NoticeDef', 'BANWlLib.Announcements.UpdateAnnouncementDef')
[IO.File]::WriteAllText($announcement, $xml, [Text.UTF8Encoding]::new($false))
& dotnet build (Join-Path $mod '1.7\Source\BANWlLib\BANWlLib.csproj') -c Release --nologo "-p:RimWorldDir=$GameRoot"
if ($LASTEXITCODE -ne 0) { throw 'BlueArchive 1.7 编译失败。' }
