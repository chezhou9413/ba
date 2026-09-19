using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能攻击段，负责配置弹丸、范围、连射和基础战斗参数。
    public class SpecialAttackConfig : BattleActionConfig
    {
        public ThingDef projectileDef;
        public float radius;
        public int shots = 1;
        public int shotIntervalTicks = 6;

        //保存脱手攻击段的完整配置，供存档后的弹丸和延迟攻击继续使用。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref shots, "shots", 1);
            Scribe_Values.Look(ref shotIntervalTicks, "shotIntervalTicks", 6);
        }
    }
}
