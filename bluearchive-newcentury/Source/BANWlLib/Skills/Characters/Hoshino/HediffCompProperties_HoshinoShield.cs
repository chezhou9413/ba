using Verse;

namespace BANWlLib.Skills
{
    //让星野护盾在护甲与承伤处理后吸收伤害。
    public class HediffCompProperties_HoshinoShield : HediffCompProperties
    {
        //绑定星野护盾组件。
        public HediffCompProperties_HoshinoShield() { compClass = typeof(HediffComp_HoshinoShield); }
    }
}
