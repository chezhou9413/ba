using HarmonyLib;
using UnityEngine;
using Verse;

namespace BANWlLib.BaVerb
{
    //能力主武器瞄准补丁，负责让指定 Ability 前摇期间显示原版武器瞄准绘制。
    [HarmonyPatch(typeof(Stance_Warmup), nameof(Stance_Warmup.StanceDraw))]
    public static class AbilityPrimaryWeaponAimPatch
    {
        //绘制前摇主武器瞄准，负责补足 Ability 施法 Job 隐藏武器时缺失的射击动作。
        public static void Postfix(Stance_Warmup __instance)
        {
            if (!ShouldDraw(__instance, out Pawn pawn, out ThingWithComps weapon, out LocalTargetInfo target))
            {
                return;
            }

            DrawWeaponAim(pawn, weapon, target);
        }

        //判断是否需要绘制，负责过滤未配置技能、无武器和原版已经会绘制武器的情况。
        private static bool ShouldDraw(Stance_Warmup stance, out Pawn pawn, out ThingWithComps weapon, out LocalTargetInfo target)
        {
            pawn = stance?.stanceTracker?.pawn;
            weapon = pawn?.equipment?.Primary;
            target = stance != null ? stance.focusTarg : LocalTargetInfo.Invalid;
            if (pawn == null || weapon == null || !target.IsValid)
            {
                return false;
            }

            Verb_CastAbilityWithTargetPreview abilityVerb = stance.verb as Verb_CastAbilityWithTargetPreview;
            if (abilityVerb == null || !abilityVerb.ShouldDrawPrimaryWeaponAim())
            {
                return false;
            }

            CompEquippable equippable = weapon.GetComp<CompEquippable>();
            Verb primaryVerb = equippable?.PrimaryVerb;
            if (primaryVerb == null || primaryVerb.verbProps == null || primaryVerb.verbProps.IsMeleeAttack)
            {
                return false;
            }

            return pawn.CurJob == null || pawn.CurJob.def?.neverShowWeapon != false;
        }

        //绘制武器朝向，负责复用原版 DrawEquipmentAiming 的枪械角度和后坐力表现。
        private static void DrawWeaponAim(Pawn pawn, ThingWithComps weapon, LocalTargetInfo target)
        {
            Vector3 targetPos = target.HasThing ? target.Thing.DrawPos : target.Cell.ToVector3Shifted();
            Vector3 drawPos = pawn.DrawPos;
            if ((targetPos - drawPos).MagnitudeHorizontalSquared() <= 0.001f)
            {
                return;
            }

            float aimAngle = (targetPos - drawPos).AngleFlat();
            float distanceFactor = pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
            drawPos += new Vector3(0f, 0f, 0.4f + weapon.def.equippedDistanceOffset).RotatedBy(aimAngle) * distanceFactor;
            PawnRenderUtility.DrawEquipmentAiming(weapon, drawPos, aimAngle);
        }
    }
}
