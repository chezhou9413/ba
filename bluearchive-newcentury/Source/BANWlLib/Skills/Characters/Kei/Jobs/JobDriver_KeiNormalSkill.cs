using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //凯伊自动普通技能工作，负责停步瞄准、前摇表现和单发弹丸释放。
    public class JobDriver_KeiNormalSkill : JobDriver
    {
        //通过工作引用定位蓄积来源，使重叠场地的释放互不覆盖。
        private Hediff_SpecialSkillState State => pawn.health.hediffSet.hediffs
            .OfType<Hediff_SpecialSkillState>().FirstOrDefault(s => s.keiNormalJob == job);

        //远程射击不占用敌方角色的工作预约。
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        //建立前摇与发射阶段，被打断时仅清理工作并保留待释放蓄积。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var state = State;
            if (state == null) { Log.Error("凯伊普通技能工作缺少蓄积状态"); yield break; }
            var config = state.profile.kei;
            this.FailOn(() => !state.releaseReady || !SpecialCombatUtility.CanAct(pawn) ||
                !SpecialCombatUtility.ValidEnemy(pawn, job.targetA.Pawn, state.profile.range));
            AddFinishAction(condition =>
            {
                if (state.keiNormalJob == job) state.keiNormalJob = null;
                if (pawn.stances.curStance is MultiShotAimStance) pawn.stances.CancelBusyStanceSoft();
            });
            Toil warmup = Toils_General.Wait(config.warmupTicks, TargetIndex.A);
            warmup.AddPreInitAction(() => pawn.stances.SetStance(new MultiShotAimStance(config.warmupTicks + 1, job.targetA)));
            if (config.warmupSound != null) warmup.PlaySustainerOrSound(config.warmupSound);
            if (config.warmupEffecter != null) warmup.WithEffect(config.warmupEffecter, TargetIndex.A);
            yield return warmup;
            Toil fire = ToilMaker.MakeToil("凯伊普通技能发射");
            fire.defaultCompleteMode = ToilCompleteMode.Instant;
            fire.initAction = () => KeiSkills.Release(state, job.targetA.Pawn);
            yield return fire;
        }
    }
}
