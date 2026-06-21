using System.Collections.Generic;
using Verse;

namespace BANWlLib.BaDef
{
    //特殊卡池队列配置，负责描述固定刷新周期和该周期强制展示的卡池列表。
    public class SpecialQueueConfig
    {
        public int triggerIndex;
        public List<Gacha> forcedPool;
    }

    //招募卡池系统配置 Def，负责提供轮换间隔、随机池、固定池和特殊周期池配置。
    public class GachaSetting : Def
    {
        public int RotationTick;
        public int SlotsCount = 4;
        public List<Gacha> StandardPool;
        public List<Gacha> LimitedPool;
        public List<Gacha> FixedPool;
        public List<SpecialQueueConfig> SpecialQueues;
    }
}
