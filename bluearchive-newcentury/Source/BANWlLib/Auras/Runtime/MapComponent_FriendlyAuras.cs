using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Auras
{
    //地图光环管理器负责统一扫描、汇总同名光环，并增删受益状态。
    public sealed class MapComponent_FriendlyAuras : MapComponent
    {
        public const int RefreshIntervalTicks = 30;
        private readonly List<FriendlyAuraSource> sources = new List<FriendlyAuraSource>();
        private readonly Dictionary<HediffDef, float> desired = new Dictionary<HediffDef, float>();

        //绑定当前地图，不保存可从角色状态重建的覆盖缓存。
        public MapComponent_FriendlyAuras(Map map) : base(map) { }

        //每半个游戏秒在主线程统一结算，扫描期间不修改任何源状态集合。
        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % RefreshIntervalTicks != 0) return;
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            CollectSources(pawns);
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn target = pawns[i];
                desired.Clear();
                foreach (FriendlyAuraSource source in sources)
                {
                    if (source.strength > 0f && FriendlyAuraTargetUtility.Accepts(source.aura, target, source.radius))
                        Accumulate(source.aura.Props.effectHediff, source.strength);
                }
                Synchronize(target);
            }
        }

        //收集所有有效发射状态，受益状态不能递归充当光环源。
        private void CollectSources(IReadOnlyList<Pawn> pawns)
        {
            sources.Clear();
            foreach (Pawn pawn in pawns)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_FriendlyAuraEffect) continue;
                    HediffComp_FriendlyAura aura = hediff.TryGetComp<HediffComp_FriendlyAura>();
                    if (aura != null && aura.IsActive()) sources.Add(new FriendlyAuraSource(aura));
                }
            }
        }

        //同名受益默认取最强值，显式叠加时累加强度并由受益定义限制上限。
        private void Accumulate(HediffDef effect, float strength)
        {
            FriendlyAuraEffectExtension extension = effect.GetModExtension<FriendlyAuraEffectExtension>();
            float previous;
            desired.TryGetValue(effect, out previous);
            float combined = extension != null && extension.stackSeverity
                ? previous + strength : Mathf.Max(previous, strength);
            desired[effect] = Mathf.Min(combined, Mathf.Min(effect.maxSeverity, extension?.maximumSeverity ?? 10f));
        }

        //复用现有受益状态，移除失去覆盖的实例，并补充首次进入范围的受益状态。
        private void Synchronize(Pawn target)
        {
            List<Hediff> hediffs = target.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff_FriendlyAuraEffect effect = hediffs[i] as Hediff_FriendlyAuraEffect;
                if (effect == null) continue;
                float strength;
                if (!desired.TryGetValue(effect.def, out strength)) target.health.RemoveHediff(effect);
                else
                {
                    effect.Refresh(strength);
                    desired.Remove(effect.def);
                }
            }
            foreach (KeyValuePair<HediffDef, float> entry in desired)
            {
                Hediff_FriendlyAuraEffect effect = (Hediff_FriendlyAuraEffect)HediffMaker.MakeHediff(entry.Key, target);
                effect.Refresh(entry.Value);
                target.health.AddHediff(effect);
            }
        }
    }
}
