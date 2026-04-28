using BANWlLib.BaDef;
using BANWlLib.mainUI.Mission;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using BANWlLib.uicreater.tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using Verse.Noise;

namespace BANWlLib.mainUI.Gaka.MonoComp
{
   public class MonoComp_updataPoit:MonoBehaviour
    {
        public static MonoComp_updataPoit instance;
        public UnityEngine.UI.Text poit_cont;
        public Button openShot;
        public GameObject gakashot;
        public List<GameObject> gakashotList = new List<GameObject>();
        public ManualDataGameComp tracker;
        void Start()
        {
            instance = this;
            tracker = Current.Game.GetComponent<ManualDataGameComp>();
            openShot = this.transform.Find("poit_buttom").GetComponent<Button>();
            poit_cont = this.transform.Find("poit_cont").GetComponent<UnityEngine.UI.Text>();
            openShot.onClick.AddListener(() =>
            {
                openGakaShot();
            });
        }

        void openGakaShot()
        {
            if (gakashot != null)
            {
                RefreshShopList();
                return;
            }

            UiMapData.isLocKBack = true;
            gakashot = GameObject.Instantiate(GakaMapData.ShotMaskBack);
            gakashot.transform.SetParent(GakaMapData.GakaUIPet.transform, false);
            gakashot.transform.SetAsLastSibling();
            RefreshShopList();
            gakashot.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() => {
                UiMapData.isLocKBack = false;
                gakashotList = new List<GameObject>();
                GameObject.Destroy(gakashot);
                gakashot = null;
            });
        }

        public void RefreshShopList()
        {
            if (gakashot == null)
            {
                return;
            }

            GameObject Content = gakashot.transform.Find("Back/grow_back/Scroll View/Viewport/Content").gameObject;
            for (int i = Content.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(Content.transform.GetChild(i).gameObject);
            }
            gakashotList.Clear();

            List<ThingDef> thingDefs = GetRedeemableStudents();
            foreach(ThingDef thingDef in thingDefs)
            {
                BaStudentRaceDef studentDef = thingDef as BaStudentRaceDef;
                if (studentDef != null)
                {
                    GameObject obj = Instantiate(GakaMapData.GakaShotList);
                    obj.transform.Find("StuName").GetComponent<UnityEngine.UI.Text>().text = studentDef.BaStudentUI.StudentBio.StudentBioName;
                    obj.transform.Find("shouchouback/Text").GetComponent<UnityEngine.UI.Text>().text = "首次奖励可获得["+ studentDef.BaStudentUI.StudentBio.StudentBioName + "]的神名文字x100]！";
                    obj.transform.Find("StuXuexiaoIcon").GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(studentDef.BaStudentUI.StudentBio.AcademyLogoPath));
                    obj.transform.Find("selectList/avt").GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetHeadShotSpriteFromDef(studentDef);
                    obj.transform.Find("selectList/box").GetComponent<Image>().sprite = MissionMapData.MissionSprite[studentDef.baStudentData.DamageType + "_box"];
                    obj.transform.Find("selectList/pos").GetComponent<Image>().sprite = MissionMapData.MissionSprite[studentDef.baStudentData.PosType + "_min"];
                    obj.transform.Find("selectList/Start/StarCont").GetComponent<UnityEngine.UI.Text>().text = studentDef.baStudentData.StarCont.ToString();
                    obj.transform.Find("StuXuexiao").GetComponent<UnityEngine.UI.Text>().text =string.IsNullOrEmpty(studentDef.baStudentData.stuSchool) ? "未配置学院" : studentDef.baStudentData.stuSchool;
                    obj.transform.Find("buttom_zhaomu/Poit").GetComponent <UnityEngine.UI.Text>().text = 200.ToString();
                    obj.transform.Find("buttom_zhaomu").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        LoopBGMManager.playEffAudio("鼠标点击音效");
                        BamessageUI.ShowBaMessageUIQuek("提示", $"是否花费200招募点数兑换 {studentDef.BaStudentUI.StudentBio.StudentBioName}\n当前持有：{GakaMapData.gamecomp_GakaAction.gacaPoit}", "确认", "取消", () =>
                        {
                            if (GakaMapData.gamecomp_GakaAction.updataGacaPoit(-200))
                            {
                                Gakalord.SelectStu(thingDef.defName);
                                BamessageUI.ShowBaMessageUI("兑换成功", $"{studentDef.BaStudentUI.StudentBio.StudentBioName }加入了你的殖民地", "确认");
                            }
                            else
                            {
                                BamessageUI.ShowBaMessageUI("提示", "招募点数不够！", "确认");
                            }
                        });
                    });
                    obj.name = studentDef.defName;
                    obj.transform.SetParent(Content.transform,false);
                    gakashotList.Add(obj);
                }
            }
        }

        private List<ThingDef> GetRedeemableStudents()
        {
            Gacha selectedGacha = GakaMapData.selectGachaDef;
            if (selectedGacha != null)
            {
                return GetRedeemableStudentsFromGacha(selectedGacha).ToList();
            }

            List<Gacha> gachas = GakaMapData.gamecomp_GakaAction.GetCurrentRandomPool();
            return gachas
                .SelectMany(GetRedeemableStudentsFromGacha)
                .Distinct()
                .ToList();
        }

        private IEnumerable<ThingDef> GetRedeemableStudentsFromGacha(Gacha gacha)
        {
            if (gacha == null)
            {
                return Enumerable.Empty<ThingDef>();
            }

            GachaPool redeemPool = gacha.isFes ? gacha.FESupthreeStarPool : gacha.upthreeStarPool;
            if (redeemPool?.RaceList.NullOrEmpty() != false)
            {
                return Enumerable.Empty<ThingDef>();
            }

            return redeemPool.RaceList;
        }

        void Update()
        {
            if(GakaMapData.gamecomp_GakaAction != null)
            {
                poit_cont.text = GakaMapData.gamecomp_GakaAction.gacaPoit.ToString();
                if(gakashot != null)
                {
                    foreach(GameObject go in gakashotList)
                    {
                        if (!tracker.HaveStudent.Any(p => p.DefName == go.name))
                        {
                            go.transform.Find("weihuode").gameObject.SetActive(true);
                        }
                        else
                        {
                            go.transform.Find("weihuode").gameObject.SetActive(false);
                        }
                    }
                    gakashot.transform.Find("Back/poit_back/Text_cont").GetComponent<UnityEngine.UI.Text>().text = GakaMapData.gamecomp_GakaAction.gacaPoit.ToString();
                }
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
