#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.BaDef;
using BANWlLib.BANWGamecomp;
using BANWlLib.mainUI.Gaka.MonoComp;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using BANWlLib.uicreater.tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using RimUnity.U2D;
#else
using UnityEngine.U2D;
#endif
#if RW_1_7
using RimUnity.UI;
#else
using UnityEngine.UI;
#endif
#if RW_1_7
using RimUnity.Video;
#else
using UnityEngine.Video;
#endif
using Verse;

namespace BANWlLib.mainUI.Gaka
{
    //协调卡池选择、抽取、结果生成及抽卡界面显示。
    public static partial class Gakalord
    {
        //加载抽卡界面模板并初始化卡池控件。
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
            GakaMapData.ShotMaskBack = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/ShotMaskBack.prefab");
            GakaMapData.GakaShotList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/GakaObj/GakaShotList.prefab");

            SettingButtomData();
        }

        //绑定入口事件，过场资源在确认抽卡后、扣费前准备。
        public static void SettingButtomData()
        {
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Button_one").GetComponent<Button>().onClick.AddListener(() => ConfirmDraw(false));
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Button_ten").GetComponent<Button>().onClick.AddListener(() => ConfirmDraw(true));
        }

        //根据学生标识执行指定抽取流程。
        public static void SelectStu(string defname)
        {
            BaStudentDef studentDef;
            StudentIdentityUtility.TryGetStudentDef(defname, out studentDef);
            ExtGakaData(studentDef);
        }
        //按当前卡池生成单抽结果，尚不扣费或结算。
        private static BaStudentDef DrawOne()
        {
            BaStudentDef thingDefs;

            //FES池单抽时把常驻三星池和其他FES三星池分开传入，避免三星分流错位
            if (GakaMapData.selectGachaDef.isFes)
            {
                thingDefs = GachaSystem.DrawFES(
                    GakaMapData.selectGachaDef.oneStarPool.StudentList,
                    GakaMapData.selectGachaDef.twoStarPool.StudentList,
                    GakaMapData.selectGachaDef.threeStarPool.StudentList,
                    GakaMapData.selectGachaDef.FESupthreeStarPool.StudentList,
                    GakaMapData.selectGachaDef.FESthreeStarPool.StudentList
                );
            }
            else
            {
                thingDefs = GachaSystem.Draw(
                    GakaMapData.selectGachaDef.oneStarPool.StudentList,
                    GakaMapData.selectGachaDef.twoStarPool.StudentList,
                    GakaMapData.selectGachaDef.threeStarPool.StudentList,
                    GakaMapData.selectGachaDef.upthreeStarPool.StudentList,
                    GakaMapData.selectGachaDef.oneStarPool.Weight,
                    GakaMapData.selectGachaDef.twoStarPool.Weight,
                    GakaMapData.selectGachaDef.threeStarPool.Weight,
                    GakaMapData.selectGachaDef.upthreeStarPool.Weight
                );
            }

            return thingDefs;
        }

        //按当前卡池生成十连结果，保留原有 FES 分流与保底规则。
        private static List<BaStudentDef> DrawTen()
        {
            List<BaStudentDef> thingDefs;

            //FES池十连时沿用同一套三星池顺序，保证保底和调试强制三星都走正确分布
            if (GakaMapData.selectGachaDef.isFes)
            {
                thingDefs = GachaSystem.MultiDrawFES10(
                    GakaMapData.selectGachaDef.oneStarPool.StudentList,
                    GakaMapData.selectGachaDef.twoStarPool.StudentList,
                    GakaMapData.selectGachaDef.threeStarPool.StudentList,
                    GakaMapData.selectGachaDef.FESupthreeStarPool.StudentList,
                    GakaMapData.selectGachaDef.FESthreeStarPool.StudentList
                );
            }
            else
            {
                thingDefs = GachaSystem.MultiDraw10(
                    GakaMapData.selectGachaDef.oneStarPool.StudentList,
                    GakaMapData.selectGachaDef.twoStarPool.StudentList,
                    GakaMapData.selectGachaDef.threeStarPool.StudentList,
                    GakaMapData.selectGachaDef.upthreeStarPool.StudentList,
                    GakaMapData.selectGachaDef.oneStarPool.Weight,
                    GakaMapData.selectGachaDef.twoStarPool.Weight,
                    GakaMapData.selectGachaDef.threeStarPool.Weight,
                    GakaMapData.selectGachaDef.upthreeStarPool.Weight
                );
            }

            return thingDefs;
        }
        //生成学生抽卡结果，仅保存头像路径，显示时再加载图片。
        public static gacaData creatGacaData(BaStudentDef studentDef)
        {
            if (studentDef == null)
            {
                return null;
            }

            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            PawnKindDef kindDef;
            StudentIdentityUtility.TryGetPawnKindDef(studentDef.defName, out kindDef);
            BaStudentUI studentUI = studentDef.BaStudentUI;
            BaStudentData studentData = studentDef.baStudentData;
            string studentId = StudentIdentityUtility.GetStudentId(studentDef) ?? StudentIdentityUtility.GetStudentId(kindDef);

            gacaData gacaData = new gacaData();
            gacaData.BaStudentDef = studentDef;
            gacaData.PawnKindDef = kindDef;
            gacaData.StudentId = studentId;
            gacaData.StudentUI = studentUI;
            gacaData.BaStudentData = studentData;
            gacaData.starNum = studentUI != null ? studentUI.CharacterStarCount : 1;
            gacaData.AvatarPath = studentData?.avtTexPath;
            if (!StudentRosterUtility.IsStudentDef(tracker, studentId))
            {
                tracker.HaveStudent.Add(new BANWlLib.mainUI.pojo.StudentData(studentId));
                gacaData.isNew = true;
            }
            else
            {
                gacaData.isNew = false;
            }
            var upPool = GakaMapData.selectGachaDef?.upthreeStarPool?.StudentList;
            gacaData.isUp = upPool != null && upPool.Any(def => def != null && StudentIdentityUtility.GetStudentId(def) == studentId);
            return gacaData;
        }
        //把外部抽取结果转换成界面结果数据。
        public static void ExtGakaData(List<BaStudentDef> extlist)
        {
            GakaMapData.gacaDatas.Clear();
            GakaMapData.tenGacha = true;
            foreach (BaStudentDef studentDef in extlist)
            {
                gacaData gacaData = creatGacaData(studentDef);
                if (gacaData != null)
                {
                    GakaMapData.gacaDatas.Add(gacaData);
                }
            }
            spawStudentThing();
        }

        //把外部抽取结果转换成界面结果数据。
        public static void ExtGakaData(BaStudentDef extlist)
        {
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            GakaMapData.gacaDatas.Clear();
            GakaMapData.tenGacha = false;
            if (extlist != null)
            {
                gacaData gacaData = creatGacaData(extlist);
                if (gacaData != null)
                {
                    GakaMapData.gacaDatas.Add(gacaData);
                }
            }
            spawStudentThing();
        }

        //结算抽卡获得的学生和重复兑换物品。
        public static void spawStudentThing()
        {
            Map map = Find.CurrentMap;
            foreach (gacaData gacaData in GakaMapData.gacaDatas)
            {
                BaStudentData studentData = gacaData.BaStudentData;
                if (studentData == null)
                {
                    continue;
                }

                if (gacaData.isUp)
                {
                    if (gacaData.isNew)
                    {
                        foreach (KeyValuePair<ThingDef, int> kvp in studentData.OneGakaStudentThingData)
                        {
                            Thing thing = ThingMaker.MakeThing(kvp.Key);
                            thing.stackCount = kvp.Value;
                            PawnDropHelper.DropThing(map, thing);
                        }
                    }
                    else if (!gacaData.isNew)
                    {
                        foreach (KeyValuePair<ThingDef, int> kvp in studentData.UpGakaStudentThingData)
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
                        foreach (KeyValuePair<ThingDef, int> kvp in studentData.GakaStudentThingData)
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
                if(gacaData.BaStudentData != null && gacaData.BaStudentData.StarCont == 3)
                {
                    GakaMapData.isP3 = true;
                }
            }
        }
        //打开抽卡界面并刷新当前卡池信息。
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

        //为可选卡池创建选择按钮。
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

            //先显示固定卡池。
            foreach (Gacha gacha in fixedPool)
            {
                CreateGachaButton(gacha);
            }

            //再显示未包含在固定池中的随机卡池。
            foreach (Gacha gacha in displayPool)
            {
                if (!fixedPool.Contains(gacha))
                {
                    CreateGachaButton(gacha);
                }
            }
        }

        //创建卡池选择按钮，图片随按钮可见状态加载。
        private static void CreateGachaButton(Gacha gacha)
        {
            GameObject gakaHead = GameObject.Instantiate(GakaMapData.GachaPofab);
            imgcvT2d.SetImage(gakaHead.GetComponent<Image>(), imgcvT2d.getRimWorldImgPath(gacha.gachaTexPath));
            gakaHead.GetComponent<Button>().onClick.AddListener(delegate
            {
                GakaMapData.selectGachaDef = gacha;
                SwitchGacha(gacha);
            });
            gakaHead.transform.SetParent(GakaMapData.Content.transform, false);
        }

        //切换当前卡池并更新展示内容。
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
        //准备并播放指定路径的抽卡视频。
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

        //在视频准备完成后开始播放。
        private static void OnVideoPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= OnVideoPrepared;
            source.Play();
        }

        //根据卡池类型设置抽取信息文字。
        public static void SettingGakaTextForRandPool(Gacha gacha)
        {
            var gameComp = GakaMapData.gamecomp_GakaAction;
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_time").GetComponent<Engine.UI.Text>().text =
                gameComp.GetRemainingTimeString(gameComp.RotationTickCounter);

            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_title").GetComponent<Engine.UI.Text>().text = gacha.gachaTitle;
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_up").GetComponent<Engine.UI.Text>().text = gacha.gachaUp;
            GakaMapData.GakaUIPet.transform.Find("MainBack/box/Text_dex").GetComponent<Engine.UI.Text>().text = gacha.gachaDesc;
        }
    }
}
