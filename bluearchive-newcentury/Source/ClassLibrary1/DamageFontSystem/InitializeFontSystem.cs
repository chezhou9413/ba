using System.IO;
using UnityEngine;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    [StaticConstructorOnStartup]
    public static class InitializeFontSystem
    {
        static InitializeFontSystem()
        {
            string abPath = Path.Combine(LoadedModManager.GetMod<newpro>().Content.RootDir, "1.6", "AssetBundles", "damagefontsystem.ab");
            AssetBundle bundle = AssetBundle.LoadFromFile(abPath);
            FontDataBase.CriticalFont = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/CriticalFont.prefab");
            var CanvasObj = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GameObj/FontCanvas.prefab");
            FontDataBase.Canvas = GameObject.Instantiate(CanvasObj);
            GameObject.DontDestroyOnLoad(FontDataBase.Canvas);
        }
    }
}
