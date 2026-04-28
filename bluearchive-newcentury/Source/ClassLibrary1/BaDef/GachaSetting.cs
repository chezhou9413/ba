using System.Collections.Generic;
using Verse;

namespace BANWlLib.BaDef
{
    public class SpecialQueueConfig
    {
        public int triggerIndex;
        public List<Gacha> forcedPool;
    }

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