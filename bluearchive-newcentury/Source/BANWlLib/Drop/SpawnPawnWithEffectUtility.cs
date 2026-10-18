using newpro;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.Drop
{
    //播放特效并延迟生成 Pawn 或物品。
    public static class SpawnPawnWithEffectUtility
    {
        //在目标附近播放特效并延迟生成，time 单位为 tick，60 tick 为 1 秒。
        //无特效时直接生成；spawnRadius 为搜索半径，默认 3 格。
        public static void SpawnPawnWithEffect(
            IntVec3 position,
            Map map,
            EffecterDef effecterDef,
            List<Thing> things,
            int time,
            Faction faction = null,
            int spawnRadius = 3)
        {
            if (map == null)
            {
                return;
            }

            //没有特效时直接生成。
            if (effecterDef == null)
            {
                if (things != null && things.Count > 0)
                {
                    IntVec3 spawnPosition = SpawnPawnWithEffectManager.FindSpawnPosition(position, map, spawnRadius);
                    if (spawnPosition.IsValid)
                    {
                        foreach (Thing thing in things)
                        {
                            GenSpawn.Spawn(thing, spawnPosition, map, Rot4.Random);
                        }
                    }
                }
                return;
            }

            if (things == null || things.Count == 0)
            {
                return;
            }

            DelayedSpawnTask task = new DelayedSpawnTask
            {
                position = position,
                map = map,
                effecterDef = effecterDef,
                things = things,
                faction = faction,
                spawnRadius = spawnRadius,
                startTick = Find.TickManager.TicksGame,  // 记录任务创建时间
                time = time
            };

            if (Current.Game == null)
            {
                return;
            }

            SpawnPawnWithEffectManager manager = Current.Game.GetComponent<SpawnPawnWithEffectManager>();
            if (manager == null)
            {
                // 如果管理器不存在，动态创建并注册
                manager = new SpawnPawnWithEffectManager(Current.Game);
                Current.Game.components.Add(manager);
            }

            //避开其他待生成任务的位置。
            IntVec3 actualSpawnPosition = manager.FindSpawnPositionWithOffset(position, map, spawnRadius);
            task.position = actualSpawnPosition;

            manager.AddTask(task);

            // 参考原版 LevelVisualComp 的实现：手动维护特效，每帧调用 EffectTick
            try
            {
                // Spawn() 方法会自动创建特效实例及其所有子特效
                task.effecter = effecterDef.Spawn(actualSpawnPosition, map);

                if (task.effecter != null)
                {
                    // 设置特效播放时长（300 Tick ≈ 5 秒）
                    //播放时长需要覆盖所有延迟子特效。
                    task.effecterTimer = 300;

                    //立即调用 EffectTick，显示首帧特效。
                    TargetInfo targetInfo = new TargetInfo(actualSpawnPosition, map);
                    task.effecter.EffectTick(targetInfo, targetInfo);
                }
            }
            catch (System.Exception)
            {
                // 特效创建失败不影响后续生成逻辑
            }
        }
    }

    //保存单个延迟生成任务，由 SpawnPawnWithEffectManager 管理。
    public class DelayedSpawnTask
    {
        //实际生成位置（已经过偏移计算）
        public IntVec3 position;

        //目标地图
        public Map map;

        //特效定义
        public EffecterDef effecterDef;

        //要生成的 Thing 列表
        public List<Thing> things;

        //Pawn 所属派系
        public Faction faction;

        //搜索可站立位置的半径
        public int spawnRadius;

        //任务创建时的游戏 Tick
        public int startTick;

        //当前播放的特效实例
        public Effecter effecter;

        //任务是否已完成（Thing 已生成）
        public bool completed = false;

        //特效剩余播放 tick，使用独立计时器，不依赖 effecter.ticksLeft。
        public int effecterTimer = 0;

        //延迟生成时间（单位：Tick，60 Tick = 1 秒）
        public int time;
    }

    //管理延迟生成任务及其特效。
    public class SpawnPawnWithEffectManager : GameComponent
    {
        //当前活动的延迟生成任务列表
        private List<DelayedSpawnTask> activeTasks = new List<DelayedSpawnTask>();

        //创建游戏级生成任务管理器。
        public SpawnPawnWithEffectManager(Game game)
        {
        }

        //添加新的延迟生成任务
        public void AddTask(DelayedSpawnTask task)
        {
            if (task != null && !activeTasks.Contains(task))
            {
                activeTasks.Add(task);
            }
        }

        //获取当前活动任务数量（调试用）
        public int GetTaskCount()
        {
            return activeTasks.Count;
        }

        public void CancelTasksForMap(Map map)
        {
            if (map == null)
            {
                return;
            }

            for (int i = activeTasks.Count - 1; i >= 0; i--)
            {
                DelayedSpawnTask task = activeTasks[i];
                if (task == null || task.map != map)
                {
                    continue;
                }

                task.effecter?.Cleanup();
                task.effecter = null;
                activeTasks.RemoveAt(i);
            }
        }

        //推进空投和延迟生成，维护特效并清理已完成任务。
        public override void GameComponentTick()
        {
            base.GameComponentTick();

            //处理空投队列。
            if (!UiMapData.uiclose)
            {
                PawnDropHelper.dropawnAct();
            }

            //倒序遍历，便于移除已完成任务。
            for (int i = activeTasks.Count - 1; i >= 0; i--)
            {
                DelayedSpawnTask task = activeTasks[i];

                if (task == null || task.map == null || task.map.Disposed)
                {
                    activeTasks.RemoveAt(i);
                    task.effecter?.Cleanup();
                    continue;
                }

                // 参考原版 LevelVisualComp：每帧调用 EffectTick 保持特效活跃
                if (task.effecter != null && task.effecterTimer > 0)
                {
                    task.effecterTimer--;
                    TargetInfo targetInfo = new TargetInfo(task.position, task.map);
                    task.effecter.EffectTick(targetInfo, targetInfo);

                    // 维护需要持续调用 Maintain() 的 Mote
                    // 这类 Mote 设置了 needsMaintenance=true，不维护会自动消失
                    MaintainMotesAtPosition(task.position, task.map);
                }

                bool shouldSpawnPawn = Find.TickManager.TicksGame >= task.startTick + task.time;

                if (shouldSpawnPawn && !task.completed)
                {
                    try
                    {
                        SpawnPawn(task);
                        task.completed = true;
                    }
                    catch (System.Exception)
                    {
                        // 生成失败也标记完成，避免无限重试
                        task.completed = true;
                    }
                }

                // 特效播放完毕后移除任务
                if (task.effecter != null && task.effecterTimer <= 0)
                {
                    task.effecter.Cleanup();
                    task.effecter = null;
                    activeTasks.RemoveAt(i);
                }
                // 特效意外丢失且计时器归零，也移除任务
                else if (task.effecter == null && task.effecterTimer <= 0)
                {
                    activeTasks.RemoveAt(i);
                }
            }
        }

        //生成任务中的 Pawn 和物品。
        private void SpawnPawn(DelayedSpawnTask task)
        {
            List<Thing> things = task.things;
            IntVec3 spawnPosition = task.position;

            if (!spawnPosition.IsValid)
            {
                return;
            }

            foreach (Thing thing in things)
            {
                //跳过已生成的 Pawn。
                if (thing is Pawn pawn)
                {
                    // 已在目标地图上，跳过
                    if (pawn.Spawned && pawn.Map == task.map)
                    {
                        Log.Warning($"尝试生成已经存在的角色: {pawn.Name}，跳过生成");
                        continue;
                    }

                    // 在其他地图上，先移除
                    if (pawn.Spawned && pawn.Map != task.map)
                    {
                        pawn.DeSpawn(DestroyMode.Vanish);
                    }
                }

                GenSpawn.Spawn(thing, spawnPosition, task.map, Rot4.Random);
            }
        }

        //由中心向外搜索未被 Pawn 占用的可站立格，找不到时返回中心位置。
        public static IntVec3 FindSpawnPosition(IntVec3 center, Map map, int radius)
        {
            // 优先尝试中心位置
            if (center.Standable(map) && center.GetFirstPawn(map) == null)
            {
                return center;
            }

            // 从内到外搜索
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

            // 找不到合适位置，返回中心（可能会导致重叠）
            return center;
        }

        //查找生成位置，并避开其他活动任务占用的位置。
        public IntVec3 FindSpawnPositionWithOffset(IntVec3 center, Map map, int radius)
        {
            IntVec3 basePosition = FindSpawnPosition(center, map, radius);

            //距其他任务生成位置不超过 1 格时视为冲突。
            bool hasConflict = false;
            foreach (DelayedSpawnTask otherTask in activeTasks)
            {
                if (otherTask.map == map && otherTask.position.IsValid)
                {
                    float distance = otherTask.position.DistanceTo(basePosition);
                    if (distance <= 1.0f)
                    {
                        hasConflict = true;
                        break;
                    }
                }
            }

            //冲突时尝试附近空位。
            if (hasConflict)
            {
                // 扩大搜索范围寻找不冲突的位置
                for (int r = 1; r <= radius + 2; r++)
                {
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(basePosition, r, true))
                    {
                        if (!cell.InBounds(map) || !cell.Standable(map) || cell.GetFirstPawn(map) != null)
                        {
                            continue;
                        }

                        // 检查这个位置是否与其他任务冲突
                        bool cellHasConflict = false;
                        foreach (DelayedSpawnTask otherTask in activeTasks)
                        {
                            if (otherTask.map == map && otherTask.position.IsValid)
                            {
                                float distance = otherTask.position.DistanceTo(cell);
                                if (distance <= 1.0f)
                                {
                                    cellHasConflict = true;
                                    break;
                                }
                            }
                        }

                        // 找到不冲突的位置
                        if (!cellHasConflict)
                        {
                            return cell;
                        }
                    }
                }
            }

            return basePosition;
        }

        //维护附近 needsMaintenance=true 的 Mote，避免特效提前消失。
        private void MaintainMotesAtPosition(IntVec3 position, Map map)
        {
            if (map == null || !position.IsValid)
            {
                return;
            }

            // 搜索 2 格半径内的所有 Mote
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, 2, true))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                foreach (Thing thing in map.thingGrid.ThingsAt(cell))
                {
                    // 只维护设置了 needsMaintenance 的 Mote
                    if (thing is Mote mote && mote.def.mote.needsMaintenance)
                    {
                        mote.Maintain();
                    }
                }
            }
        }

        //活动任务不参与存档，读档后不会恢复进行中的特效和延迟生成。
        public override void ExposeData()
        {
            base.ExposeData();
            // TODO: 如需存档持久化，在此添加 activeTasks 的序列化
            // Scribe_Collections.Look(ref activeTasks, "activeTasks", LookMode.Deep);
        }
    }
}
