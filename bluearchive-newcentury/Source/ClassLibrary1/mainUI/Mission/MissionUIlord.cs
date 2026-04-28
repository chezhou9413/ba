using BANWlLib.BaDef;
using BANWlLib.mainUI.Mission.MonoComp;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using newpro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using Verse.Noise;
using static UnityEngine.Networking.UnityWebRequest;

namespace BANWlLib.mainUI.Mission
{
    public static class MissionUIlord
    {
        public static void lord()
        {
            lordMissionPrefab(UiMapData.bundle);
            LordMissionTypeDef();
            LordMissionNodeDef();
            lordPawnHead();
        }

        private static void LordMissionTypeDef()
        {
            MissionMapData.MissionUI = UiMapData.mainUI.transform.Find("Mission").gameObject;
            MissionMapData.back2 = MissionMapData.MissionUI.transform.Find("back2").gameObject;
            MissionMapData.back3 = MissionMapData.MissionUI.transform.Find("back3").gameObject;
            MissionMapData.back4 = MissionMapData.MissionUI.transform.Find("back4").gameObject;
            MissionMapData.missionSelect = MissionMapData.back4.AddComponent<MonoComp_BaMissionSelect>();
            MissionMapData.missionInfo = MissionMapData.back3.AddComponent<MonoComp_BaMissionInfo>();
            MissionMapData.typeContent = MissionMapData.MissionUI.transform.Find("back/MissionTypeList/TypeBack/Scroll View/Viewport/Content").gameObject;
            List<BaMissionType> sortedList = DefDatabase<BaMissionType>.AllDefs.OrderBy(x => x.oder).ToList();
            foreach (BaMissionType missionType in sortedList)
            {
                GameObject typeObj = GameObject.Instantiate(MissionMapData.MissionTypeObj, MissionMapData.typeContent.transform);
                if (!string.IsNullOrEmpty(missionType.UIIconPath))
                {
                    typeObj.GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(missionType.UIIconPath));
                }
                MonoComp_BaMissionType monoComp = typeObj.AddComponent<MonoComp_BaMissionType>();
                monoComp.baMissionType = missionType;
            }
        }

        private static void LordMissionNodeDef()
        {
            MissionMapData.NodeContent = MissionMapData.MissionUI.transform.Find("back2/MissionNodeBack/Scroll View/Viewport/Content").gameObject;
            MissionMapData.mianImage = MissionMapData.MissionUI.transform.Find("back2/mianImage").gameObject;
            List<BaMissionNode> sortedList = DefDatabase<BaMissionNode>.AllDefs.OrderBy(x => x.oder).ToList();
            foreach (BaMissionNode missionNode in sortedList)
            {
                GameObject typeObj = GameObject.Instantiate(MissionMapData.MissionNodeGameObj, MissionMapData.NodeContent.transform);
                typeObj.transform.Find("MissionID").GetComponent<UnityEngine.UI.Text>().text = missionNode.MissionID;
                typeObj.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = missionNode.MissionTitle;
                MonoComp_BaMissionNode monoComp = typeObj.AddComponent<MonoComp_BaMissionNode>();
                monoComp.type = missionNode.MissionType;
                monoComp.selfMissionInfo = missionNode;
                MissionMapData.AllBaMissionNode.Add(monoComp);
            }
        }

        private static void lordPawnHead()
        {
             List<BaStudentRaceDef> StudentList = DefDatabase<BaStudentRaceDef>.AllDefsListForReading;
            if (StudentList.NullOrEmpty())
            {
                return;
            }
            foreach(BaStudentRaceDef student in StudentList)
            {
                Sprite sprite = RimWorldUISpriteUtil.GetHeadShotSpriteFromDef(student, MissionSpriteSizes.CachedHead);
                MissionMapData.pawnBigHardSprite[student.defName] = sprite;
            }
        }
        private static void lordMissionPrefab(AssetBundle bundle)
        {
            MissionMapData.MissionTypeObj = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/MissionType.prefab");
            MissionMapData.MissionNodeGameObj = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/MissionNodeGameObj.prefab");
            MissionMapData.MissionTargetNode = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/MissionTarget_Node.prefab");
            MissionMapData.MissionRewardNode = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/MissionRewardNode.prefab");
            MissionMapData.EnemyListNode = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/EnemyListNode.prefab");
            MissionMapData.selectList = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/selectList.prefab");
            MissionMapData.selectQue = bundle.LoadAsset<GameObject>("Assets/Scenes/Resources/MissionGameObj/selectQue.prefab");
            string[] allAssetNames = bundle.GetAllAssetNames();
            string targetFolder = "Assets/Scenes/Resources/Tex/Mission/P4/chooseType/".ToLower();
            foreach (string assetPath in allAssetNames)
            {
                if (assetPath.StartsWith(targetFolder))
                {
                    Sprite sp = bundle.LoadAsset<Sprite>(assetPath);
                    if (sp != null)
                    {
                        MissionMapData.MissionSprite[sp.name] = sp;
                    }
                }
            }
        }
    }
}

