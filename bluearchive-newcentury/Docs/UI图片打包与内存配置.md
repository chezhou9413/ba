# UI 图片与内存配置

## 资源位置

运行图片由 RimUnity 清单和普通散图提供。制作源图仍在独立的 Unity 工程中，路径映射见[贴图路径与 Unity 工程说明](../../贴图路径与Unity工程说明.md)。

| 内容 | 模组内位置 |
| --- | --- |
| 预制体、材质、粒子与专用图片 | `1.7/Data/RimUnity` |
| 学生图片路径索引 | `1.7/UI/images/bundles.xml` |
| 图片缓存设置 | `1.7/UI/images/memory.xml` |
| 普通游戏贴图 | `Common/Textures`、`1.7/Textures` |
| 直接读取的界面散图 | `Common/UIAssets` |
| 图片管理代码 | `Source/BANWlLib/mainUI/Images` |

## 导出图片

模组维护的 `Tools/Build/BAImageRimUnityExport.cs` 从图片索引和制作工程源图生成 RimUnity 图片入口；界面导出工具为 `BAUiRimUnityProbe.cs`，源资源路径位于 `UIExportAssets.txt`。导出内容经 `BAAssetPublish.cs` 整理后发布到 `1.7/Data/RimUnity`。

当前运行时不读取 Unity AB 包。图片索引中的 `bundle` 作为缓存分组标识保留，实际图片通过 `UIImage/` 逻辑路径加载。普通 Def 图片可使用 `Tools/Build/Sync-GameTextures.ps1` 同步。

Unity 工程内的现有文件与旧构建菜单保持原样；这里的导出工具是模组内维护的副本。

## XML 与散图配置

原有 Def 路径保持不变，无需改写为包内路径。例如：

```xml
<StudentAvatar>ManuaUI/Avatar/Shiroko</StudentAvatar>
<CharacterimagePath>ManuaUI/Live/Shiroko</CharacterimagePath>
<BackgroundPath>ManuaUI/Bg/BG_Abydos</BackgroundPath>
<avtTexPath>Gacha/Student/Shiroko</avtTexPath>
```

专用 UI 图片按索引从 RimUnity 清单加载，索引未收录的图片可以使用散图。共享图标优先通过 `ContentFinder` 读取，保留 RimWorld 的资源覆盖规则；缺少声明的资源时报告实际加载错误。

支持 PNG、JPG、JPEG，以及 DXT1/DXT5 格式的 DDS。散图路径对应关系：

| 配置路径 | Mod 内散图位置 |
| --- | --- |
| `ManuaUI/Avatar/MyStudent` | `Common/UIAssets/ManuaUI/Avatar/MyStudent.png` |
| `Gacha/Student/MyStudent` | `Common/Textures/Gacha/Student/MyStudent.png` |
| `Common/UIAssets/Custom/MyIcon` | `Common/UIAssets/Custom/MyIcon.png` |

自行添加且需要按需加载的 UI 图片建议放在 `Common/UIAssets`。放在 `Common/Textures` 的散图还会被 RimWorld 原版贴图系统读取，其生命周期由游戏管理。同一路径已有 RimUnity 图片入口时，应更新对应导出资源。

可以混合使用 RimUnity 资源和散图；路径可带扩展名，也可省略。本模组内的绝对路径会与相对路径共用缓存。

## 内存设置

编辑 `1.7/UI/images/memory.xml`：

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

图片只在控件实际显示时加载。滚出列表视口、控件禁用、页面隐藏或销毁时归还引用；多个控件使用同一图片时共用一份资源。立绘、背景和索引中标记为 transient 的图片，在最后一个引用释放后立即回收。小图受上述闲置限制管理，清理周期约一秒。

自有图片释放时销毁对应的 RimUnity 纹理，并更新图片分组引用计数。关闭或重建整套 UI 时会清理控件引用。原版技能、物品等共享图标仅回收 UI 精灵包装，不销毁游戏仍在使用的原版纹理。

缓存上限只约束闲置图片，不代表进程总内存上限。当前页面正在显示的图片、原版共享纹理、预制体内置资源及视频各自占用内存。

## 给界面绑定图片

代码创建 `RimUnity.UI.Image` 后调用：

```csharp
BANWlLib.mainUI.Images.BAUIImageCache.SetImage(image, "ManuaUI/Avatar/Shiroko");
```

由控件组件自动取得和归还引用。切换图片时再次调用 `SetImage`；清空图片可传入 `null`。无需自行创建 `Sprite`，也无需手动销毁纹理。图片会在布局完成后的可见性检查中出现。

## 界面加载与语音

进入存档时只登记阿洛娜、普拉娜的对话配置。实际播放问候或点击语音时才异步读取对应 OGG，等待期间界面仍可操作；同一界面再次播放相同语音时使用缓存。关闭页面会取消尚未完成的播放，读档重建界面时释放这些语音。

在 `Player.log` 搜索 `[BA UI耗时]` 可以查看资源包、控件实例、商店、学生手册和任务界面的初始化耗时。`[BA UI语音]` 会报告缺失或无法解码的语音文件路径。

## 1.7 UI 着色器导出

抽卡的 Panda UI 材质关闭了深度淡出、屏幕折射和静态法线偏移。重新导出 RimUnity 资源后，在模组目录执行 `python Tools/Build/Optimize-UiShaderExports.py`，使用本地 RimUnity ShaderTool 编译检查后生成专用变体。脚本会检查材质值和动画；如果重新启用了这些功能，会中止并提示使用通用 Shader。

专用变体不声明场景抓取，也不采样地表深度，使用独立的程序键避免与其他模组共用缓存。COST 与抽卡 Shader 的零宽度 `smoothstep` 按硬边界计算，普通和反向边界保持插值。现有的颜色、溶解、贴图和遮罩仍沿用原计算。

## 查看图片内存

开发者模式 → 调试操作菜单 → **BA调试 → UI内存** 提供：

- **输出图片内存**：输出已加载图片数量、显示引用、图片分组数量，以及自有显示纹理、闲置纹理、原版共享纹理的内存统计。
- **释放闲置图片**：立即回收所有零引用缓存，保留仍在显示的图片。

打开学生详情后查看统计，再切换学生或关闭页面。大图应随显示引用归零而释放；滚动列表时只为可见条目取得引用，小图闲置后按配置回收。日志统计是纹理对象占用估算，不包含整个游戏、所有资源清单元数据或图形驱动的内存。
