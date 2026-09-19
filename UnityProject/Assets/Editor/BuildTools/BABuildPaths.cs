using System.IO;
using UnityEngine;

//根据 Unity 工程的位置定位仓库、模组与构建输出，供全部资源打包入口共用。
internal static class BABuildPaths
{
    internal static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    internal static readonly string RepositoryRoot = Path.GetFullPath(Path.Combine(ProjectRoot, ".."));
    internal static readonly string ModRoot = ResolveModRoot();
    internal static readonly string BundleRoot = Path.Combine(ModRoot, "1.6/AssetBundles");
    internal static readonly string Pawn1024Root = Path.Combine(RepositoryRoot, "1024贴图包");
    internal static readonly string StagingRoot = Path.Combine(ProjectRoot, "Build/AssetBundles");

    //检查与 Unity 工程并列的模组目录，目录布局不正确时停止构建并报告路径。
    private static string ResolveModRoot()
    {
        string path = Path.Combine(RepositoryRoot, "bluearchive-newcentury");
        if (!File.Exists(Path.Combine(path, "About/About.xml")))
            throw new DirectoryNotFoundException("UnityProject 应与 bluearchive-newcentury 模组目录并列，找不到 About.xml：" + path);
        return path;
    }
}
