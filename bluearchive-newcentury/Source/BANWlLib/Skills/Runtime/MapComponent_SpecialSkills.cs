using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //地图特殊技能调度器，负责延迟攻击、状态索引和无人机绘制。
    public class MapComponent_SpecialSkills : MapComponent
    {
        private List<SpecialPendingAttack> pending = new List<SpecialPendingAttack>();
        private readonly HashSet<Hediff_SpecialSkillState> states = new HashSet<Hediff_SpecialSkillState>();
        public IEnumerable<Hediff_SpecialSkillState> States => states;

        //保存所属地图，供攻击发射和效果绘制使用。
        public MapComponent_SpecialSkills(Map map) : base(map) { }

        //注册已存在于当前地图的角色技能状态。
        public void Register(Hediff_SpecialSkillState state) { states.Add(state); }

        //移除离图角色的运行索引。
        public void Unregister(Hediff_SpecialSkillState state) { states.Remove(state); }

        //安排下一游戏时刻或指定时刻执行的攻击。
        public void Enqueue(SpecialPendingAttack attack) { pending.Add(attack); }

        //调试重置时取消指定角色尚未完成的特殊技能攻击。
        public void Cancel(Pawn pawn)
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                if (pending[i].caster != pawn) continue;
                pending[i].Complete();
                pending.RemoveAt(i);
            }
            foreach (var projectile in map.listerThings.AllThings.OfType<Projectile_SpecialSkill>()
                .Where(p => p.attack?.caster == pawn).ToList()) projectile.Destroy();
        }

        //按登记顺序执行到期攻击，保证同一时刻的多发攻击保持配置顺序。
        public override void MapComponentTick()
        {
            states.RemoveWhere(s => s.pawn.Dead || !s.pawn.Spawned || s.pawn.Map != map ||
                !s.pawn.health.hediffSet.hediffs.Contains(s));
            int now = Find.TickManager.TicksGame;
            for (int i = 0; i < pending.Count;)
            {
                if (pending[i].dueTick > now) { i++; continue; }
                SpecialPendingAttack attack = pending[i];
                pending.RemoveAt(i);
                if (attack.caster == null || attack.caster.Dead || attack.caster.Map != map)
                { attack.Complete(); continue; }
                if (attack.droneOrigin && (attack.state == null || !attack.state.Active))
                { attack.Complete(); continue; }
                SpecialCombatUtility.Launch(attack, map);
            }
        }

        //绘制当前有效的跟随无人机和前向拦截范围。
        public override void MapComponentDraw()
        {
            foreach (Hediff_SpecialSkillState state in states)
            {
                if (!state.pawn.Spawned || state.pawn.Dead) continue;
                if (state.profile.role == SpecialSkillRole.Shiroko && state.Active) DroneRenderer.Draw(state);
                if (state.profile.role == SpecialSkillRole.Hoshino && state.stage == 1 && state.Active)
                    HoshinoInterceptor.Draw(state);
            }
        }

        //读档后从角色已有状态重建地图索引，不重复创建状态。
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                foreach (var state in pawn.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>())
                    Register(state);
        }

        //保存所有尚未发射的攻击，状态索引由地图角色重建。
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pending, "specialPendingAttacks", LookMode.Deep);
        }
    }
}
