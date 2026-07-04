using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.BaClass
{
    // 旋转偏移 Mote 延迟组件，负责在地图 tick 中托管 SubEffecter_SprayerTriggeredRotatedOffset 的延迟生成任务。
    public class RotatedOffsetMoteDelayComponent : MapComponent
    {
        private List<PendingRotatedOffsetMote> pendingMotes = new List<PendingRotatedOffsetMote>();

        // 创建地图延迟组件，负责绑定当前地图的旋转偏移 Mote 队列。
        public RotatedOffsetMoteDelayComponent(Map map) : base(map)
        {
        }

        // 地图每 tick 更新，负责生成到期的延迟 Mote。
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            SpawnDueMotes(Find.TickManager.TicksGame);
        }

        // 保存和读取延迟队列，负责让延迟 Mote 在存读档后继续生成。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingMotes, "pendingRotatedOffsetMotes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pendingMotes == null)
            {
                pendingMotes = new List<PendingRotatedOffsetMote>();
            }
        }

        // 入队延迟 Mote，负责记录触发时已经计算好的位置、缩放和旋转角度。
        public static void Queue(Map map, ThingDef moteDef, Vector3 position, float scale, float? rotationAngle, int delayTicks)
        {
            if (map == null || moteDef == null)
            {
                Log.Error("[BANW] 旋转偏移 Mote 入队缺少地图或 moteDef。");
                return;
            }

            RotatedOffsetMoteDelayComponent component = map.GetComponent<RotatedOffsetMoteDelayComponent>();
            if (component == null)
            {
                Log.Error("[BANW] 地图缺少 RotatedOffsetMoteDelayComponent，无法延迟生成旋转偏移 Mote。");
                return;
            }

            component.pendingMotes.Add(new PendingRotatedOffsetMote
            {
                fireAtTick = Find.TickManager.TicksGame + delayTicks,
                moteDef = moteDef,
                position = position,
                scale = scale,
                rotationAngle = rotationAngle ?? 0f,
                hasRotation = rotationAngle.HasValue
            });
        }

        // 生成到期 Mote，负责倒序移除已处理任务。
        private void SpawnDueMotes(int currentTick)
        {
            if (pendingMotes.NullOrEmpty())
            {
                return;
            }

            for (int i = pendingMotes.Count - 1; i >= 0; i--)
            {
                PendingRotatedOffsetMote pendingMote = pendingMotes[i];
                if (pendingMote == null || pendingMote.fireAtTick > currentTick)
                {
                    continue;
                }

                pendingMotes.RemoveAt(i);
                pendingMote.Spawn(map);
            }
        }
    }
}
