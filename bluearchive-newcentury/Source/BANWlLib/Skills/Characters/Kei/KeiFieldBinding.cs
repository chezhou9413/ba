using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //凯伊场地绑定器，每次成功生成场地都加入独立的普通技能机会。
    public static class KeiFieldBinding
    {
        //接收EX场地生成通知，重复施法复用角色已有状态。
        public static void NotifySpawned(Ability ability, Thing_BattleFieldController field)
        {
            var profile = DefDatabase<SpecialSkillProfileDef>.AllDefsListForReading
                .FirstOrDefault(p => p.role == SpecialSkillRole.Kei && p.kei.sourceExAbility == ability.def);
            if (profile == null) return;
            var state = Hediff_SpecialSkillState.Find(ability.pawn, profile)
                ?? Hediff_SpecialSkillState.Create(ability.pawn, profile);
            Begin(state, field);
        }

        //新增场地不覆盖已有记录、待释放机会或正在进行的前摇。
        public static void Begin(Hediff_SpecialSkillState state, Thing_BattleFieldController field)
        {
            var runtime = KeiRuntimeState.Get(state);
            state.center = field.Position;
            runtime.Add(field, state.Now + field.RemainingTicks,
                SpecialCombatUtility.Power(state.pawn, state.profile.burstAttack) * state.profile.recordCapRatio);
            runtime.Sync(state);
        }

        //共用实际场地的格子枚举，范围外仍残留的短时增益不能继续贡献伤害。
        public static bool Contains(Hediff_SpecialSkillState state, Thing field, Pawn attacker)
        {
            if (field?.Spawned != true || attacker.Map != field.Map) return false;
            var fieldConfig = field.def.GetModExtension<BattleFieldControllerExtension>();
            if (!GenRadial.RadialCellsAround(field.Position, fieldConfig.radius, true).Contains(attacker.Position))
                return false;
            HediffDef buff = state.profile.kei.contributionHediff;
            return buff == null || attacker.health.hediffSet.HasHediff(buff);
        }
    }
}
