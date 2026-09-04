using System.Collections.Generic;
using BANWlLib.CostSystem;
using Verse;

namespace BANWlLib.BaDef
{
    //任务节点Def负责描述任务入口、地图、奖励、敌人和任务专属COST规则。
    public class BaMissionNode : Def
    {
        public float oder;
        public string MissionID;
        public string MissionTitle;
        public BaMissionType MissionType;
        public BaMissionNode UnlockedOn;
        public string MissionDes;
        public List<string> MissionTarget = new List<string>();
        public List<MissionReward> Reward = new List<MissionReward>();
        public List<EnemyList> EnemyList = new List<EnemyList>();
        public BaMapDef missionMapDef;
        public BaMissionRunTime missionRunTimeDef;
        public BACostRules costRules = new BACostRules();

        //检查任务专属COST上限与回复倍率是否合法。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (costRules == null)
            {
                yield return defName + " 缺少 costRules。";
                yield break;
            }

            foreach (string error in costRules.ConfigErrors(defName))
            {
                yield return error;
            }
        }
    }

    //任务奖励品质枚举负责表达任务界面与结算使用的奖励等级。
    public enum BaMissionQuality
    {
        Low,
        Medium,
        High,
        Epic
    }

    //任务奖励配置负责绑定奖励物品、品质与数量。
    public class MissionReward
    {
        public ThingDef thingDef;
        public BaMissionQuality quality;
        public int count;
    }

    //任务敌人配置负责绑定敌人种类与界面分类标签。
    public class EnemyList
    {
        public PawnKindDef pawnKindDef;
        public string tagPath1;
        public string tagPath2;
    }
}
