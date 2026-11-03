using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //白子自动技能前摇、连射截止时间与退场缓冲的独立游戏时钟。
    public class ShirokoRuntimeState : IExposable
    {
        public Job normalJob;
        public int warmupStartTick = -1;
        public int normalEndTick = -1;
        public int nextBufferStackTick = -1;

        //恢复前摇和剩余连射时序，不重复发动技能。
        public void ExposeData()
        {
            Scribe_References.Look(ref normalJob, "normalJob");
            Scribe_Values.Look(ref warmupStartTick, "warmupStartTick", -1);
            Scribe_Values.Look(ref normalEndTick, "normalEndTick", -1);
            Scribe_Values.Look(ref nextBufferStackTick, "nextBufferStackTick", -1);
        }
    }
}
