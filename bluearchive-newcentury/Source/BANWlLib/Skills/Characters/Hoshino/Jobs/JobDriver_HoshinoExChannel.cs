using Verse.AI;

namespace BANWlLib.Skills
{
    //星野在整轮输出EX正常结束后取得强化普攻。
    public class JobDriver_HoshinoExChannel : JobDriver_SpecialBurstChannel
    {
        protected override SpecialSkillRole Role => SpecialSkillRole.Hoshino;

        //取消施法只清理未发射攻击；目标死亡不属于取消条件。
        protected override void FinishBurst(Hediff_SpecialSkillState state, JobCondition condition)
        {
            if (condition == JobCondition.Succeeded && state.hoshino.grantAfterCast && state.stage == 0)
                state.hoshino.empoweredReady = true;
            state.hoshino.grantAfterCast = state.hoshino.outputExCasting = false;
        }
    }
}
