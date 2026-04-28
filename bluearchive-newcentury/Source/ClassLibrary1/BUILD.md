# bluearchive-newcentury 本地开发说明

## 目录职责

- `E:\RimModDev\bluearchive-newcentury` 是外层开发目录，保存构建和部署入口。
- `E:\RimModDev\bluearchive-newcentury\bluearchive-newcentury` 是 RimWorld 实际加载的内层 Mod 目录。
- `bluearchive-newcentury\Source\ClassLibrary1\BANWlLib.csproj` 负责主库 C# 源码编译。
- `1.6\Assemblies` 保存主库 `BANWlLib.dll` 和项目附带的运行库 DLL。
- `Common` 和 `Sounds` 保存公共资源与音频资源。
- `deploy_modSelf.bat` 负责把内层 Mod 目录同步到 RimWorld Mods 目录。

## 编译

在外层开发目录运行：

```bat
build.bat
```

需要 Release 构建时运行：

```bat
build.bat Release
```

## 部署

在外层开发目录运行：

```bat
deploy_modSelf.bat
```

默认部署位置：

```text
E:\huanshijie\1.6\RimWorld\Mods\bluearchive-newcentury
```

## 汇总文件

在内层 Mod 目录运行：

```bat
Abstract.bat
Abstract_New.bat
```