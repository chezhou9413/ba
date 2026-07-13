using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormChallengeRuntimeModifiers 负责把选中的 XML 词条效果汇总成挑战运行时可直接使用的参数。
    public sealed class SandWormChallengeRuntimeModifiers : IExposable
    {
        public int extraSmallWormCount;
        public bool smallWormHeadInstantKill;
        public float smallWormHitPointFactor = 1f;
        public float chargeCooldownFactor = 1f;
        public float maxIncomingDamagePerHit = -1f;
        public float pawnRangeFactor = 1f;
        public int pawnMoveSuppressionLevel;
        public bool enableShockwaveAttack;
        public float shockwaveCooldownFactor = 1f;
        public float shockwaveLaneWidthFactor = 1f;
        public float shockwaveDamageFactor = 1f;
        public int pawnAimSuppressionLevel;
        public int resonanceEscalationLevel;

        // ExposeData 负责把本次挑战词条汇总后的运行参数写入存档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref extraSmallWormCount, "extraSmallWormCount", 0);
            Scribe_Values.Look(ref smallWormHeadInstantKill, "smallWormHeadInstantKill", defaultValue: false);
            Scribe_Values.Look(ref smallWormHitPointFactor, "smallWormHitPointFactor", 1f);
            Scribe_Values.Look(ref chargeCooldownFactor, "chargeCooldownFactor", 1f);
            Scribe_Values.Look(ref maxIncomingDamagePerHit, "maxIncomingDamagePerHit", -1f);
            Scribe_Values.Look(ref pawnRangeFactor, "pawnRangeFactor", 1f);
            Scribe_Values.Look(ref pawnMoveSuppressionLevel, "pawnMoveSuppressionLevel", 0);
            Scribe_Values.Look(ref enableShockwaveAttack, "enableShockwaveAttack", defaultValue: false);
            Scribe_Values.Look(ref shockwaveCooldownFactor, "shockwaveCooldownFactor", 1f);
            Scribe_Values.Look(ref shockwaveLaneWidthFactor, "shockwaveLaneWidthFactor", 1f);
            Scribe_Values.Look(ref shockwaveDamageFactor, "shockwaveDamageFactor", 1f);
            Scribe_Values.Look(ref pawnAimSuppressionLevel, "pawnAimSuppressionLevel", 0);
            Scribe_Values.Look(ref resonanceEscalationLevel, "resonanceEscalationLevel", 0);
        }

        // FromRisks 负责解析已选词条的效果列表，并合并为本次挑战的最终参数。
        public static SandWormChallengeRuntimeModifiers FromRisks(IEnumerable<SandWormChallengeRiskDef> risks)
        {
            SandWormChallengeRuntimeModifiers modifiers = new SandWormChallengeRuntimeModifiers();
            if (risks == null)
            {
                return modifiers;
            }

            foreach (SandWormChallengeRiskDef risk in risks)
            {
                if (risk?.effects == null)
                {
                    continue;
                }

                for (int i = 0; i < risk.effects.Count; i++)
                {
                    modifiers.Apply(risk.effects[i]);
                }
            }

            return modifiers;
        }

        // Apply 负责把单个词条效果写入累计参数。
        private void Apply(SandWormChallengeRiskEffect effect)
        {
            if (effect == null || effect.effectType.NullOrEmpty())
            {
                return;
            }

            switch (effect.effectType)
            {
                case "SpawnSmallWorm":
                    extraSmallWormCount += effect.count;
                    break;
                case "SmallWormHeadInstantKill":
                    smallWormHeadInstantKill = true;
                    break;
                case "SmallWormHitPointFactor":
                    if (effect.factor > 0f)
                    {
                        smallWormHitPointFactor = Mathf.Max(smallWormHitPointFactor, effect.factor);
                    }
                    break;
                case "ChargeCooldownFactor":
                    if (effect.factor > 0f)
                    {
                        chargeCooldownFactor = Mathf.Min(chargeCooldownFactor, effect.factor);
                    }
                    break;
                case "MaxIncomingDamagePerHit":
                    if (effect.count > 0)
                    {
                        maxIncomingDamagePerHit = maxIncomingDamagePerHit > 0f
                            ? Mathf.Min(maxIncomingDamagePerHit, effect.count)
                            : effect.count;
                    }
                    break;
                case "PawnRangeFactor":
                    if (effect.factor > 0f)
                    {
                        pawnRangeFactor = Mathf.Min(pawnRangeFactor, effect.factor);
                    }
                    break;
                case "PawnMoveSuppression":
                    if (effect.count > 0)
                    {
                        pawnMoveSuppressionLevel = Mathf.Max(pawnMoveSuppressionLevel, effect.count);
                    }
                    break;
                case "EnableShockwaveAttack":
                    enableShockwaveAttack = true;
                    break;
                case "ShockwaveCooldownFactor":
                    if (effect.factor > 0f)
                    {
                        shockwaveCooldownFactor = Mathf.Min(shockwaveCooldownFactor, effect.factor);
                    }
                    break;
                case "ShockwaveLaneWidthFactor":
                    if (effect.factor > 0f)
                    {
                        shockwaveLaneWidthFactor = Mathf.Max(shockwaveLaneWidthFactor, effect.factor);
                    }
                    break;
                case "ShockwaveDamageFactor":
                    if (effect.factor > 0f)
                    {
                        shockwaveDamageFactor = Mathf.Max(shockwaveDamageFactor, effect.factor);
                    }
                    break;
                case "PawnAimSuppression":
                    if (effect.count > 0)
                    {
                        pawnAimSuppressionLevel = Mathf.Max(pawnAimSuppressionLevel, effect.count);
                    }
                    break;
                case "ResonanceEscalation":
                    if (effect.count > 0)
                    {
                        resonanceEscalationLevel = Mathf.Max(resonanceEscalationLevel, effect.count);
                    }
                    break;
            }
        }
    }
}
