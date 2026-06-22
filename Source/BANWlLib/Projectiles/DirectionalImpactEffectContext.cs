using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 方向命中特效上下文，负责在投射物命中时把弹道方向传给 Effecter 子节点。
    public static class DirectionalImpactEffectContext
    {
        private static readonly Dictionary<int, DirectionalImpactEffectData> dataByThingId = new Dictionary<int, DirectionalImpactEffectData>();

        // 注册命中特效参数，负责让同一帧触发的 SubEffecter 能读取命中方向。
        public static void Register(Thing target, Vector3 direction, float speed, float offsetForward, float offsetUp)
        {
            if (target == null)
            {
                return;
            }

            Vector3 normalizedDirection = direction.Yto0();
            if (normalizedDirection.sqrMagnitude < 0.0001f)
            {
                normalizedDirection = Vector3.forward;
            }

            normalizedDirection.Normalize();
            dataByThingId[target.thingIDNumber] = new DirectionalImpactEffectData
            {
                direction = normalizedDirection,
                speed = speed,
                offsetForward = offsetForward,
                offsetUp = offsetUp
            };
        }

        // 读取命中特效参数，负责优先按目标 Thing 取出当前弹道信息。
        public static bool TryGet(Thing target, out DirectionalImpactEffectData data)
        {
            data = default;
            return target != null && dataByThingId.TryGetValue(target.thingIDNumber, out data);
        }

        // 清理命中特效参数，负责避免后续无关特效复用旧方向。
        public static void Clear(Thing target)
        {
            if (target != null)
            {
                dataByThingId.Remove(target.thingIDNumber);
            }
        }
    }

    // 方向命中特效数据，负责保存一次命中需要的飞行方向、速度和生成偏移。
    public struct DirectionalImpactEffectData
    {
        public Vector3 direction;
        public float speed;
        public float offsetForward;
        public float offsetUp;
    }
}
