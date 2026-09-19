# 贴图路径与 Unity 工程说明

源图放在仓库中的 `UnityProject`。只查看或编辑 PNG 时，直接用文件管理器或图片编辑器打开即可，不需要解包 AssetBundle，也不需要启动 Unity。

## 仓库目录

以下路径均相对于本说明所在的仓库根目录：

```text
仓库根目录/
├─ bluearchive-newcentury/             RimWorld Mod，发布和安装使用这个目录
│  ├─ About/
│  ├─ Common/
│  ├─ 1.6/Defs/                        角色与技能 XML
│  ├─ 1.6/AssetBundles/                默认资源包，含 512 小人包
│  └─ 1.6/UI/images/bundles.xml        UI 图片路径索引
├─ UnityProject/                      独立 Unity 工程和图片源文件
│  ├─ Assets/
│  ├─ Packages/
│  ├─ ProjectSettings/
│  └─ Build-AssetBundles.ps1           统一构建入口
└─ 1024贴图包/                        可直接替换的 1024 小人包
```

下载仓库时保留上述相邻目录关系。仓库可以放在任意盘符或文件夹中，构建工具根据自身位置寻找 Mod 和输出目录。安装 Mod 时只使用 `bluearchive-newcentury` 子目录，Unity 工程无需放入游戏的 `Mods` 目录。

## 从 XML 找到图片

先在 `bluearchive-newcentury/1.6/Defs` 内找到角色或技能 XML，再查看 `ReplacePath`、`texPath`、`wornGraphicPath`、`iconPath`、`StudentAvatar`、`CharacterimagePath`、`avtTexPath` 等字段。

大多数配置填写的是不含扩展名的逻辑路径。不要把电脑盘符、`UnityProject` 或 `Assets` 前缀写进这些字段。

| XML 路径或用途 | 源图位置，相对于仓库根目录 |
| --- | --- |
| `Pawns/Abydos/Ayane/Body/Ayane_Body` | `UnityProject/Assets/Data/Archive.NewWorld/Textures/Pawns/Abydos/Ayane/Body/Ayane_Body_south.png` 等朝向文件 |
| `Pawns/Abydos/Ayane/Apparel/Ayane_Apparel` | `UnityProject/Assets/Data/Archive.NewWorld/Textures/Pawns/Abydos/Ayane/Apparel/` 内对应体型与朝向文件 |
| `Ability/Icon/Nero/Nero_EX` | `UnityProject/Assets/Data/Archive.NewWorld/Textures/Ability/Icon/Nero/Nero_EX.png` |
| `Gacha/Student/Shiroko` | `UnityProject/Assets/Data/Archive.NewWorld/Textures/Gacha/Student/Shiroko.png` |
| `ManuaUI/Avatar/Shiroko` | `UnityProject/Assets/BAUIImages/Common/UIAssets/ManuaUI/Avatar/Shiroko.png` |
| `ManuaUI/Live/Shiroko` | `UnityProject/Assets/BAUIImages/Common/UIAssets/ManuaUI/Live/Shiroko.png` |
| `Effect/Effect_Akane_A` | `UnityProject/Assets/EffectImages/Effect_Akane_A.png` |

普通贴图路径的对应规则是：

```text
XML：Pawns/学院/角色/部位/图片名
源图：UnityProject/Assets/Data/Archive.NewWorld/Textures/Pawns/学院/角色/部位/图片名.png
```

角色身体、头部、头发、服装等可能追加 `_north`、`_south`、`_east`、`_west`，服装还可能包含 `_Thin` 等体型后缀。先按 XML 中的名称前缀查找，然后以目录中实际文件为准，不要擅自给配置加上朝向后缀。部分角色没有独立的西向图片。

`Pawns` 下按学院和角色分目录，常见子目录有 `Body`、`Head`、`Hair`、`Apparel`、`Halo`。角色目录名和显示名称可能不同，应以 XML 路径为准。

`Assets/PawnImages` 是保留的旧素材副本。当前小人包读取 **`Assets/Data/Archive.NewWorld/Textures/Pawns`**，替换小人贴图应编辑这个目录。`Assets/EffImage`、`Assets/Pyimage` 等目录中的素材按具体材质引用使用，不能只凭同名文件判断它是否进入某个包。

## 用 UI 索引确认路径

打开 `bluearchive-newcentury/1.6/UI/images/bundles.xml`，搜索角色英文名或完整路径。每个 `Image` 条目记录：

| 字段 | 含义 |
| --- | --- |
| `key` | 标准化后的图片逻辑路径，不含扩展名 |
| `source` | 对应的 Mod 散图路径，用于定位和散图读取 |
| `asset` | Unity 包内的资源路径；在前面加上 `UnityProject/` 即可定位工程内文件 |
| `bundle` | 图片所在的资源包文件名 |

索引中的 `asset` 通常会转为小写，工程文件名可能保留大写。浏览仓库时按实际大小写打开文件。普通小人包可以查看 `1.6/AssetBundles/banw_pawns_win.manifest` 中的 `Assets` 清单，同样在资源路径前加 `UnityProject/` 即可定位源图。

UI 索引由构建工具生成。更新图片后重新打包，避免手动改包名或索引造成不一致。更多 UI 绑定和内存配置见 [UI 图片打包与内存配置](bluearchive-newcentury/Docs/UI图片打包与内存配置.md)。

## 搜索示例

在仓库根目录打开 PowerShell，安装了 `rg` 时可以执行：

```powershell
#查找白子的 XML 引用。
rg -n 'Shiroko|白子' './bluearchive-newcentury/1.6/Defs' -g '*.xml'

#按名称查找所有相关 PNG，结果包含原始文件路径。
rg --files './UnityProject/Assets' -g '*Shiroko*.png'

#通过 UI 索引确认头像和立绘对应的包内路径。
rg -n 'ManuaUI/(Avatar|Live)/Shiroko' './bluearchive-newcentury/1.6/UI/images/bundles.xml'
```

没有命令行工具时，在文件管理器中搜索图片名，或在 Unity 的 Project 窗口搜索角色英文名。需要检查尺寸时，在图片属性中查看像素宽高；Unity 的导入上限和包内最终尺寸可能小于源图。

## 打开和构建 Unity 工程

在 Unity Hub 中添加 `UnityProject` 文件夹。使用 `ProjectSettings/ProjectVersion.txt` 中的编辑器版本，当前为 **2022.3.35f1c1**。首次打开会导入素材并恢复 Packages 依赖；查看贴图文件本身不需要这些步骤。

在仓库根目录执行以下命令，将示例编辑器路径替换为本机路径：

```powershell
$unity = '你的Unity安装目录/Editor/Unity.exe'

#仅编译编辑器脚本。
& './UnityProject/Build-AssetBundles.ps1' -Target Compile -UnityEditor $unity

#用同一批源图同时更新默认 512 包和独立 1024 包。
& './UnityProject/Build-AssetBundles.ps1' -Target PawnsBoth -UnityEditor $unity

#更新 UI 图片包和路径索引。
& './UnityProject/Build-AssetBundles.ps1' -Target UI -UnityEditor $unity
```

也可以先设置 `$env:UNITY_EDITOR_PATH = $unity`，后续命令省略 `-UnityEditor`。各入口都根据脚本位置解析工程路径，不依赖终端当前目录；上面的 `./UnityProject/...` 命令示例以仓库根目录为起点。

| `-Target` | 输出 |
| --- | --- |
| `Compile` | 编译编辑器脚本，不生成资源包 |
| `Pawns512` | `bluearchive-newcentury/1.6/AssetBundles/banw_pawns_win` |
| `Pawns1024` | `1024贴图包/banw_pawns_win` |
| `PawnsBoth` | 同时输出上述两份小人包 |
| `UI` | Mod 的 `1.6/AssetBundles/UIImage`、技能图标包和 `1.6/UI/images/bundles.xml` |
| `Textures` | Mod 的 `1.6/AssetBundles/banw_textures_*` 通用目录包 |
| `Effects` | Mod 的 `Common/Textures/Effect` 特效图片包 |

UI 图片使用 `UI` 入口，小人使用 `Pawns512`、`Pawns1024` 或 `PawnsBoth`。`Textures` 用于通用目录贴图，不代替 UI 图片索引构建。也可通过 Unity 菜单 `RimWorldTools` 中对应的分类打包。

日志保存在 `UnityProject/Logs/build-目标名.log`；小人双版本和 UI 的中间产物在 `UnityProject/Build/AssetBundles`。构建只更新仓库内的包，不复制到游戏安装目录。1024 包的覆盖方式见 [1024 贴图包使用说明](1024贴图包/使用说明.md)。

## 替换图片与提交文件

直接替换对应源图，保留原路径和文件名。Unity 的 `.meta` 文件记录资源 GUID 和导入设置，应与素材一起保留、提交；移动素材时优先在 Unity 的 Project 窗口中移动，避免引用丢失。

更新源图后，需要使用对应入口重新打包，游戏才能读取修改。小人 512 和 1024 包共用同一份源图，区别在导入分辨率上限。需要保持两份包内容同步时使用 `PawnsBoth`。

`Assets`、`.meta`、`Packages`、`ProjectSettings` 和构建脚本纳入 Git。`Library`、`Temp`、`Logs`、`Build`、`UserSettings` 及生成的 IDE 工程不提交。
