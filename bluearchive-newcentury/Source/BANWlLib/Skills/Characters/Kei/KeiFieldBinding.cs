using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //凯伊场地绑定器，负责把现有EX创建的实际场地连接到独立蓄积状态。
    public static class KeiFieldBinding
    {
        //接收EX场地生成通知，重叠施法使用独立状态保留各自蓄积与释放机会。
        public static void NotifySpawned(Ability ability, Thing_BattleFieldController field)
        {
            var profile = DefDatabase<SpecialSkillProfileDef>.AllDefsListForReading
                .FirstOrDefault(p => p.role == SpecialSkillRole.Kei && p.kei.sourceExAbility == ability.def);
            if (profile == null) return;
            var state = Hediff_SpecialSkillState.Find(ability.pawn, profile)
                ?? Hediff_SpecialSkillState.Create(ability.pawn, profile);
            if (state.Active || state.releaseReady || state.pendingAttacks > 0)
                state = Hediff_SpecialSkillState.Create(ability.pawn, profile, false);
            Begin(state, field);
        }

        //根据已生成场地的真实剩余时间建立记录，攻击基数只在此刻取值。
        public static void Begin(Hediff_SpecialSkillState state, Thing_BattleFieldController field)
        {
            state.center = field.Position;
            state.field = field;
            state.recorded = 0f;
            state.recordCap = SpecialCombatUtility.Power(state.pawn, state.profile.burstAttack) * state.profile.recordCapRatio;
            state.endTick = state.Now + field.RemainingTicks;
            state.releaseReady = false;
        }

        //共用实际场地的格子枚举，范围外仍残留的短时增益不能继续贡献伤害。
        public static bool Contains(Hediff_SpecialSkillState state, Pawn attacker)
        {
            if (state.field?.Spawned != true || attacker.Map != state.field.Map) return false;
            var fieldConfig = state.field.def.GetModExtension<BattleFieldControllerExtension>();
            if (!GenRadial.RadialCellsAround(state.field.Position, fieldConfig.radius, true).Contains(attacker.Position))
                return false;
            HediffDef buff = state.profile.kei.contributionHediff;
            return buff == null || attacker.health.hediffSet.HasHediff(buff);
        }
    }
}
