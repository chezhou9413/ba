using Verse;

namespace BANWlLib.Skills
{
    //若藻记录身份，负责区分本次EX与旧的飞行弹丸，并保存敌方标记引用。
    public class WakamoRuntimeState : IExposable
    {
        public int castId;
        public Hediff_WakamoMark mark;

        //保存施法批次和标记，避免读档后重复附加状态。
        public void ExposeData()
        {
            Scribe_Values.Look(ref castId, "castId");
            Scribe_References.Look(ref mark, "mark");
        }
    }
}
