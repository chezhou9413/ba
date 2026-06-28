using Verse;

namespace BANWlLib.BattleSystem
{
    // 叠层战斗状态，负责在健康页括号中显示当前实例的叠层进度。
    public class Hediff_BattleStack : HediffWithComps
    {
        // 状态括号文本，负责从当前 Hediff 实例的叠层组件读取层数。
        public override string LabelInBrackets
        {
            get
            {
                HediffComp_BattleStack stackComp = this.TryGetComp<HediffComp_BattleStack>();
                if (stackComp == null)
                {
                    return base.LabelInBrackets;
                }

                return stackComp.CurrentStacks + "/" + stackComp.MaxStacks;
            }
        }
    }
}
