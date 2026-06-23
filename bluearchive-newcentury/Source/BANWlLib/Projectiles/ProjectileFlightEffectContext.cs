using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 投射物飞行特效上下文，负责把直线弹 XML 中的飞行特效参数传给 Effecter 子节点。
    public static class ProjectileFlightEffectContext
    {
        private static readonly Dictionary<int, ProjectileFlightEffectData> dataByThingId = new Dictionary<int, ProjectileFlightEffectData>();

        // 注册飞行特效参数，负责让附着子特效读取当前投射物的偏移和旋转规则。
        public static void Register(Thing projectile, ProjectileFlightEffectData data)
        {
            if (projectile == null)
            {
                return;
            }

            dataByThingId[projectile.thingIDNumber] = data;
        }

        // 读取飞行特效参数，负责按投射物实例取出对应配置。
        public static bool TryGet(Thing projectile, out ProjectileFlightEffectData data)
        {
            data = default;
            return projectile != null && dataByThingId.TryGetValue(projectile.thingIDNumber, out data);
        }

        // 清理飞行特效参数，负责避免投射物销毁后残留旧配置。
        public static void Clear(Thing projectile)
        {
            if (projectile != null)
            {
                dataByThingId.Remove(projectile.thingIDNumber);
            }
        }
    }

    // 投射物飞行特效数据，负责保存一次飞行期间的偏移和旋转规则。
    public struct ProjectileFlightEffectData
    {
        public bool rotateWithProjectile;
        public float offsetForward;
        public float offsetRight;
        public float offsetUp;
    }
}
