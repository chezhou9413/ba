using BANWlLib.BaDef;
using BANWlLib.Drop;
using BANWlLib.mainUI.Mission.MonoComp;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.mainUI.StudentManual.MonoComp;
using BANWlLib.Tool;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace newpro
{
    /// <summary>
    /// Pawn 空投工具类
    /// 
    /// 职责：
    /// 1. 管理角色的生成和空投（带特效）
    /// 2. 处理重复角色的逻辑（转为道具）
    /// 3. 生成敌人并分配 AI 行为
    /// 4. 维护待空投的 Pawn 队列
    /// 
    /// 依赖：
    /// - SpawnPawnWithEffectUtility：实际的特效生成逻辑
    /// - ManualDataGameComp：角色收集数据追踪
    /// - MapComponent_EveryFrame：地图级别的空投位置配置
    /// </summary>
    public static class PawnDropHelper
    {
        /// <summary>
        /// 空投配置定义（从 Def 数据库加载）
        /// 包含特效定义、延迟时间等配置
        /// </summary>
        public static BaDrop baDrop;

        /// <summary>
        /// 待空投的 Pawn 队列
        /// 
        /// 工作流程：
        /// 1. DropPawnsByDefName() 生成 Pawn 并加入此队列
        /// 2. dropawnAct() 每帧检查并执行实际空投
        /// 3. 空投完成后从队列移除
        /// </summary>
        public static List<Pawn> droppedPawns = new List<Pawn>();

        public static bool HasPendingPawnForDefName(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return false;
            }

            return droppedPawns.Any(p =>
                p != null &&
                !p.DestroyedOrNull() &&
                p.def != null &&
                p.def.defName == defName);
        }

        /// <summary>
        /// 批量空投角色（按 defName 列表）
        /// 
        /// 核心逻辑：
        /// - 首次获得的角色：记录到收集系统
        /// - 重复获得的角色：转换为对应的升级道具
        /// 
        /// 使用场景：抽卡结果批量生成
        /// </summary>
        /// <param name="defNames">要生成的角色 defName 列表</param>
        public static void DropPawnsByDefNames(List<string> defNames)
        {
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            Map map = Find.CurrentMap;

            foreach (string studentname in defNames)
            {
                // === 判断是否已拥有该角色 ===
                if (!StudentRosterUtility.IsStudentDef(tracker, studentname))
                {
                    // 首次获得：添加到收集记录
                    tracker.HaveStudent.Add(new StudentData(studentname));
                }
                else
                {
                    // 重复获得：生成升级道具代替

                    // 检查该角色是否已经在地图上（此变量目前未使用，可能是预留逻辑）
                    Pawn existingPawn = map.mapPawns.AllPawns.FirstOrDefault(p =>
                        p.def.defName == studentname && p.Spawned && p.Map == map);

                    // 尝试查找对应的升级道具（命名规则：角色defName + "_Proplevel"）
                    string propDefName = studentname + "_Proplevel";
                    ThingDef propDef = DefDatabase<ThingDef>.GetNamedSilentFail(propDefName);

                    if (propDef != null)
                    {
                        // 找到专属道具，空投它
                        DropProp(map, propDef, 1);
                    }
                    else
                    {
                        // 找不到专属道具，使用通用道具 "Kami_Proplevel" 作为兜底
                        ThingDef dropDef = DefDatabase<ThingDef>.GetNamedSilentFail("Kami_Proplevel");
                        DropProp(map, dropDef, 1);
                    }
                }
            }
        }

        /// <summary>
        /// 生成敌人 Pawn 并分配 AI
        /// 
        /// 用途：任务系统中的敌人刷新点
        /// 
        /// 流程：
        /// 1. 确定敌人派系（优先使用配置的派系，否则随机敌对派系）
        /// 2. 根据 KindSettings 批量生成 Pawn
        /// 3. 通过 SpawnPawnWithEffectUtility 播放特效并延迟生成
        /// 4. 为所有生成的 Pawn 分配统一的 AI 行为（Lord）
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="enemySpawnPoint">敌人刷新点配置（包含位置、种类、AI 类型等）</param>
        /// <returns>生成的所有敌人 Pawn 列表</returns>
        public static List<Pawn> SpawEnemyForkind(Map map, EnemySpawnPoint enemySpawnPoint)
        {
            List<Pawn> pawns = new List<Pawn>();
            if (map == null || enemySpawnPoint == null || enemySpawnPoint.KindSettings == null || enemySpawnPoint.KindSettings.Count == 0)
            {
                return pawns;
            }

            if (baDrop == null)
            {
                baDrop = DefDatabase<BaDrop>.AllDefs.FirstOrDefault();
            }

            // === 派系处理（关键：LordJob 必须有派系）===
            Faction targetFaction = null;

            // 优先使用配置的派系
            if (enemySpawnPoint.factionDef != null)
            {
                targetFaction = Find.FactionManager.FirstFactionOfDef(enemySpawnPoint.factionDef);
            }

            // 兜底方案：随机敌对派系 → 古代敌人
            if (targetFaction == null)
            {
                targetFaction = Find.FactionManager.RandomEnemyFaction(false, false, true);
                if (targetFaction == null)
                {
                    targetFaction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
                }
            }

            IntVec3 safeLoc = enemySpawnPoint.SpawnPosition;

            // === 批量生成 Pawn ===
            foreach (KindSetting kindSetting in enemySpawnPoint.KindSettings)
            {
                if (kindSetting == null || kindSetting.pawnKindDef == null || kindSetting.count <= 0)
                {
                    continue;
                }

                for (int i = 0; i < kindSetting.count; i++)
                {
                    // 生成 Pawn（此时只在内存中，未放入地图）
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        kindSetting.pawnKindDef,
                        targetFaction,
                        PawnGenerationContext.NonPlayer,
                        tile: -1,
                        forceGenerateNewPawn: true
                    );
                    Pawn p = PawnGenerator.GeneratePawn(request);
                    IntVec3 spawnPosition = SpawnPawnWithEffectManager.FindSpawnPosition(enemySpawnPoint.SpawnPosition, map, 4);
                    if (!spawnPosition.IsValid)
                    {
                        spawnPosition = enemySpawnPoint.SpawnPosition;
                    }
                    GenSpawn.Spawn(p, spawnPosition, map, Rot4.Random);

                    pawns.Add(p);
                }
            }

            // === 分配 AI 行为 ===
            if (pawns.Count > 0)
            {
                SetEnemyAi(map, pawns, enemySpawnPoint, targetFaction, safeLoc);
            }

            return pawns;
        }

        /// <summary>
        /// 为敌人 Pawn 分配 AI 行为（Lord）
        /// 
        /// 支持的 AI 类型：
        /// - Hunt（进攻）：使用 LordJob_AssaultColony，主动攻击殖民地
        /// - Defend（防守）：使用 LordJob_DefendPoint，守卫指定位置
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="pawns">要分配 AI 的 Pawn 列表</param>
        /// <param name="enemySpawnPoint">刷新点配置</param>
        /// <param name="targetFaction">敌人派系</param>
        /// <param name="intVec">防守位置（Defend 模式使用）</param>
        public static void SetEnemyAi(Map map, List<Pawn> pawns, EnemySpawnPoint enemySpawnPoint, Faction targetFaction, IntVec3 intVec)
        {
            LordJob lordJob = null;

            if (enemySpawnPoint.AiType == AiType.Hunt)
            {
                // 进攻模式：主动攻击殖民地
                lordJob = new LordJob_AssaultColony(
                    targetFaction,
                    canKidnap: false,
                    canTimeoutOrFlee: !enemySpawnPoint.CannotFlee,  // CannotFlee=true 时不会逃跑
                    sappers: false,
                    useAvoidGridSmart: false,
                    canSteal: false
                );
            }
            else if (enemySpawnPoint.AiType == AiType.Defend)
            {
                // 防守模式：守卫指定位置
                lordJob = new LordJob_DefendPoint(
                    intVec,
                    enemySpawnPoint.DefendRadius
                );
            }

            // 创建 Lord 并分配给所有 Pawn
            if (lordJob != null)
            {
                LordMaker.MakeNewLord(targetFaction, lordJob, map, pawns);
            }
        }

        /// <summary>
        /// 将已存在的 Pawn 传送到指定位置（带特效）
        /// 
        /// 用途：角色跳跃、传送等功能
        /// 
        /// 注意：会先将 Pawn 从当前位置移除（DeSpawn），再通过特效重新生成
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="pawn">要传送的 Pawn</param>
        /// <param name="intVec3">目标位置</param>
        /// <returns>传送后的 Pawn（同一实例）</returns>
        public static Pawn JumpForPawnOfBaEff(Map map, Pawn pawn, IntVec3 intVec3)
        {
            return JumpForPawnOfBaEff(map, pawn, intVec3, true);
        }

        public static Pawn JumpForPawnOfBaEff(Map map, Pawn pawn, IntVec3 intVec3, bool useEffect)
        {
            // 确保配置已加载
            if (baDrop == null)
            {
                baDrop = DefDatabase<BaDrop>.AllDefs.FirstOrDefault();
            }

            if (pawn == null)
            {
                return pawn;
            }

            // 先从当前位置移除
            if (pawn.Spawned)
            {
                pawn.DeSpawn(DestroyMode.Vanish);
            }

            // 任务地图批量入场时可关闭特效，避免同时创建过多 Effecter 导致卡顿。
            EffecterDef effecterDef = useEffect ? baDrop.effecterDef : null;
            SpawnPawnWithEffectUtility.SpawnPawnWithEffect(
                intVec3,
                map,
                effecterDef,
                new List<Thing> { pawn },
                useEffect ? baDrop.dropTime : 0
            );

            return pawn;
        }

        /// <summary>
        /// 根据种族定义生成新 Pawn 并传送到指定位置（带特效）
        /// 
        /// 用途：从角色图鉴/选择界面召唤角色到地图
        /// 
        /// 与 JumpForPawnOfBaEff 的区别：
        /// - 此方法会生成新的 Pawn（如果需要）
        /// - 会更新 StudentData 和 selectData 的关联
        /// - 会调用 StudentDetailsController 更新角色信息
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="raceDef">角色种族定义</param>
        /// <param name="intVec3">目标位置</param>
        /// <param name="studentData">角色数据（会被更新）</param>
        /// <param name="selectData">选择数据（会被更新）</param>
        /// <returns>生成/传送的 Pawn</returns>
        public static Pawn JumpForRaceOfBaEff(Map map, BaStudentRaceDef raceDef, IntVec3 intVec3, StudentData studentData, selectData selectData)
        {
            return JumpForRaceOfBaEff(map, raceDef, intVec3, studentData, selectData, true);
        }

        public static Pawn JumpForRaceOfBaEff(Map map, BaStudentRaceDef raceDef, IntVec3 intVec3, StudentData studentData, selectData selectData, bool useEffect)
        {
            // 确保配置已加载
            if (baDrop == null)
            {
                baDrop = DefDatabase<BaDrop>.AllDefs.FirstOrDefault();
            }

            // 生成 Pawn（内部会加入 droppedPawns 队列）
            Pawn pawn = PawnDropHelper.DropPawnsByDefName(raceDef.defName);
            if (pawn == null)
            {
                return null;
            }

            // 从队列移除，避免 dropawnAct() 重复处理
            if (droppedPawns.Contains(pawn))
            {
                droppedPawns.Remove(pawn);
            }

            // 先从当前位置移除
            if (pawn.Spawned)
            {
                pawn.DeSpawn(DestroyMode.Vanish);
            }

            // === 更新关联数据 ===
            selectData.Pawn = pawn;
            StudentRosterUtility.BindStudentPawn(studentData, pawn);
            StudentDetailsController.lordStudentPawninfo(pawn, studentData);

            // 任务地图批量入场时可关闭特效，避免同时创建过多 Effecter 导致卡顿。
            EffecterDef effecterDef = useEffect ? baDrop.effecterDef : null;
            SpawnPawnWithEffectUtility.SpawnPawnWithEffect(
                intVec3,
                map,
                effecterDef,
                new List<Thing> { pawn },
                useEffect ? baDrop.dropTime : 0
            );

            return pawn;
        }

        /// <summary>
        /// 处理待空投队列（每帧调用）
        /// 
        /// 由 SpawnPawnWithEffectManager.GameComponentTick() 调用
        /// 
        /// 工作流程：
        /// 1. 遍历 droppedPawns 队列
        /// 2. 跳过已在地图上的 Pawn
        /// 3. 对每个 Pawn 调用 SpawnPawnWithEffectUtility 执行带特效的生成
        /// 4. 生成后从队列移除
        /// </summary>
        public static void dropawnAct()
        {
            // 确保配置已加载
            if (baDrop == null)
            {
                baDrop = DefDatabase<BaDrop>.AllDefs.FirstOrDefault();
            }

            // 队列为空则跳过
            if (droppedPawns.Count == 0)
            {
                return;
            }

            Map map = Find.CurrentMap;
            EffecterDef effecterDef = baDrop.effecterDef;
            MapComponent_EveryFrame comp = map.GetComponent<MapComponent_EveryFrame>();

            // === 倒序遍历，安全移除元素 ===
            // 参考 SpawnPawnWithEffectManager.GameComponentTick() 的实现方式
            for (int i = droppedPawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = droppedPawns[i];

                // --- 已在目标地图上：跳过并移除 ---
                if (pawn.Spawned && pawn.Map == map)
                {
                    droppedPawns.RemoveAt(i);
                    continue;
                }

                // --- 在其他地图上：先移除 ---
                if (pawn.Spawned && pawn.Map != map)
                {
                    pawn.DeSpawn(DestroyMode.Vanish);
                }

                // --- 特效检查 ---
                if (effecterDef == null)
                {
                    Log.Error("[PawnDropHelper] 无法找到特效定义: BANW_Effecter_LevelUp");
                }

                // === 确定空投位置 ===
                IntVec3 dropPosition;
                if (comp.dropCell != IntVec3.Zero)
                {
                    // 使用配置的固定空投点
                    dropPosition = comp.dropCell;
                }
                else
                {
                    // 随机选择空投点
                    dropPosition = DropCellFinder.RandomDropSpot(map);
                }

                // === 执行带特效的生成 ===
                SpawnPawnWithEffectUtility.SpawnPawnWithEffect(
                    dropPosition,
                    map,
                    effecterDef,
                    new List<Thing> { pawn },
                    baDrop.dropTime
                );

                // 从队列移除已处理的 Pawn
                droppedPawns.RemoveAt(i);
            }
        }

        /// <summary>
        /// 根据 defName 生成单个 Pawn 并加入空投队列
        /// 
        /// 生成规则：
        /// - 通过 defName 查找对应的 PawnKindDef
        /// - 生成为玩家派系的 Pawn
        /// - 生成后加入 droppedPawns 队列，等待 dropawnAct() 处理
        /// 
        /// 注意：此方法只生成 Pawn 到内存，不会立即放入地图
        /// </summary>
        /// <param name="defName">角色种族的 defName</param>
        /// <returns>生成的 Pawn，失败返回 null</returns>
        public static Pawn DropPawnsByDefName(string defName)
        {
            try
            {
                if (string.IsNullOrEmpty(defName))
                {
                    return null;
                }

                if (HasPendingPawnForDefName(defName))
                {
                    return null;
                }

                StudentData existingStudent = StudentRosterUtility.GetStudentData(defName);
                if (existingStudent != null &&
                    existingStudent.StudentPawn != null &&
                    !existingStudent.StudentPawn.DestroyedOrNull())
                {
                    return existingStudent.StudentPawn;
                }

                Map map = Find.CurrentMap;

                // === 查找对应的 PawnKindDef ===
                // 通过 race.defName 匹配，因为 PawnKindDef 和 ThingDef 是不同的定义
                PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefs
                    .FirstOrDefault(k => k.race != null && k.race.defName == defName);

                if (kindDef == null)
                {
                    Log.Error($"无法找到与种族 {defName} 匹配的 PawnKindDef");
                    return null;
                }

                // === 配置生成请求 ===
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: kindDef,
                    faction: Faction.OfPlayer,                    // 玩家派系
                    context: PawnGenerationContext.NonPlayer,     // 非玩家生成上下文
                    forceGenerateNewPawn: true,                   // 强制生成新 Pawn
                    allowDead: false,                             // 不允许死亡
                    allowDowned: false,                           // 不允许倒地
                    canGeneratePawnRelations: true,               // 允许生成社交关系
                    mustBeCapableOfViolence: false,               // 不强制要求能战斗
                    colonistRelationChanceFactor: 1f,             // 殖民者关系概率
                    forceAddFreeWarmLayerIfNeeded: false,         // 不强制添加保暖衣物
                    allowGay: true,                               // 允许同性取向
                    allowFood: true,                              // 允许携带食物
                    allowAddictions: true,                        // 允许成瘾
                    inhabitant: false,                            // 非原住民
                    certainlyBeenInCryptosleep: false             // 非冬眠舱唤醒
                );

                // === 执行生成 ===
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (pawn == null)
                {
                    Log.Error($"无法生成角色: {defName}");
                    return null;
                }

                // 加入空投队列，等待 dropawnAct() 处理
                droppedPawns.Add(pawn);
                return pawn;
            }
            catch (System.Exception ex)
            {
                Log.Error($"空投角色时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 空投物品（使用原版 DropPod 效果）
        /// 
        /// 用途：抽到重复角色时生成升级道具
        /// 
        /// 与角色空投的区别：
        /// - 使用原版 DropPodUtility，有运输舱坠落效果
        /// - 不使用自定义特效系统
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="propDef">物品定义</param>
        /// <param name="amount">堆叠数量</param>
        /// <returns>生成的 Thing，失败返回 null</returns>
        public static Thing DropProp(Map map, ThingDef propDef, int amount)
        {
            try
            {
                if (map == null || propDef == null || amount <= 0)
                {
                    return null;
                }

                // === 创建物品并设置数量 ===
                Thing prop = ThingMaker.MakeThing(propDef);
                prop.stackCount = amount;

                // === 确定空投位置 ===
                MapComponent_EveryFrame comp = map.GetComponent<MapComponent_EveryFrame>();
                IntVec3 dropPosition;

                if (comp.dropCell != IntVec3.Zero)
                {
                    // 使用配置的固定空投点
                    dropPosition = comp.dropCell;
                }
                else
                {
                    // 随机选择空投点
                    dropPosition = DropCellFinder.RandomDropSpot(map);
                }

                // === 执行空投（原版运输舱效果）===
                // 参数说明：
                // - 110：运输舱下落时间（Tick）
                // - canInstaDropDuringInit: true - 初始化时可立即掉落
                // - leaveSlag: true - 留下残骸
                // - canRoofPunch: true - 可以砸穿屋顶
                DropPodUtility.DropThingsNear(
                    dropPosition,
                    map,
                    new List<Thing> { prop },
                    110,
                    canInstaDropDuringInit: true,
                    leaveSlag: true,
                    canRoofPunch: true
                );

                return prop;
            }
            catch (System.Exception ex)
            {
                Log.Error($"空投物品时发生错误: {ex.Message}");
                return null;  // 注意：原代码中 return 在 Log.Error 之前，这里已修正
            }
        }


        /// <summary>
        /// 空投单个 Thing（使用原版 DropPod 效果）
        /// 
        /// 纯净版本：直接传入已创建的 Thing 对象
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="thing">要空投的物品</param>
        /// <param name="position">空投位置（可选，为 null 时使用默认位置）</param>
        /// <returns>是否成功</returns>
        public static bool DropThing(Map map, Thing thing, IntVec3? position = null)
        {
            if (map == null || thing == null)
            {
                return false;
            }

            try
            {
                IntVec3 dropPosition = GetDropPosition(map, position);

                DropPodUtility.DropThingsNear(
                    dropPosition,
                    map,
                    new List<Thing> { thing },
                    110,
                    canInstaDropDuringInit: true,
                    leaveSlag: true,
                    canRoofPunch: true
                );

                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[PawnDropHelper] 空投物品时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 批量空投 Thing（使用原版 DropPod 效果）
        /// 
        /// 纯净版本：直接传入已创建的 Thing 列表
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="things">要空投的物品列表</param>
        /// <param name="position">空投位置（可选，为 null 时使用默认位置）</param>
        /// <param name="groupInOnePod">是否放在同一个运输舱（默认 true）</param>
        /// <returns>是否成功</returns>
        public static bool DropThings(Map map, List<Thing> things, IntVec3? position = null, bool groupInOnePod = true)
        {
            if (map == null || things == null || things.Count == 0)
            {
                return false;
            }

            try
            {
                IntVec3 dropPosition = GetDropPosition(map, position);

                if (groupInOnePod)
                {
                    // 所有物品放在同一个运输舱
                    DropPodUtility.DropThingsNear(
                        dropPosition,
                        map,
                        things,
                        110,
                        canInstaDropDuringInit: true,
                        leaveSlag: true,
                        canRoofPunch: true
                    );
                }
                else
                {
                    // 每个物品单独一个运输舱
                    foreach (Thing thing in things)
                    {
                        DropPodUtility.DropThingsNear(
                            dropPosition,
                            map,
                            new List<Thing> { thing },
                            110,
                            canInstaDropDuringInit: true,
                            leaveSlag: true,
                            canRoofPunch: true
                        );
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[PawnDropHelper] 批量空投物品时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 根据 ThingDef 和数量创建并空投 Thing
        /// 
        /// 便捷方法：自动创建 Thing 并空投
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="def">物品定义</param>
        /// <param name="count">数量</param>
        /// <param name="position">空投位置（可选）</param>
        /// <param name="stuff">材质（可选，用于需要材质的物品如衣物）</param>
        /// <returns>创建的 Thing，失败返回 null</returns>
        public static Thing DropThingByDef(Map map, ThingDef def, int count = 1, IntVec3? position = null, ThingDef stuff = null)
        {
            if (map == null || def == null || count <= 0)
            {
                return null;
            }

            try
            {
                Thing thing = ThingMaker.MakeThing(def, stuff);
                thing.stackCount = count;

                if (DropThing(map, thing, position))
                {
                    return thing;
                }

                return null;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[PawnDropHelper] 创建并空投物品时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据字典批量创建并空投 Thing
        /// 
        /// 便捷方法：适用于 Dictionary&lt;ThingDef, int&gt; 格式的数据
        /// </summary>
        /// <param name="map">目标地图</param>
        /// <param name="thingData">物品数据（Key: ThingDef, Value: 数量）</param>
        /// <param name="position">空投位置（可选）</param>
        /// <param name="groupInOnePod">是否放在同一个运输舱</param>
        /// <returns>创建的 Thing 列表</returns>
        public static List<Thing> DropThingsByDict(Map map, Dictionary<ThingDef, int> thingData, IntVec3? position = null, bool groupInOnePod = true)
        {
            List<Thing> createdThings = new List<Thing>();

            if (map == null || thingData == null || thingData.Count == 0)
            {
                return createdThings;
            }

            try
            {
                foreach (KeyValuePair<ThingDef, int> kvp in thingData)
                {
                    if (kvp.Key == null || kvp.Value <= 0)
                    {
                        continue;
                    }

                    Thing thing = ThingMaker.MakeThing(kvp.Key);
                    thing.stackCount = kvp.Value;
                    createdThings.Add(thing);
                }

                if (createdThings.Count > 0)
                {
                    DropThings(map, createdThings, position, groupInOnePod);
                }

                return createdThings;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[PawnDropHelper] 批量创建并空投物品时发生错误: {ex.Message}");
                return createdThings;
            }
        }

        /// <summary>
        /// 获取空投位置
        /// 
        /// 优先级：
        /// 1. 传入的指定位置
        /// 2. MapComponent_EveryFrame 配置的 dropCell
        /// 3. 随机空投点
        /// </summary>
        private static IntVec3 GetDropPosition(Map map, IntVec3? position)
        {
            // 优先使用指定位置
            if (position.HasValue && position.Value.IsValid && position.Value != IntVec3.Zero)
            {
                return position.Value;
            }

            // 尝试使用配置的空投点
            MapComponent_EveryFrame comp = map.GetComponent<MapComponent_EveryFrame>();
            if (comp != null && comp.dropCell != IntVec3.Zero)
            {
                return comp.dropCell;
            }

            // 兜底：随机空投点
            return DropCellFinder.RandomDropSpot(map);
        }
    }
}
