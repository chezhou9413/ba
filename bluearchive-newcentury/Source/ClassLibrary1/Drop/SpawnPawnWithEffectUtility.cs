using newpro;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.Drop
{
    /// <summary>
    /// 工具类：延迟生成 Pawn/Thing 并播放特效
    /// 
    /// 使用场景：
    /// - 抽卡系统生成角色时的特效演出
    /// - 空投、传送等需要视觉效果的生成场景
    /// 
    /// 工作流程：
    /// 1. 调用 SpawnPawnWithEffect() 注册生成任务
    /// 2. 立即播放特效（Effecter）
    /// 3. 等待指定时间后生成实际的 Thing/Pawn
    /// 4. 特效播放完毕后自动清理
    /// </summary>
    public static class SpawnPawnWithEffectUtility
    {
        /// <summary>
        /// 在指定位置播放特效，延迟后生成 Thing/Pawn
        /// </summary>
        /// <param name="position">目标位置（会自动寻找附近可站立的格子）</param>
        /// <param name="map">目标地图</param>
        /// <param name="effecterDef">特效定义（可为 null，则直接生成无特效）</param>
        /// <param name="things">要生成的 Thing 列表（可以是 Pawn 或普通物品）</param>
        /// <param name="time">延迟时间（单位：Tick，60 Tick = 1 秒）</param>
        /// <param name="faction">生成的 Pawn 所属派系（可选）</param>
        /// <param name="spawnRadius">搜索可站立位置的半径（默认 3 格）</param>
        public static void SpawnPawnWithEffect(
            IntVec3 position,
            Map map,
            EffecterDef effecterDef,
            List<Thing> things,
            int time,
            Faction faction = null,
            int spawnRadius = 3)
        {
            // === 参数校验 ===
            if (map == null)
            {
                return;
            }

            // === 无特效的情况：直接生成 ===
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

            // === 无生成物的情况：跳过 ===
            if (things == null || things.Count == 0)
            {
                return;
            }

            // === 创建延迟生成任务 ===
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

            // === 获取/创建 GameComponent 管理器 ===
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

            // === 计算实际生成位置（避免多个任务位置重叠）===
            IntVec3 actualSpawnPosition = manager.FindSpawnPositionWithOffset(position, map, spawnRadius);
            task.position = actualSpawnPosition;

            // === 注册任务 ===
            manager.AddTask(task);

            // === 立即开始播放特效 ===
            // 参考原版 LevelVisualComp 的实现：手动维护特效，每帧调用 EffectTick
            try
            {
                // Spawn() 方法会自动创建特效实例及其所有子特效
                task.effecter = effecterDef.Spawn(actualSpawnPosition, map);

                if (task.effecter != null)
                {
                    // 设置特效播放时长（300 Tick ≈ 5 秒）
                    // 确保所有延迟子特效都能播放完成
                    task.effecterTimer = 300;

                    // 立即调用一次 EffectTick 确保特效立即显示
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

    /// <summary>
    /// 延迟生成任务的数据结构
    /// 
    /// 存储单个生成任务的所有必要信息，由 SpawnPawnWithEffectManager 统一管理
    /// </summary>
    public class DelayedSpawnTask
    {
        /// <summary>实际生成位置（已经过偏移计算）</summary>
        public IntVec3 position;

        /// <summary>目标地图</summary>
        public Map map;

        /// <summary>特效定义</summary>
        public EffecterDef effecterDef;

        /// <summary>要生成的 Thing 列表</summary>
        public List<Thing> things;

        /// <summary>Pawn 所属派系</summary>
        public Faction faction;

        /// <summary>搜索可站立位置的半径</summary>
        public int spawnRadius;

        /// <summary>任务创建时的游戏 Tick</summary>
        public int startTick;

        /// <summary>当前播放的特效实例</summary>
        public Effecter effecter;

        /// <summary>任务是否已完成（Thing 已生成）</summary>
        public bool completed = false;

        /// <summary>
        /// 特效剩余播放时间（单位：Tick）
        /// 
        /// 参考原版 LevelVisualComp 的实现，使用独立计时器而非 effecter.ticksLeft
        /// 这样可以更精确地控制特效播放时长
        /// </summary>
        public int effecterTimer = 0;

        /// <summary>延迟生成时间（单位：Tick，60 Tick = 1 秒）</summary>
        public int time;
    }

    /// <summary>
    /// GameComponent：统一管理所有延迟生成任务
    /// 
    /// 职责：
    /// 1. 维护活动任务列表
    /// 2. 每帧更新特效播放状态
    /// 3. 在指定延迟后执行实际生成
    /// 4. 清理已完成的任务和特效
    /// 
    /// 生命周期：
    /// - 随游戏存档创建/加载
    /// - 通过 GameComponentTick() 每帧更新
    /// </summary>
    public class SpawnPawnWithEffectManager : GameComponent
    {
        /// <summary>当前活动的延迟生成任务列表</summary>
        private List<DelayedSpawnTask> activeTasks = new List<DelayedSpawnTask>();

        /// <summary>构造函数（GameComponent 必须）</summary>
        public SpawnPawnWithEffectManager(Game game)
        {
        }

        /// <summary>添加新的延迟生成任务</summary>
        public void AddTask(DelayedSpawnTask task)
        {
            if (task != null && !activeTasks.Contains(task))
            {
                activeTasks.Add(task);
            }
        }

        /// <summary>获取当前活动任务数量（调试用）</summary>
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

        /// <summary>
        /// 每帧更新
        /// 
        /// 处理流程：
        /// 1. 执行全局的 PawnDropHelper.dropawnAct()（UI 相关）
        /// 2. 遍历所有活动任务
        /// 3. 维护特效播放（调用 EffectTick）
        /// 4. 检查是否到达生成时间，执行生成
        /// 5. 清理已完成的任务
        /// </summary>
        public override void GameComponentTick()
        {
            base.GameComponentTick();

            // === UI 相关逻辑（与延迟生成无关）===
            if (!UiMapData.uiclose)
            {
                PawnDropHelper.dropawnAct();
            }

            // === 倒序遍历，方便安全移除元素 ===
            for (int i = activeTasks.Count - 1; i >= 0; i--)
            {
                DelayedSpawnTask task = activeTasks[i];

                // --- 地图失效检查 ---
                if (task == null || task.map == null || task.map.Disposed)
                {
                    activeTasks.RemoveAt(i);
                    task.effecter?.Cleanup();
                    continue;
                }

                // --- 维护特效播放 ---
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

                // --- 检查是否到达生成时间 ---
                bool shouldSpawnPawn = Find.TickManager.TicksGame >= task.startTick + task.time;

                // --- 执行生成（仅一次）---
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

                // --- 清理已完成的任务 ---
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

        /// <summary>
        /// 执行实际的 Thing/Pawn 生成
        /// </summary>
        /// <param name="task">要执行的任务</param>
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
                // === Pawn 特殊处理：防止重复生成 ===
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

                // === 执行生成 ===
                GenSpawn.Spawn(thing, spawnPosition, task.map, Rot4.Random);
            }
        }

        /// <summary>
        /// 查找可站立的生成位置（静态方法，供外部调用）
        /// 
        /// 算法：
        /// 1. 优先尝试中心位置
        /// 2. 从内到外搜索半径内的可站立格子
        /// 3. 排除已有 Pawn 的格子
        /// </summary>
        /// <param name="center">搜索中心</param>
        /// <param name="map">目标地图</param>
        /// <param name="radius">搜索半径</param>
        /// <returns>可用的生成位置，找不到则返回中心位置</returns>
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

        /// <summary>
        /// 查找生成位置（带冲突检测）
        /// 
        /// 与 FindSpawnPosition 的区别：
        /// 会检查是否与其他活动任务的生成位置重叠，
        /// 如果重叠则尝试偏移到附近的空位
        /// 
        /// 用途：多个 Thing 同时生成时避免特效和实体重叠
        /// </summary>
        public IntVec3 FindSpawnPositionWithOffset(IntVec3 center, Map map, int radius)
        {
            IntVec3 basePosition = FindSpawnPosition(center, map, radius);

            // === 检查是否与其他任务位置冲突（距离 ≤ 1 格视为冲突）===
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

            // === 有冲突时尝试偏移 ===
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

        /// <summary>
        /// 维护指定位置附近需要持续维护的 Mote
        /// 
        /// 背景：
        /// 部分 Mote（使用 Graphic_MoteWithAgeSecs 且 needsMaintenance=true）
        /// 需要每帧调用 Maintain() 才能保持显示，否则会自动消失
        /// 
        /// 这是原版特效系统的设计，用于实现"只有在某些条件下才显示"的效果
        /// </summary>
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

        /// <summary>
        /// 存档序列化
        /// 
        /// 注意：当前未保存活动任务列表
        /// 这意味着存档/读档时进行中的特效和延迟生成会丢失
        /// 如需持久化，需要实现 activeTasks 的序列化
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            // TODO: 如需存档持久化，在此添加 activeTasks 的序列化
            // Scribe_Collections.Look(ref activeTasks, "activeTasks", LookMode.Deep);
        }
    }
}
