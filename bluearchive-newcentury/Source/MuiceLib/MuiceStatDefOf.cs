using RimWorld;

namespace MuiceLib;

[DefOf]
public static class MuiceStatDefOf
{
    public static StatDef Muice_RangedAimingTimeFactor;

    static MuiceStatDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MuiceStatDefOf));
    }
}
