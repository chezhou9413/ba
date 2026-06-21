using BANWlLib.mainUI.Mission.MonoComp;
using BANWlLib.Tool;
using System.Collections.Generic;
using UnityEngine;

namespace BANWlLib.mainUI.Mission
{
    public static class MissionMapData
    {
        public static GameObject MissionUI;
        public static GameObject typeContent;
        public static GameObject MissionTypeObj;

        public static GameObject NodeContent;
        public static GameObject MissionNodeGameObj;

        public static GameObject back2;
        public static GameObject mianImage;

        public static GameObject back3;
        public static MonoComp_BaMissionInfo missionInfo;

        public static List<MonoComp_BaMissionNode> AllBaMissionNode = new List<MonoComp_BaMissionNode>();

        public static Dictionary<string, Sprite> MissionSprite = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> pawnBigHardSprite = new Dictionary<string, Sprite>();

        public static GameObject back4;
        public static MonoComp_BaMissionSelect missionSelect;
        public static GameObject selectList;
        public static GameObject selectQue;

        public static GameObject MissionTargetNode;
        public static GameObject MissionRewardNode;
        public static GameObject EnemyListNode;

        public static void Reset()
        {
            RimWorldUISpriteUtil.ClearGeneratedSpriteCache();
            selectQue = null;
            selectList = null;
            missionSelect = null;
            pawnBigHardSprite.Clear();
            MissionSprite.Clear();
            EnemyListNode = null;
            MissionTargetNode = null;
            MissionRewardNode = null;
            missionInfo = null;
            AllBaMissionNode.Clear();
            back3 = null;
            back2 = null;
            back4 = null;
            mianImage = null;
            MissionNodeGameObj = null;
            NodeContent = null;
            MissionTypeObj = null;
            MissionUI = null;
            typeContent = null;
        }
    }
}
