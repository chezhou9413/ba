using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using RimUnity;
using RimUnity.UI;
using RimUnity.U2D;
using BANWlLib.mainUI.Gaka.MonoComp;
using newpro;

namespace BANWlLib.mainUI.Gaka
{
    //抽卡展示资源在真正抽取前加载，避免特效错误阻断主界面按钮初始化。
    public static partial class Gakalord
    {
        //整组资源全部成功后才标记可用，失败由本次抽卡入口报告。
        private static void LoadPresentationAssets(AssetBundle bundle)
        {
            if (GakaMapData.presentationAssetsLoaded) return;
            GakaMapData.GakaAnimationPofab = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/GakaAnimation.prefab");
            GakaMapData.cardZheng = bundle.LoadAsset<SpriteAtlas>("Assets/Scenes/Resources/Gaka/abImage/card.spriteatlas").GetSprite("cardzheng_0");
            GakaMapData.cardFan = bundle.LoadAsset<SpriteAtlas>("Assets/Scenes/Resources/Gaka/abImage/card.spriteatlas").GetSprite("cardfan_0");
            GakaMapData.showGaList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/showGaList.prefab");
            GakaMapData.ResultList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/ResultList.prefab");

            Texture2D texture1 = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/FX_TEX_GT_RCard_1.png");
            Texture2D texture2 = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/FX_TEX_GT_SRCard.png");
            Texture2D texture3 = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/FX_TEX_GT_SSRCard.png");

            GakaMapData.studentcard1star = Sprite.Create(texture1, new Rect(0, 0, texture1.width, texture1.height), new Vector2(0.5f, 0.5f));
            GakaMapData.studentcard2star = Sprite.Create(texture2, new Rect(0, 0, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
            GakaMapData.studentcard3star = Sprite.Create(texture3, new Rect(0, 0, texture3.width, texture3.height), new Vector2(0.5f, 0.5f));
            Texture2D tex; //复用变量读取卡面纹理。
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/gtwenhaoback.png");
            GakaMapData.gtwenhaoback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/srwenhaoback.png");
            GakaMapData.srwenhaoback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/ssrwenhaoback.png");
            GakaMapData.srrwenhaoback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/gtback.png");
            GakaMapData.gtback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/srback.png");
            GakaMapData.srback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/srrback.png");
            GakaMapData.srrback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/gtstar.png");
            GakaMapData.gtstar = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/srstar.png");
            GakaMapData.srstar = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            tex = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/srrstar.png");
            GakaMapData.srrstar = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            GakaMapData.presentationAssetsLoaded = true;
        }

        //在扣费之前创建过场、设置排序并绑定控件，失败时清理本次实例。
        private static MonoComp_GakaAnimationPofab PrepareAnimation(bool tenGacha)
        {
            try
            {
                LoadPresentationAssets(UiMapData.bundle);
            }
            catch
            {
                GakaMapData.ReleasePresentationAssets();
                throw;
            }
            GameObject game = GameObject.Instantiate(GakaMapData.GakaAnimationPofab);
            try
            {
                game.SetActive(false);
                game.GetComponent<Canvas>().worldCamera = UiMapData.mainUI.GetComponent<Canvas>().worldCamera;
                game.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                BANWlLib.mainUI.Initialization.BAFullscreenLayout.ConfigureGacha(game);
                MonoComp_GakaAnimationPofab animation = game.AddComponent<MonoComp_GakaAnimationPofab>();
                animation.Initialize(tenGacha);
                return animation;
            }
            catch
            {
                game.SetActive(false);
                GameObject.Destroy(game);
                throw;
            }
        }
    }
}
