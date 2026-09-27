namespace BANWlLib.Skills
{
    //星野EX伤害段，负责声明相对准备结束的发射时间和独立伤害配置。
    public class HoshinoAttackStage
    {
        public int delayTicks;
        public bool targetLocation;
        public SpecialAttackConfig attack;
    }
}
