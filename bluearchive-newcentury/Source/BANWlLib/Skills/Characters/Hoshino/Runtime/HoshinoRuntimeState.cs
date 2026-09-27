using Verse;

namespace BANWlLib.Skills
{
    //星野运行数据，负责保存下一轮强化资格和当前连射的强化身份。
    public class HoshinoRuntimeState : IExposable
    {
        public bool empoweredReady;
        public bool empoweredBurst;
        public bool grantAfterCast;
        public bool outputExCasting;
        public bool moving;

        //保存技能结束后的强化资格和连射中途的强化身份。
        public void ExposeData()
        {
            Scribe_Values.Look(ref empoweredReady, "empoweredReady");
            Scribe_Values.Look(ref empoweredBurst, "empoweredBurst");
            Scribe_Values.Look(ref grantAfterCast, "grantAfterCast");
            Scribe_Values.Look(ref outputExCasting, "outputExCasting");
            Scribe_Values.Look(ref moving, "moving");
        }
    }
}
