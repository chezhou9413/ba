using Verse;

namespace BANWlLib.Skills
{
    //星野运行数据，保存下一轮强化资格和当前连射的强化身份。
    public class HoshinoRuntimeState : IExposable
    {
        public bool empoweredReady;
        public bool empoweredBurst;
        public bool grantAfterCast;
        public bool outputExCasting;
        public bool moving;
        public Thing empoweredTarget;
        public IntVec3 empoweredAimCell = IntVec3.Invalid;

        //保存技能结束后的强化资格和连射中途的强化身份。
        public void ExposeData()
        {
            Scribe_Values.Look(ref empoweredReady, "empoweredReady");
            Scribe_Values.Look(ref empoweredBurst, "empoweredBurst");
            Scribe_Values.Look(ref grantAfterCast, "grantAfterCast");
            Scribe_Values.Look(ref outputExCasting, "outputExCasting");
            Scribe_Values.Look(ref moving, "moving");
            Scribe_References.Look(ref empoweredTarget, "empoweredTarget");
            Scribe_Values.Look(ref empoweredAimCell, "empoweredAimCell", IntVec3.Invalid);
        }
    }
}
