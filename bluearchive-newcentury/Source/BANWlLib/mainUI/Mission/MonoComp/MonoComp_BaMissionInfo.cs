using BANWlLib.BaDef;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.Tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.Mission.MonoComp
{
    //展示选中任务的信息、目标、奖励和敌方单位。
    public class MonoComp_BaMissionInfo : MonoBehaviour
    {
        public BaMissionNode BaMissionNode;

        public GameObject MissionID;

        public GameObject TargetContent;
        public GameObject RewardContent;
        public GameObject EnemyContent;

        //缓存任务详情控件引用。
        void Awake()
        {
            EnemyContent = this.transform.Find("EnemyList/Scroll View/Viewport/Content").gameObject;
            RewardContent = this.transform.Find("MissionReward/Viewport/Content").gameObject;
            TargetContent = this.transform.Find("MissionTarget/Viewport/Content").gameObject;
        }
        //初始化任务详情按钮与交互。
        void Start()
        {
            this.transform.Find("ColseButtom").GetComponent<Button>().onClick.AddListener(() =>
            {
                ColseMissInfo();
            });
            this.transform.Find("StartMission").GetComponent<Button>().onClick.AddListener(() =>
            {
                MissionMapData.missionSelect.baMissionNode = BaMissionNode;
                MonoComp_BackButton.instance.setNewObj(MissionMapData.back4, "bgm6");
                LoopBGMManager.switchUiBgm("bgm6");
                MissionMapData.back4.SetActive(true);
                UiMapData.mainUI.transform.Find("daohang/fanhuiyouxi").gameObject.SetActive(false);
                ColseMissInfo();
            });
        }
        //在任务详情启用时刷新显示数据。
        void OnEnable()
        {
            DeleteAllChildren(TargetContent);
            DeleteAllChildren(RewardContent);
            DeleteAllChildren(EnemyContent);
            SetMissInfoAndShowUI(BaMissionNode);
        }

        //关闭任务详情面板。
        public void ColseMissInfo()
        {
            UiMapData.isLocKBack = false;
            this.gameObject.SetActive(false);
        }
        //选择任务并显示其详情。
        public void ShowMissInfo(BaMissionNode mission)
        {
            BaMissionNode = mission;
            this.gameObject.SetActive(true);
        }
        //刷新任务说明、目标、奖励及敌方信息。
        void SetMissInfoAndShowUI(BaMissionNode mission)
        {
            this.gameObject.transform.Find("MissionID").GetComponent<UnityEngine.UI.Text>().text = mission.MissionID;
            this.gameObject.transform.Find("MissionType").GetComponent<UnityEngine.UI.Text>().text = mission.MissionTitle;
            this.gameObject.transform.Find("MissionDes").GetComponent<UnityEngine.UI.Text>().text = mission.MissionDes;
            spawMissionTarget();
            spawnRewardTarget();
            spawnRewardEnemy();
        }

        //清除列表中已有的子控件。
        private void DeleteAllChildren(GameObject parentObj)
        {
            Transform parentTrans = parentObj.transform;
            for (int i = parentTrans.childCount - 1; i >= 0; i--)
            {
                GameObject child = parentTrans.GetChild(i).gameObject;
                GameObject.Destroy(child);
            }
        }

        //创建任务目标条目。
        private void spawMissionTarget()
        {
            foreach (string a in BaMissionNode.MissionTarget)
            {
                if (TargetContent == null)
                {
                    Log.Error("TargetContent 找不到目标挂载点");
                }
                if (MissionMapData.MissionTargetNode == null)
                {
                    Log.Error("MissionTargetNode 找不到预制体");
                }
                GameObject MissionTargetObj = GameObject.Instantiate(MissionMapData.MissionTargetNode, TargetContent.transform);
                MissionTargetObj.GetComponent<UnityEngine.UI.Text>().text = a;
            }
        }

        //创建任务奖励条目。
        private void spawnRewardTarget()
        {
            foreach (MissionReward a in BaMissionNode.Reward)
            {
                GameObject MissionTargetObj = GameObject.Instantiate(MissionMapData.MissionRewardNode, RewardContent.transform);
                MissionTargetObj.transform.Find("pingzhi/" + a.quality.ToString().ToLower()).gameObject.SetActive(true);
                MissionTargetObj.transform.Find("bodyMain").GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetSpriteFromThingDef(a.thingDef);
                MissionTargetObj.transform.Find("cont").GetComponent<UnityEngine.UI.Text>().text = "X" + a.count;
            }
        }

        //创建敌方单位条目并按需绑定其标签图标。
        private void spawnRewardEnemy()
        {
            foreach (EnemyList a in BaMissionNode.EnemyList)
            {
                GameObject EnemyObj = GameObject.Instantiate(MissionMapData.EnemyListNode, EnemyContent.transform);
                EnemyObj.transform.Find("EnemyImage").GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetSpriteFromKind(a.pawnKindDef);
                EnemyObj.transform.Find("EnemyLable").GetComponent<UnityEngine.UI.Text>().text = a.pawnKindDef.label;
                GameObject tag1 = EnemyObj.transform.Find("tag1").gameObject;
                GameObject tag2 = EnemyObj.transform.Find("tag2").gameObject;
                tag1.SetActive(false);
                tag2.SetActive(false);
                if(a.tagPath1 != null)
                {
                    tag1.SetActive(true);
                    imgcvT2d.SetImage(tag1.GetComponent<Image>(), imgcvT2d.getRimWorldImgPath(a.tagPath1));
                }
                if(a.tagPath2 != null)
                {
                    tag2.SetActive(true);
                    imgcvT2d.SetImage(tag2.GetComponent<Image>(), imgcvT2d.getRimWorldImgPath(a.tagPath2));
                }
            }
        }
    }
}
