using System.Collections.Generic;
using Verse;

namespace BANWlLib.Auras
{
    //受益状态扩展负责统一同名光环的叠加规则和强度上限。
    public sealed class FriendlyAuraEffectExtension : DefModExtension
    {
        public bool stackSeverity;
        public float maximumSeverity = 10f;

        //检查受益强度上限。
        public override IEnumerable<string> ConfigErrors()
        {
            if (maximumSeverity <= 0f)
                yield return "友军光环maximumSeverity必须大于0。";
        }
    }
}
