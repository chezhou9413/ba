# 贴图路径与 Unity 工程说明

模组仅发布 RimWorld 1.7 内容。Unity 工程和其中的源图、脚本、导入设置独立保留。

## 仓库目录

```text
仓库根目录/
├─ bluearchive-newcentury/
│  ├─ About/
│  ├─ Common/Textures/                当前版本共享普通贴图
│  ├─ Common/UIAssets/                界面散图
│  ├─ 1.7/Defs/                       角色与技能配置
│  ├─ 1.7/Textures/                   原版系统读取的普通贴图
│  ├─ 1.7/Data/RimUnity/              UI、Spine、特效和材质资源
│  ├─ 1.7/UI/images/bundles.xml       UI图片索引
│  └─ Tools/Build/                    模组构建、贴图同步和导出工具
└─ UnityProject/                     独立制作工程，安装模组时不复制
```

## 从配置定位源图

在 `bluearchive-newcentury/1.7/Defs` 中查看角色或技能的 `texPath`、`wornGraphicPath`、`iconPath`、`StudentAvatar`、`CharacterimagePath` 等字段。

| 图片用途 | UnityProject 中的源位置 |
| --- | --- |
| 身体、头发、服装和光环 | `Assets/Data/Archive.NewWorld/Textures/Pawns` |
| 技能图标 | `Assets/Data/Archive.NewWorld/Textures/Ability` |
| 抽卡头像 | `Assets/Data/Archive.NewWorld/Textures/Gacha` |
| 学生手册头像、立绘与背景 | `Assets/BAUIImages/Common/UIAssets/ManuaUI` |
| 特效图片 | `Assets/EffectImages` |

普通贴图路径不带扩展名。身体、头发和服装可能追加朝向、体型后缀，应以实际文件为准。`Common/Textures` 中直接维护的图片优先于同步源图。

UI 图片可在 `1.7/UI/images/bundles.xml` 中按 `key` 查找；`asset` 是制作工程内的源路径，同时用于 RimUnity 的 `UIImage/` 入口。`bundle` 保留为图片分组标识，不代表游戏还要加载 Unity AB 文件。

## 更新运行资源

普通 Def 贴图在模组目录运行：

```powershell
& './Tools/Build/Sync-GameTextures.ps1'
```

该脚本只读取 Unity 源图并更新 `1.7/Textures`，不修改 Unity 工程。

界面、Spine、材质和特效通过 RimUnity 导出到暂存目录，再将整理后的内容发布到 `1.7/Data/RimUnity`。模组维护的导出工具位于 `Tools/Build`：

- `BAImageRimUnityExport.cs`：图片和特效资源。
- `BAUiRimUnityProbe.cs` 与 `UIExportAssets.txt`：界面资源及源路径清单。
- `BAAssetPublish.cs`：合并导出清单并整理载荷。

Unity 工程内现有脚本、菜单和输出设置保持原样，其中的旧资源包构建入口不用于当前模组发布。普通贴图直接使用 PNG/JPG，画质由这些实际运行图片决定，不再使用独立的小人 AB 替换包。

## 构建与安装

外层 `build.bat` 编译当前版本程序集，`deploy_modSelf.bat` 部署模组运行内容。Unity 工程无需安装到游戏目录，也不会被这些入口启动或改动。
