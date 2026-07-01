using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 投射物多段延迟地图组件，负责在子弹命中后按目标所在地图托管后续伤害段。
    public class ProjectileMultiHitDelayComponent : MapComponent
    {
        private List<PendingProjectileMultiHitDamage> pendingDamages = new List<PendingProjectileMultiHitDamage>();

        // 创建地图延迟组件，负责绑定当前地图的多段子弹伤害队列。
        public ProjectileMultiHitDelayComponent(Map map) : base(map)
        {
        }

        // 地图每 tick 更新，负责触发到期的多段子弹伤害。
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (pendingDamages.NullOrEmpty())
            {
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            for (int i = pendingDamages.Count - 1; i >= 0; i--)
            {
                PendingProjectileMultiHitDamage pendingDamage = pendingDamages[i];
                if (pendingDamage == null || pendingDamage.fireAtTick > currentTick)
                {
                    continue;
                }

                pendingDamages.RemoveAt(i);
                pendingDamage.Apply();
            }
        }

        // 保存和读取地图延迟队列，负责支持多段子弹延迟期间存读档。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingDamages, "pendingProjectileMultiHitDamages", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pendingDamages == null)
            {
                pendingDamages = new List<PendingProjectileMultiHitDamage>();
            }
        }

        // 入队多段子弹伤害，负责优先使用单段延迟配置，没有单段延迟时再按统一间隔排队。
        public static void Queue(ProjectileMultiHitImpactPatch.ProjectileMultiHitImpactState state)
        {
            if (state == null || state.extraDamages.NullOrEmpty())
            {
                return;
            }

            Map map = ResolveQueueMap(state);
            if (map == null)
            {
                Log.Error("[BANW] 多段子弹伤害没有可用地图，无法延迟触发。");
                return;
            }

            ProjectileMultiHitDelayComponent component = map.GetComponent<ProjectileMultiHitDelayComponent>();
            if (component == null)
            {
                Log.Error("[BANW] 地图缺少 ProjectileMultiHitDelayComponent，无法延迟触发多段子弹伤害。");
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            for (int i = 0; i < state.extraDamages.Count; i++)
            {
                ProjectileExtraDamageConfig config = state.extraDamages[i];
                int delayTicks = ResolveDelayTicks(config, i, state.damageIntervalTicks);
                component.pendingDamages.Add(new PendingProjectileMultiHitDamage
                {
                    fireAtTick = currentTick + delayTicks,
                    projectileDef = state.projectileDef,
                    launcher = state.launcher,
                    target = state.target,
                    config = config,
                    canHitOwnPawn = state.canHitOwnPawn,
                    canHitOwnBuilding = state.canHitOwnBuilding,
                    weaponBaseAttack = state.weaponBaseAttack,
                    armorPenetration = state.armorPenetration
                });
            }
        }

        // 解析延迟队列所属地图，负责优先把伤害绑定到命中目标当前地图。
        private static Map ResolveQueueMap(ProjectileMultiHitImpactPatch.ProjectileMultiHitImpactState state)
        {
            if (state == null)
            {
                return null;
            }

            return state.target?.MapHeld ?? state.launcher?.MapHeld;
        }

        // 解析单段延迟，负责让段内 delayTicks 覆盖外层统一间隔。
        private static int ResolveDelayTicks(ProjectileExtraDamageConfig config, int index, int intervalTicks)
        {
            if (config != null && config.delayTicks >= 0)
            {
                return config.delayTicks;
            }

            return index * intervalTicks;
        }
    }

    // 待触发投射物多段伤害，负责保存单段延迟伤害的施法者、目标和公式参数。
    public class PendingProjectileMultiHitDamage : IExposable
    {
        public int fireAtTick;
        public ThingDef projectileDef;
        public Pawn launcher;
        public Thing target;
        public ProjectileExtraDamageConfig config;
        public bool canHitOwnPawn;
        public bool canHitOwnBuilding;
        public float weaponBaseAttack;
        public float armorPenetration;

        // 触发单段延迟伤害，负责还原为普通多段命中上下文并进入统一伤害链。
        public void Apply()
        {
            if (launcher == null || target == null || config == null)
            {
                return;
            }

            ProjectileMultiHitImpactPatch.ApplyExtraDamage(new ProjectileMultiHitImpactPatch.ProjectileMultiHitImpactState
            {
                projectileDef = projectileDef,
                launcher = launcher,
                target = target,
                canHitOwnPawn = canHitOwnPawn,
                canHitOwnBuilding = canHitOwnBuilding,
                weaponBaseAttack = weaponBaseAttack,
                armorPenetration = armorPenetration
            }, config);
        }

        // 保存和读取待触发伤害，负责支持延迟期间存读档。
        public void ExposeData()
        {
            Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_References.Look(ref launcher, "launcher");
            Scribe_References.Look(ref target, "target");
            Scribe_Deep.Look(ref config, "config");
            Scribe_Values.Look(ref canHitOwnPawn, "canHitOwnPawn", false);
            Scribe_Values.Look(ref canHitOwnBuilding, "canHitOwnBuilding", false);
            Scribe_Values.Look(ref weaponBaseAttack, "weaponBaseAttack", 0f);
            Scribe_Values.Look(ref armorPenetration, "armorPenetration", 0f);
        }
    }
}
