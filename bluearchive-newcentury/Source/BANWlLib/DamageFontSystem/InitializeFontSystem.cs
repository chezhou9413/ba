using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using System.IO;
using RimUnity;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    //缓存飘字模板，运行时画布按引擎的场景生命周期创建。
    [StaticConstructorOnStartup]
    public static class InitializeFontSystem
    {
        private static GameObject canvasPrefab;
        private static Game canvasGame;

        //启动时加载模板，RimUnity 的场景实例留到当前存档内创建。
        static InitializeFontSystem()
        {
            AssetBundle bundle = AssetBundle.Open();
            FontDataBase.CriticalFont = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/CriticalFont.prefab");
            canvasPrefab = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/FontCanvas.prefab");
        }

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
    }
}
