# UI 图片打包与内存配置

## 资源位置

Unity 工程位于仓库根目录的 `UnityProject`，与 Mod 文件夹并列。UI 图片源文件在 Unity 工程维护，Mod 工程不保留已经迁移的散图。完整目录映射见[贴图路径与 Unity 工程说明](../../贴图路径与Unity工程说明.md)。

| 图片用途 | Unity 工程内的位置 |
| --- | --- |
| 学生手册头像、立绘、背景、武器、标签 | `Assets/BAUIImages/Common/UIAssets/ManuaUI` |
| 抽卡头像及卡池图片 | `Assets/Data/Archive.NewWorld/Textures/Gacha` |
| 通用图标和商店图片 | `Assets/Data/Archive.NewWorld/Textures/UI`、`Stone` |
| 技能按钮图标 | `Assets/Data/Archive.NewWorld/Textures/Ability` |
| 青辉石图标 | `Assets/Data/Archive.NewWorld/Textures/QinghuiStone.png` |

界面预制体仍使用 `bamainui.ab`；本工具管理外部图片。特效图片使用自己的打包入口。

## 打包图片

在上述 Unity 工程中选择 **RimWorldTools → BA界面图片 → 打包全部UI图片**。此入口从 Unity 工程读取源图，把包输出到 Mod 工程，不复制到游戏安装目录。

首次把 Mod 中的图片转为 Unity 源文件时，选择 **迁移模组散图并打包**。只有构建和包内路径检查全部成功后，才移除 Mod 中与 Unity 源文件内容完全相同的散图。

也可在仓库根目录的 PowerShell 中执行，仅需指定本机编辑器位置：

```powershell
& './UnityProject/Build-AssetBundles.ps1' -Target UI `
  -UnityEditor '你的Unity安装目录/Editor/Unity.exe'
```

脚本自动定位同级 Mod，日志写入 `UnityProject/Logs/build-UI.log`，中间产物写入 `UnityProject/Build/AssetBundles/UIImages`。也可设置 `UNITY_EDITOR_PATH` 环境变量，省略 `-UnityEditor`。

以下内容需要一起发布：

- `1.6/AssetBundles/UIImage`：UI 专用图片包和共享图标包。
- `1.6/AssetBundles/banw_textures_ability` 及其 `.manifest`：原版可读取的技能图标包。
- `1.6/UI/images/bundles.xml`：由打包工具生成的图片索引，不手动编写包名。
- `1.6/UI/images/memory.xml`：内存配置。
- `1.6/Assemblies/BANWlLib.dll`：支持图片引用管理的代码。

立绘和背景按单图分包；小图按目录分包。图片保留原始尺寸、透明度，关闭 mipmap 和 CPU 像素副本，使用 LZ4 包压缩。

## XML 与散图配置

原有 Def 路径保持不变，无需改写为包内路径。例如：

```xml
<StudentAvatar>ManuaUI/Avatar/Shiroko</StudentAvatar>
<CharacterimagePath>ManuaUI/Live/Shiroko</CharacterimagePath>
<BackgroundPath>ManuaUI/Bg/BG_Abydos</BackgroundPath>
<avtTexPath>Gacha/Student/Shiroko</avtTexPath>
```

专用 UI 图片优先使用索引中的 AB；未打包或对应包不存在时，读取同路径散图。包存在但损坏或缺少声明的资源时会输出错误，不隐藏问题。原版共享图标通过 `ContentFinder` 读取，保留 RimWorld 的资源覆盖规则。

支持 PNG、JPG、JPEG，以及 DXT1/DXT5 格式的 DDS。散图路径对应关系：

| 配置路径 | Mod 内散图位置 |
| --- | --- |
| `ManuaUI/Avatar/MyStudent` | `Common/UIAssets/ManuaUI/Avatar/MyStudent.png` |
| `Gacha/Student/MyStudent` | `Common/Textures/Gacha/Student/MyStudent.png` |
| `Common/UIAssets/Custom/MyIcon` | `Common/UIAssets/Custom/MyIcon.png` |

自行添加且需要按需加载的 UI 图片建议放在 `Common/UIAssets`。放在 `Common/Textures` 的散图还会被 RimWorld 原版贴图系统读取，其生命周期由游戏管理。同一路径已有专用 AB 时，修改 Unity 源图并重新打包即可更新图片。

可以混合使用 AB 和散图；路径可带扩展名，也可省略。本模组内的绝对路径会与相对路径共用缓存。

## 内存设置

编辑 `1.6/UI/images/memory.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<UIImageMemory>
  <idleSeconds>30</idleSeconds>
  <idleMegabytes>32</idleMegabytes>
</UIImageMemory>
```

- `idleSeconds`：小图最后一次停止显示后，最多保留多少实际秒。游戏暂停时也会计时。
- `idleMegabytes`：闲置图片缓存的 MiB 上限。超出后优先释放最久未使用的图片；设为 `0` 可停止保留闲置图片。

设置在下一次 UI 完整重建或重新进入存档后读取。

图片只在控件实际显示时加载。滚出列表视口、控件禁用、页面隐藏或销毁时归还引用；多个控件使用同一图片时共用一份资源。立绘、背景及像素数不小于 524288 的大图，在最后一个引用释放后立即回收。小图受上述闲置限制管理，清理周期约一秒。

最后一张缓存图片被释放后，对应专用 AB 也会卸载。关闭或重建整套 UI 时会清理控件引用。原版技能、物品等共享图标仅回收 UI 精灵包装，不销毁游戏仍在使用的原版纹理。

缓存上限只约束闲置图片，不代表进程总内存上限。当前页面正在显示的图片、原版共享纹理、预制体内置资源及视频各自占用内存。

## 给界面绑定图片

代码创建 `UnityEngine.UI.Image` 后调用：

```csharp
BANWlLib.mainUI.Images.BAUIImageCache.SetImage(image, "ManuaUI/Avatar/Shiroko");
```

由控件组件自动取得和归还引用。切换图片时再次调用 `SetImage`；清空图片可传入 `null`。无需自行创建 `Sprite`，也无需手动销毁纹理。图片会在布局完成后的可见性检查中出现。

## 界面加载与语音

进入存档时只登记阿洛娜、普拉娜的对话配置。实际播放问候或点击语音时才异步读取对应 OGG，等待期间界面仍可操作；同一界面再次播放相同语音时使用缓存。关闭页面会取消尚未完成的播放，读档重建界面时释放这些语音。

在 `Player.log` 搜索 `[BA UI耗时]` 可以查看资源包、控件实例、商店、学生手册和任务界面的初始化耗时。`[BA UI语音]` 会报告缺失或无法解码的语音文件路径。

## 查看图片内存

开发者模式 → 调试操作菜单 → **BA调试 → UI内存** 提供：

- **输出图片内存**：输出已加载图片数量、显示引用、专用包数量，以及自有显示纹理、闲置纹理、原版共享纹理的内存统计。
- **释放闲置图片**：立即回收所有零引用缓存，保留仍在显示的图片。

打开学生详情后查看统计，再切换学生或关闭页面。大图应随显示引用归零而释放；滚动列表时只为可见条目取得引用，小图闲置后按配置回收。日志统计是纹理对象占用估算，不包含整个游戏、所有 AB 元数据或图形驱动的内存。
