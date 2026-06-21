using RimWorld;
using Verse;

namespace DodgeMod
{
    // 闪避 StatDef 引用，负责让代码稳定读取 BANW_Miss 闪避属性。
    [DefOf]
    public static class DodgeStatDefOf
    {
        public static StatDef BANW_Miss;

        // 静态初始化，负责触发 RimWorld 的 DefOf 字段绑定。
        static DodgeStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DodgeStatDefOf));
        }
    }
}
