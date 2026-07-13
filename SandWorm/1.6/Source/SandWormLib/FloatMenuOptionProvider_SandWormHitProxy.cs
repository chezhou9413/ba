using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SandWormLib
{
    public class FloatMenuOptionProvider_SandWormHitProxy : FloatMenuOptionProvider
    {
        private static readonly List<Pawn> tmpPawns = new List<Pawn>();

        protected override bool Drafted => true;

        protected override bool Undrafted => false;

        protected override bool Multiselect => true;

        protected override bool MechanoidCanDo => true;

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            SandWormHitProxyThing proxy = clickedThing as SandWormHitProxyThing;
            if (proxy == null || proxy.Owner == null || proxy.Owner.Destroyed)
            {
                return null;
            }

            tmpPawns.Clear();
            FloatMenuOption option = context.IsMultiselect ? GetMultiselectOption(proxy, context) : GetSingleSelectOption(proxy, context);
            if (option == null)
            {
                return null;
            }

            if (!option.Disabled)
            {
                option.Priority = MenuOptionPriority.AttackEnemy;
                option.autoTakeable = true;
                option.autoTakeablePriority = 45f;
            }

            return option;
        }

        public override bool TargetThingValid(Thing thing, FloatMenuContext context)
        {
            if (!(thing is SandWormHitProxyThing proxy))
            {
                return false;
            }

            return proxy.Spawned && proxy.Map == context.map && proxy.Owner != null && !proxy.Owner.Destroyed;
        }

        private static FloatMenuOption GetSingleSelectOption(SandWormHitProxyThing proxy, FloatMenuContext context)
        {
            string label;
            string failStr;
            Action action = GetAttackAction(context.FirstSelectedPawn, proxy, out label, out failStr);
            FleckDef fleck = FloatMenuUtility.UseRangedAttack(context.FirstSelectedPawn) ? FleckDefOf.FeedbackShoot : FleckDefOf.FeedbackMelee;
            if (action == null)
            {
                if (!failStr.NullOrEmpty())
                {
                    return new FloatMenuOption((label ?? GetDefaultLabel(context.FirstSelectedPawn, proxy)) + ": " + failStr, null);
                }

                return null;
            }

            return new FloatMenuOption(label ?? GetDefaultLabel(context.FirstSelectedPawn, proxy), delegate
            {
                FleckMaker.Static(proxy.DrawPos, proxy.Map, fleck);
                action();
            }, MenuOptionPriority.AttackEnemy);
        }

        private static FloatMenuOption GetMultiselectOption(SandWormHitProxyThing proxy, FloatMenuContext context)
        {
            string label = null;
            foreach (Pawn pawn in context.ValidSelectedPawns)
            {
                if (GetAttackAction(pawn, proxy, out label, out var _) != null)
                {
                    tmpPawns.Add(pawn);
                }
            }

            if (tmpPawns.Count == 0)
            {
                return null;
            }

            FleckDef fleck = FloatMenuUtility.UseRangedAttack(tmpPawns[0]) ? FleckDefOf.FeedbackShoot : FleckDefOf.FeedbackMelee;
            return new FloatMenuOption(label ?? GetDefaultLabel(tmpPawns[0], proxy), delegate
            {
                for (int i = 0; i < tmpPawns.Count; i++)
                {
                    FleckMaker.Static(proxy.DrawPos, proxy.Map, fleck);
                    GetAttackAction(tmpPawns[i], proxy, out var _, out var _)?.Invoke();
                }
            }, MenuOptionPriority.AttackEnemy);
        }

        private static Action GetAttackAction(Pawn pawn, SandWormHitProxyThing proxy, out string label, out string failStr)
        {
            failStr = null;
            label = GetDefaultLabel(pawn, proxy);

            if (proxy.Owner == null || proxy.Owner.Destroyed || !proxy.Spawned)
            {
                failStr = "CannotHitTarget".Translate().CapitalizeFirst();
                return null;
            }

            if (FloatMenuUtility.UseRangedAttack(pawn))
            {
                label = "FireAt".Translate(proxy.Owner.Label, proxy.Owner);
                return FloatMenuUtility.GetRangedAttackAction(pawn, proxy, out failStr);
            }

            label = "MeleeAttack".Translate(proxy.Owner.Label, proxy.Owner);
            return FloatMenuUtility.GetMeleeAttackAction(pawn, proxy, out failStr);
        }

        private static string GetDefaultLabel(Pawn pawn, SandWormHitProxyThing proxy)
        {
            if (FloatMenuUtility.UseRangedAttack(pawn))
            {
                return "FireAt".Translate(proxy.Owner.Label, proxy.Owner);
            }

            return "Attack".Translate(proxy.Owner.Label, proxy.Owner);
        }
    }
}
