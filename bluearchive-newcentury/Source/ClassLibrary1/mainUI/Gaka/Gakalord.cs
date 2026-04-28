using BANWlLib.BaDef;
using BANWlLib.BANWGamecomp;
using BANWlLib.mainUI.Gaka.MonoComp;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.uicreater.tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.Video;
using Verse;

namespace BANWlLib.mainUI.Gaka
{
    public static class Gakalord
    {
        public static void lordGaka(AssetBundle bundle)
        {
            if (bundle == null)
            {
                Log.Error("传入的 AssetBundle 为空！");
                return;
            }
            if (Current.Game != null)
            {
                GakaMapData.gamecomp_GakaAction = Current.Game.GetComponent<Gamecomp_gakaAction>();
            }
            Transform contentTrans = GakaMapData.GakaUIPet.transform.Find("MainBack/cardHead/Viewport/Content");
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Poit").gameObject.AddComponent<MonoComp_updataPoit>();
            GakaMapData.Content = contentTrans.gameObject;
            GakaMapData.GachaPofab = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/GakaHead.prefab");
            GakaMapData.GakaAnimationPofab = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/GakaAnimation.prefab");
            GakaMapData.cardZheng = bundle.LoadAsset<SpriteAtlas>("Assets/Scenes/Resources/Gaka/abImage/card.spriteatlas").GetSprite("cardzheng_0");
            GakaMapData.cardFan = bundle.LoadAsset<SpriteAtlas>("Assets/Scenes/Resources/Gaka/abImage/card.spriteatlas").GetSprite("cardfan_0");
            GakaMapData.showGaList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/showGaList.prefab");
            GakaMapData.ResultList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/ResultList.prefab");
            GakaMapData.ShotMaskBack = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/ShotMaskBack.prefab");
            GakaMapData.GakaShotList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/GakaShotList.prefab");

            Texture2D texture1 = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/FX_TEX_GT_RCard_1.png");
            Texture2D texture2 = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/FX_TEX_GT_SRCard.png");
            Texture2D texture3 = bundle.LoadAsset<Texture2D>("Assets/Scenes/Resources/Gaka/abImage/FX_TEX_GT_SSRCard.png");

            GakaMapData.studentcard1star = Sprite.Create(texture1, new Rect(0, 0, texture1.width, texture1.height), new Vector2(0.5f, 0.5f));
            GakaMapData.studentcard2star = Sprite.Create(texture2, new Rect(0, 0, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
            GakaMapData.studentcard3star = Sprite.Create(texture3, new Rect(0, 0, texture3.width, texture3.height), new Vector2(0.5f, 0.5f));
            Texture2D tex; // 临时变量用于存储加载的图片
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
            SettingButtomData();
        }

        public static void SettingButtomData()
        {
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Button_one").GetComponent<Button>().onClick.AddListener(() =>
            {
                LoopBGMManager.playEffAudio("鼠标点击音效");
                int cost = 120;

                // 上帝模式：免费抽卡
                if (Prefs.DevMode && DebugSettings.godMode)
                {
                    BamessageUI.ShowBaMessageUIQuek("提示", "【上帝模式】是否进行一次单抽？", "确认", "取消", () =>
                    {
                        UiMapData.isLocKBack = true;
                        danchou();
                    });
                    return;
                }

                int current = ItemUtility.GetTotalItemCount("QinghuiStone");
                if (current < cost)
                {
                    BamessageUI.ShowBaMessageUI("提示", $"青辉石不足！\n需要：{cost}\n当前：{current}", "确认");
                    return;
                }
                BamessageUI.ShowBaMessageUIQuek("提示", $"是否花费{cost}青辉石进行一次单抽？\n当前持有：{current}", "确认", "取消", () =>
                {
                    if (ItemUtility.TryRemoveItem("QinghuiStone", cost))
                    {
                        UiMapData.isLocKBack = true;
                        danchou();
                    }
                    else
                    {
                        BamessageUI.ShowBaMessageUI("提示", "青辉石不足，扣除失败！", "确认");
                    }
                });
            });

            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Button_ten").GetComponent<Button>().onClick.AddListener(() =>
            {
                LoopBGMManager.playEffAudio("鼠标点击音效");
                int cost = 1200;

                // 上帝模式：免费抽卡
                if (Prefs.DevMode && DebugSettings.godMode)
                {
                    BamessageUI.ShowBaMessageUIQuek("提示", "【上帝模式】是否进行一次十连抽？", "确认", "取消", () =>
                    {
                        UiMapData.isLocKBack = true;
                        shilianchou();
                    });
                    return;
                }

                int current = ItemUtility.GetTotalItemCount("QinghuiStone");
                if (current < cost)
                {
                    BamessageUI.ShowBaMessageUI("提示", $"青辉石不足！\n需要：{cost}\n当前：{current}", "确认");
                    return;
                }
                BamessageUI.ShowBaMessageUIQuek("提示", $"是否花费{cost}青辉石进行一次十连抽？\n当前持有：{current}", "确认", "取消", () =>
                {
                    if (ItemUtility.TryRemoveItem("QinghuiStone", cost))
                    {
                        UiMapData.isLocKBack = true;
                        shilianchou();
                    }
                    else
                    {
                        BamessageUI.ShowBaMessageUI("提示", "青辉石不足，扣除失败！", "确认");
                    }
                });
            });
        }

        public static void SelectStu(string defname)
        {
            BaStudentRaceDef thingDefs = DefDatabase<BaStudentRaceDef>.GetNamed(defname);
            ExtGakaData(thingDefs);
        }
        public static void danchou()
        {
            GakaMapData.gamecomp_GakaAction.updataGacaPoit(1);

            ThingDef thingDefs;

            // 判断是否为FES池
            if (GakaMapData.selectGachaDef.isFes)
            {
                thingDefs = GachaSystem.DrawFES(
                    GakaMapData.selectGachaDef.oneStarPool.RaceList,
                    GakaMapData.selectGachaDef.twoStarPool.RaceList,
                    GakaMapData.selectGachaDef.FESthreeStarPool.RaceList,
                    GakaMapData.selectGachaDef.FESupthreeStarPool.RaceList,
                    GakaMapData.selectGachaDef.threeStarPool.RaceList
                );
            }
            else
            {
                thingDefs = GachaSystem.Draw(
                    GakaMapData.selectGachaDef.oneStarPool.RaceList,
                    GakaMapData.selectGachaDef.twoStarPool.RaceList,
                    GakaMapData.selectGachaDef.threeStarPool.RaceList,
                    GakaMapData.selectGachaDef.upthreeStarPool.RaceList,
                    GakaMapData.selectGachaDef.oneStarPool.Weight,
                    GakaMapData.selectGachaDef.twoStarPool.Weight,
                    GakaMapData.selectGachaDef.threeStarPool.Weight,
                    GakaMapData.selectGachaDef.upthreeStarPool.Weight
                );
            }

            ExtGakaData(thingDefs);
            GameObject game = GameObject.Instantiate(GakaMapData.GakaAnimationPofab);
            game.GetComponent<Canvas>().worldCamera = UiMapData.mainUI.GetComponent<Canvas>().worldCamera;
            game.AddComponent<MonoComp_GakaAnimationPofab>();
        }

        public static void shilianchou()
        {
            GakaMapData.gamecomp_GakaAction.updataGacaPoit(10);

            List<ThingDef> thingDefs;

            // 判断是否为FES池
            if (GakaMapData.selectGachaDef.isFes)
            {
                thingDefs = GachaSystem.MultiDrawFES10(
                    GakaMapData.selectGachaDef.oneStarPool.RaceList,
                    GakaMapData.selectGachaDef.twoStarPool.RaceList,
                    GakaMapData.selectGachaDef.FESthreeStarPool.RaceList,
                    GakaMapData.selectGachaDef.FESupthreeStarPool.RaceList,
                    GakaMapData.selectGachaDef.threeStarPool.RaceList
                );
            }
            else
            {
                thingDefs = GachaSystem.MultiDraw10(
                    GakaMapData.selectGachaDef.oneStarPool.RaceList,
                    GakaMapData.selectGachaDef.twoStarPool.RaceList,
                    GakaMapData.selectGachaDef.threeStarPool.RaceList,
                    GakaMapData.selectGachaDef.upthreeStarPool.RaceList,
                    GakaMapData.selectGachaDef.oneStarPool.Weight,
                    GakaMapData.selectGachaDef.twoStarPool.Weight,
                    GakaMapData.selectGachaDef.threeStarPool.Weight,
                    GakaMapData.selectGachaDef.upthreeStarPool.Weight
                );
            }

            string results = string.Join(", ", thingDefs.Select(d => d.label.ToString()));
            ExtGakaData(thingDefs);
            GameObject game = GameObject.Instantiate(GakaMapData.GakaAnimationPofab);
            game.GetComponent<Canvas>().worldCamera = UiMapData.mainUI.GetComponent<Canvas>().worldCamera;
            game.AddComponent<MonoComp_GakaAnimationPofab>();
        }
        public static gacaData creatGacaData(BaStudentRaceDef badef)
        {
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            gacaData gacaData = new gacaData();
            gacaData.BaStudentRaceDef = badef;
            gacaData.starNum = badef.BaStudentUI != null ? badef.BaStudentUI.CharacterStarCount : 1;
            Texture2D texture = null;
            string path = badef.baStudentData.avtTexPath;
            if (!string.IsNullOrEmpty(path))
            {
                texture = ContentFinder<Texture2D>.Get(path);
            }
            if (texture == null)
            {
                texture = BaseContent.BadTex;
            }
            gacaData.gakaAvt = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            if (!BANWlLib.Tool.StudentRosterUtility.IsStudentDef(tracker, badef.defName))
            {
                tracker.HaveStudent.Add(new BANWlLib.mainUI.pojo.StudentData(badef.defName));
                gacaData.isNew = true;
            }
            else
            {
                gacaData.isNew = false;
            }
            var upPool = GakaMapData.selectGachaDef?.upthreeStarPool?.RaceList;
            gacaData.isUp = upPool != null && upPool.Contains(badef);
            return gacaData;
        }
        public static void ExtGakaData(List<ThingDef> extlist)
        {
            GakaMapData.gacaDatas.Clear();
            GakaMapData.tenGacha = true;
            foreach (ThingDef thingDef in extlist)
            {
                if (thingDef is BaStudentRaceDef badef)
                {
                    gacaData gacaData = creatGacaData(badef);
                    GakaMapData.gacaDatas.Add(gacaData);
                }
            }
            spawStudentThing();
        }

        public static void ExtGakaData(ThingDef extlist)
        {
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            GakaMapData.gacaDatas.Clear();
            GakaMapData.tenGacha = false;
            if (extlist is BaStudentRaceDef badef)
            {
                gacaData gacaData = creatGacaData(badef);
                GakaMapData.gacaDatas.Add(gacaData);
            }
            spawStudentThing();
        }

        public static void spawStudentThing()
        {
            Map map = Find.CurrentMap;
            foreach (gacaData gacaData in GakaMapData.gacaDatas)
            {
                if (gacaData.isUp)
                {
                    if (gacaData.isNew)
                    {
                        foreach (KeyValuePair<ThingDef, int> kvp in gacaData.BaStudentRaceDef.baStudentData.OneGakaStudentThingData)
                        {
                            Thing thing = ThingMaker.MakeThing(kvp.Key);
                            thing.stackCount = kvp.Value;
                            PawnDropHelper.DropThing(map, thing);
                        }
                    }
                    else if (!gacaData.isNew)
                    {
                        foreach (KeyValuePair<ThingDef, int> kvp in gacaData.BaStudentRaceDef.baStudentData.UpGakaStudentThingData)
                        {
                            Thing thing = ThingMaker.MakeThing(kvp.Key);
                            thing.stackCount = kvp.Value;
                            PawnDropHelper.DropThing(map, thing);
                        }
                    }
                }
                else
                {
                    if (!gacaData.isNew)
                    {
                        foreach (KeyValuePair<ThingDef, int> kvp in gacaData.BaStudentRaceDef.baStudentData.GakaStudentThingData)
                        {
                            Thing thing = ThingMaker.MakeThing(kvp.Key);
                            thing.stackCount = kvp.Value;
                            PawnDropHelper.DropThing(map, thing);
                        }
                    }
                }
            }
            GakaMapData.isP3 = false;
            foreach (gacaData gacaData in GakaMapData.gacaDatas)
            {
                if(gacaData.BaStudentRaceDef.baStudentData.StarCont == 3)
                {
                    GakaMapData.isP3 = true;
                }
            }
        }
        public static void OpenGakaUI()
        {
            // 默认选中第一个卡池（优先固定池，没有则取随机池第一个）
            if (GakaMapData.selectGachaDef == null)
            {
                var fixedPool = GakaMapData.gamecomp_GakaAction.gachaSetting?.FixedPool;
                if (!fixedPool.NullOrEmpty())
                {
                    GakaMapData.selectGachaDef = fixedPool[0];
                }
                else if (!GakaMapData.gamecomp_GakaAction.CurrentDisplayPool.NullOrEmpty())
                {
                    GakaMapData.selectGachaDef = GakaMapData.gamecomp_GakaAction.CurrentDisplayPool[0];
                }
            }
            creategakaHead();
            SwitchGacha(GakaMapData.selectGachaDef);
        }

        public static void creategakaHead()
        {
            GakaMapData.GachaType.Clear();
            Transform parentTrans = GakaMapData.Content.transform;

            // 清空旧的子物体
            for (int i = parentTrans.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parentTrans.GetChild(i).gameObject);
            }

            var gameComp = GakaMapData.gamecomp_GakaAction;
            var fixedPool = gameComp.gachaSetting?.FixedPool ?? new List<Gacha>();
            var displayPool = gameComp.CurrentDisplayPool ?? new List<Gacha>();

            // 1. 先渲染固定卡池（始终在前）
            foreach (Gacha gacha in fixedPool)
            {
                CreateGachaButton(gacha);
            }

            // 2. 再渲染随机卡池（排除已在固定池中的）
            foreach (Gacha gacha in displayPool)
            {
                if (!fixedPool.Contains(gacha))
                {
                    CreateGachaButton(gacha);
                }
            }
        }

        private static void CreateGachaButton(Gacha gacha)
        {
            GameObject gakaHead = GameObject.Instantiate(GakaMapData.GachaPofab);
            gakaHead.GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(gacha.gachaTexPath));
            gakaHead.GetComponent<Button>().onClick.AddListener(delegate
            {
                GakaMapData.selectGachaDef = gacha;
                SwitchGacha(gacha);
            });
            gakaHead.transform.SetParent(GakaMapData.Content.transform, false);
        }

        public static void SwitchGacha(Gacha gacha)
        {
            if (gacha == null) return;

            GakaMapData.selectGachaDef = gacha;
            string path = "\\Common\\Textures\\";
            string fpath = (UiMapData.modRootPath + path + gacha.gachaVidPath).Replace("/", "\\");
            VideoPlayer videoPlayer = GakaMapData.GakaUIPet.transform.Find("MainBack/vid").GetComponent<VideoPlayer>();
            PlayLocalVideo(videoPlayer, fpath);
            SettingGakaTextForRandPool(gacha);
            MonoComp_updataPoit.instance?.RefreshShopList();
        }
        public static void PlayLocalVideo(VideoPlayer videoPlayer, string videoPath)
        {
            if (videoPlayer == null || string.IsNullOrEmpty(videoPath)) return;
            if (!System.IO.File.Exists(videoPath))
            {
                Log.Error("[VideoPlayer] 路径不存在: " + videoPath);
                return;
            }
            videoPlayer.Stop();
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoPath;
            videoPlayer.isLooping = true;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }

        private static void OnVideoPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= OnVideoPrepared;
            source.Play();
        }

        public static void SettingGakaTextForRandPool(Gacha gacha)
        {
            var gameComp = GakaMapData.gamecomp_GakaAction;
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_time").GetComponent<UnityEngine.UI.Text>().text =
                gameComp.GetRemainingTimeString(gameComp.RotationTickCounter);

            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_title").GetComponent<UnityEngine.UI.Text>().text = gacha.gachaTitle;
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_up").GetComponent<UnityEngine.UI.Text>().text = gacha.gachaUp;
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_dex").GetComponent<UnityEngine.UI.Text>().text = gacha.gachaDesc;
        }
    }
}
