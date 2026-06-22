using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.comp
{
    /// <summary>
    /// 工具类：在任何地方调用，播放特效后1秒生成pawn
    /// 不需要挂载在Thing上
    /// </summary>
    public static class SpawnPawnWithEffectUtility
    {
        /// <summary>
        /// 在指定位置播放特效，1秒后生成pawn
        /// </summary>
        /// <param name="position">生成位置</param>
        /// <param name="map">地图</param>
        /// <param name="effecterDef">特效定义</param>
        /// <param name="pawnKindDef">要生成的pawn类型</param>
        /// <param name="faction">pawn所属的派系（可选）</param>
        /// <param name="spawnRadius">搜索可站立位置的半径</param>
        public static void SpawnPawnWithEffect(IntVec3 position, Map map, EffecterDef effecterDef, IEnumerator<Thing> things, Faction faction = null, int spawnRadius = 3)
        {
            if (map == null)
            {
                return;
            }

            // 创建延迟生成任务
            DelayedSpawnTask task = new DelayedSpawnTask
            {
                position = position,
                map = map,
                effecterDef = effecterDef,
                things = things,
                faction = faction,
                spawnRadius = spawnRadius,
                startTick = Find.TickManager.TicksGame
            };

            // 注册到GameComponent进行管理
            SpawnPawnWithEffectManager manager = Current.Game.GetComponent<SpawnPawnWithEffectManager>();
            if (manager == null)
            {
                manager = new SpawnPawnWithEffectManager(Current.Game);
                Current.Game.components.Add(manager);
            }

            manager.AddTask(task);

            // 立即开始播放特效
            if (effecterDef != null)
            {
                task.effecter = effecterDef.Spawn(position, map);
            }
        }
    }

    /// <summary>
    /// 延迟生成任务数据
    /// </summary>
    public class DelayedSpawnTask
    {
        public IntVec3 position;
        public Map map;
        public EffecterDef effecterDef;
        public IEnumerator<Thing> things;
        public Faction faction;
        public int spawnRadius;
        public int startTick;
        public Effecter effecter;
        public bool completed = false;
    }

    /// <summary>
    /// GameComponent：管理所有延迟生成任务
    /// </summary>
    public class SpawnPawnWithEffectManager : GameComponent
    {
        private System.Collections.Generic.List<DelayedSpawnTask> activeTasks = new System.Collections.Generic.List<DelayedSpawnTask>();

        public SpawnPawnWithEffectManager(Game game)
        {
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(DelayedSpawnTask task)
        {
            if (task != null && !activeTasks.Contains(task))
            {
                activeTasks.Add(task);
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public override void GameComponentTick()
        {
            base.GameComponentTick();

            // 遍历所有活动任务
            for (int i = activeTasks.Count - 1; i >= 0; i--)
            {
                DelayedSpawnTask task = activeTasks[i];

                if (task.completed || task.map == null)
                {
                    activeTasks.RemoveAt(i);
                    if (task.effecter != null)
                    {
                        task.effecter.Cleanup();
                    }
                    continue;
                }

                // 更新特效
                if (task.effecter != null)
                {
                    TargetInfo targetInfo = new TargetInfo(task.position, task.map);
                    task.effecter.EffectTick(targetInfo, targetInfo);
                }

                // 检查是否已经过了1秒（60 ticks）
                if (Find.TickManager.TicksGame >= task.startTick + 60)
                {
                    // 生成pawn
                    SpawnPawn(task);
                    task.completed = true;
                    activeTasks.RemoveAt(i);

                    // 清理特效
                    if (task.effecter != null)
                    {
                        task.effecter.Cleanup();
                        task.effecter = null;
                    }
                }
            }
        }

        /// <summary>
        /// 生成pawn
        /// </summary>
        private void SpawnPawn(DelayedSpawnTask task)
        {
            // 生成pawn
            IEnumerator<Thing> enumerator = task.things;

            // 查找生成pawn的位置
            IntVec3 spawnPosition = FindSpawnPosition(task.position, task.map, task.spawnRadius);

            if (spawnPosition.IsValid)
            {
                while (enumerator.MoveNext())  // 循环直到没有下一个元素
                {
                    Thing currentThing = enumerator.Current;
                    GenSpawn.Spawn(currentThing, spawnPosition, task.map, Rot4.Random);
                }
            }
        }

        /// <summary>
        /// 查找pawn的生成位置
        /// </summary>
        private IntVec3 FindSpawnPosition(IntVec3 center, Map map, int radius)
        {
            // 首先尝试中心位置
            if (center.Standable(map) && center.GetFirstPawn(map) == null)
            {
                return center;
            }

            // 在附近寻找可站立的位置
            for (int r = 1; r <= radius; r++)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, r, true))
                {
                    if (cell.InBounds(map) && cell.Standable(map) && cell.GetFirstPawn(map) == null)
                    {
                        return cell;
                    }
                }
            }
            return center;
        }

        /// <summary>
        /// 保存/加载数据
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }

}
