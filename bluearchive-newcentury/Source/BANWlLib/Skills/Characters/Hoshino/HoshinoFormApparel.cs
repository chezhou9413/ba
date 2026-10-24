using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //在星野的两件形态服装之间转换，保留耐久、品质、颜色与穿着锁定。
    public static class HoshinoFormApparel
    {
        //只转换已经穿着的指定服装，不生成丢失或被脱下的装备。
        public static void Apply(Pawn pawn, HoshinoSkillConfig config, bool tank)
        {
            ThingDef desired = tank ? config.shieldApparel : config.backpackApparel;
            ThingDef previous = tank ? config.backpackApparel : config.shieldApparel;
            Apparel source = pawn.apparel.WornApparel.FirstOrDefault(a => a.def == previous);
            if (source == null) return;
            var replacement = (Apparel)ThingMaker.MakeThing(desired, source.Stuff);
            var quality = source.TryGetComp<CompQuality>();
            if (quality != null) replacement.TryGetComp<CompQuality>()?.SetQuality(quality.Quality, ArtGenerationContext.Colony);
            replacement.TryGetComp<CompColorable>()?.SetColor(source.DrawColor);
            if (source.def.useHitPoints && replacement.def.useHitPoints)
                replacement.HitPoints = Mathf.Clamp(Mathf.RoundToInt(
                    source.HitPoints * (float)replacement.MaxHitPoints / source.MaxHitPoints), 1, replacement.MaxHitPoints);
            bool locked = pawn.apparel.IsLocked(source);
            pawn.apparel.Remove(source);
            pawn.apparel.Wear(replacement, locked: locked);
            if (replacement.Wearer != pawn)
            {
                pawn.apparel.Wear(source, locked: locked);
                replacement.Destroy();
                Log.Error("星野形态装备转换失败：" + previous.defName + " -> " + desired.defName);
                return;
            }
            source.Destroy();
        }
    }
}
