# bluearchive-newcentury 本地开发说明

项目仅支持 RimWorld 1.7，使用 .NET 10 和 RimUnity。

## 目录

- 外层目录保存 `build.bat`、`compile_modSelf.bat` 和 `deploy_modSelf.bat`。
- 内层 `bluearchive-newcentury` 是实际模组目录。
- `1.7/Source/BANWlLib/BANWlLib.csproj` 是主库编译入口，包含 `Source/BANWlLib` 的共享玩法源码。
- `Source/MuiceLib/MuiceLib.csproj` 编译瞄准显示组件。
- `Source/References/Newtonsoft.Json.dll` 是主库使用的第三方依赖。
- 两个项目均输出到 `1.7/Assemblies`。
- `1.7/Defs`、`Patches`、`Languages`、`UI` 独立维护。
- `Common`、`Sounds` 与 `1.7/Textures` 保存当前版本使用的图片、音频和配置。
- `1.7/Data/RimUnity` 保存界面、粒子、Spine、材质和着色器的运行资源。

## 编译与部署

在外层目录运行 `build.bat` 或 `compile_modSelf.bat`，默认 Release；可通过第一个参数指定 Debug。

也可以在模组目录运行：

```powershell
& './Tools/Build/Build-1.7.ps1'
```

仅更新普通游戏贴图时运行 `Tools/Build/Sync-GameTextures.ps1`。它只读取相邻 UnityProject 的源图，将普通贴图复制到模组；Common 中已有的同路径图片优先。

关闭游戏后运行外层 `deploy_modSelf.bat`，或模组内的 `Tools/Deploy.ps1`。部署只发布当前版本运行内容，保留原安装目录备份，不携带源码和编译缓存。默认目标为：

```text
E:/steam/steamapps/common/RimWorld/Mods/bluearchive-newcentury
```

Unity 工程独立保留，C# 构建与游戏部署不会改动或启动 Unity。
