#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using System.IO;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
using Verse;

namespace BANWlLib.DamageFontSystem
{
    //缓存飘字模板，运行时画布按引擎的场景生命周期创建。
    [StaticConstructorOnStartup]
    public static class InitializeFontSystem
    {
#if RW_1_7
        private static GameObject canvasPrefab;
        private static Game canvasGame;
#endif

        //启动时加载模板，RimUnity 的场景实例留到当前存档内创建。
        static InitializeFontSystem()
        {
#if RW_1_7
            AssetBundle bundle = AssetBundle.Open();
#else
            string abPath = Path.Combine(LoadedModManager.GetMod<newpro>().Content.RootDir, "1.6", "AssetBundles", "damagefontsystem.ab");
            AssetBundle bundle = AssetBundle.LoadFromFile(abPath);
#endif
            FontDataBase.CriticalFont = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/CriticalFont.prefab");
#if RW_1_7
            canvasPrefab = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/FontCanvas.prefab");
#else
            var canvasObj = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/FontCanvas.prefab");
            FontDataBase.Canvas = GameObject.Instantiate(canvasObj);
            GameObject.DontDestroyOnLoad(FontDataBase.Canvas);
#endif
        }

#if RW_1_7
        //显示入口已确认处于主线程和当前地图，切换存档时丢弃旧场景的池对象。
        internal static void EnsureCurrentGame()
        {
            if (ReferenceEquals(canvasGame, Current.Game) && FontDataBase.Canvas != null)
            {
                return;
            }

            CriticalObjPool.Criticalpool.Clear();
            if (FontDataBase.Canvas != null)
            {
                GameObject.DestroyImmediate(FontDataBase.Canvas);
            }

            FontDataBase.Canvas = GameObject.Instantiate(canvasPrefab);
            Canvas canvas = FontDataBase.Canvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1000;
            canvasGame = Current.Game;
        }
#endif
    }
}
