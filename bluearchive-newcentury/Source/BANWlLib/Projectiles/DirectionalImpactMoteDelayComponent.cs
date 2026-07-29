using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 方向命中 Mote 延迟组件，负责按 SubEffecterDef 的 initialDelayTicks 托管待播放特效。
    public class DirectionalImpactMoteDelayComponent : MapComponent
    {
        private List<PendingDirectionalImpactMote> pendingMotes = new List<PendingDirectionalImpactMote>();

        // 创建延迟组件，负责绑定当前地图的方向命中特效队列。
        public DirectionalImpactMoteDelayComponent(Map map) : base(map)
        {
        }

        // 地图每 tick 更新，负责生成已经到达播放时间的 Mote。
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (pendingMotes.NullOrEmpty())
            {
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            for (int i = pendingMotes.Count - 1; i >= 0; i--)
            {
                PendingDirectionalImpactMote pendingMote = pendingMotes[i];
                if (pendingMote == null || pendingMote.fireAtTick > currentTick)
                {
                    continue;
                }

                pendingMotes.RemoveAt(i);
                pendingMote.Spawn(map);
            }
        }

        // 保存和读取延迟队列，负责让尚未播放的方向命中特效支持存读档。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingMotes, "pendingDirectionalImpactMotes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pendingMotes == null)
            {
                pendingMotes = new List<PendingDirectionalImpactMote>();
            }
        }

        // 立即生成或排队 Mote，负责在触发时固定随机缩放、旋转速度、方向和命中位置。
        public static void SpawnOrQueue(Map map, Vector3 spawnPosition, DirectionalImpactEffectData data, SubEffecterDef def, int overrideSpawnTick)
        {
            PendingDirectionalImpactMote pendingMote = new PendingDirectionalImpactMote
            {
                fireAtTick = Find.TickManager.TicksGame + Mathf.Max(0, def.initialDelayTicks),
                moteDef = def.moteDef,
                spawnPosition = spawnPosition,
                direction = data.direction,
                speed = data.speed,
                scale = def.scale.RandomInRange,
                rotationRate = def.rotationRate.RandomInRange,
                overrideSpawnTick = overrideSpawnTick < 0 ? -1 : overrideSpawnTick + Mathf.Max(0, def.initialDelayTicks)
            };

            if (def.initialDelayTicks <= 0)
            {
                pendingMote.Spawn(map);
                return;
            }

            DirectionalImpactMoteDelayComponent component = map.GetComponent<DirectionalImpactMoteDelayComponent>();
            if (component == null)
            {
                Log.Error("[BANW] 地图缺少 DirectionalImpactMoteDelayComponent，无法延迟播放方向命中特效。");
                return;
            }

            component.pendingMotes.Add(pendingMote);
        }
    }
}
