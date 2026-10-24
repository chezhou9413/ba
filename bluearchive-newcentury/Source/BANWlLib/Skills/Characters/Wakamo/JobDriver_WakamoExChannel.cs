namespace BANWlLib.Skills
{
    //若藻EX按逐发时序出伤，发射完成前持续瞄准并禁止普攻。
    public class JobDriver_WakamoExChannel : JobDriver_SpecialBurstChannel
    {
        protected override SpecialSkillRole Role => SpecialSkillRole.Wakamo;
    }
}
