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
    //生成和空投角色，将重复角色转换为道具，并为敌人分配行为。
    public static class PawnDropHelper
    {
        //空投特效和延迟配置。
        public static BaDrop baDrop;

        //由 DropPawnsByDefName 入队，dropawnAct 生成后移除。
        public static List<Pawn> droppedPawns = new List<Pawn>();

        //判断指定学生身份是否已有等待空投的 Pawn。
        public static bool HasPendingPawnForDefName(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return false;
            }

            string studentId = StudentIdentityUtility.GetStudentId(defName);
            return droppedPawns.Any(p =>
                p != null &&
                !p.DestroyedOrNull() &&
                !p.Dead &&
                StudentIdentityUtility.GetStudentId(p) == studentId);
        }

        //按 defName 批量空投学生；首次获得时加入名册，重复获得时转换为升级道具。
        public static void DropPawnsByDefNames(List<string> defNames)
        {
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();
            Map map = Find.CurrentMap;

            foreach (string studentname in defNames)
            {
                //检查是否已拥有该学生。
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
                        StudentIdentityUtility.GetStudentId(p) == studentname && p.Spawned && p.Map == map);

                    // 尝试查找对应的升级道具（命名规则：当前 studentId + "_Proplevel"）
                    string propDefName = studentname + "_Proplevel";
                    ThingDef propDef = DefDatabase<ThingDef>.GetNamedSilentFail(propDefName);

                    if (propDef != null)
                    {
                        // 找到专属道具，空投它
                        DropProp(map, propDef, 1);
                    }
                    else
                    {
                        //缺少专属道具时使用 Kami_Proplevel。
                        ThingDef dropDef = DefDatabase<ThingDef>.GetNamedSilentFail("Kami_Proplevel");
                        DropProp(map, dropDef, 1);
                    }
                }
            }
        }

        //按刷新点配置生成敌人，播放空投特效并分配 Lord 行为，返回生成列表。
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

            //LordJob 必须有所属派系。
            Faction targetFaction = null;

            // 优先使用配置的派系
            if (enemySpawnPoint.factionDef != null)
            {
                targetFaction = Find.FactionManager.FirstFactionOfDef(enemySpawnPoint.factionDef);
            }

            //未指定派系时依次尝试随机敌对派系和古代敌人。
            if (targetFaction == null)
            {
                targetFaction = Find.FactionManager.RandomEnemyFaction(false, false, true);
                if (targetFaction == null)
                {
                    targetFaction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
                }
            }

            IntVec3 safeLoc = enemySpawnPoint.SpawnPosition;

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

            //分配敌人行为。
            if (pawns.Count > 0)
            {
                SetEnemyAi(map, pawns, enemySpawnPoint, targetFaction, safeLoc);
            }

            return pawns;
        }

        //为敌人分配行为：Hunt 进攻殖民地，Defend 防守 intVec 指定的位置。
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

        //先 DeSpawn，再带特效传送到指定位置，保留原 Pawn 实例。
        public static Pawn JumpForPawnOfBaEff(Map map, Pawn pawn, IntVec3 intVec3)
        {
            return JumpForPawnOfBaEff(map, pawn, intVec3, true);
        }

        public static Pawn JumpForPawnOfBaEff(Map map, Pawn pawn, IntVec3 intVec3, bool useEffect)
        {
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

        //根据当前学生身份生成新 Pawn 并传送到指定位置，让任务和图鉴生成统一走 PawnKind。
        public static Pawn JumpForStudentOfBaEff(Map map, string studentId, IntVec3 intVec3, StudentData studentData, selectData selectData, bool useEffect)
        {
            if (baDrop == null)
            {
                baDrop = DefDatabase<BaDrop>.AllDefs.FirstOrDefault();
            }

            // 生成 Pawn（内部会加入 droppedPawns 队列）
            Pawn pawn = PawnDropHelper.DropPawnsByDefName(studentId);
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

            //更新关联数据。
            if (selectData != null)
            {
                selectData.Pawn = pawn;
            }
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

        //由 GameComponentTick 推进空投队列，跳过已生成角色并移除已处理项。
        public static void dropawnAct()
        {
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

            //倒序遍历，便于移除已处理项。
            // 参考 SpawnPawnWithEffectManager.GameComponentTick() 的实现方式
            for (int i = droppedPawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = droppedPawns[i];

                //已在目标地图的 Pawn 直接移出队列。
                if (pawn.Spawned && pawn.Map == map)
                {
                    droppedPawns.RemoveAt(i);
                    continue;
                }

                //先从其他地图移除 Pawn。
                if (pawn.Spawned && pawn.Map != map)
                {
                    pawn.DeSpawn(DestroyMode.Vanish);
                }

                if (effecterDef == null)
                {
                    Log.Error("[PawnDropHelper] 无法找到特效定义: BANW_Effecter_LevelUp");
                }

                //选择空投位置。
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

        //按 PawnKindDef 名称创建玩家 Pawn 并入队，不立即生成到地图；失败返回 null。
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
                    !existingStudent.StudentPawn.DestroyedOrNull() &&
                    !existingStudent.StudentPawn.Dead)
                {
                    return existingStudent.StudentPawn;
                }

                Map map = Find.CurrentMap;

                // 通过当前学生身份工具解析 PawnKind。
                PawnKindDef kindDef;
                StudentIdentityUtility.TryGetPawnKindDef(defName, out kindDef);

                if (kindDef == null)
                {
                    Log.Error($"无法找到与学生身份 {defName} 匹配的 PawnKindDef");
                    return null;
                }

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

        //创建升级道具并通过原版运输舱空投，失败返回 null。
        public static Thing DropProp(Map map, ThingDef propDef, int amount)
        {
            try
            {
                if (map == null || propDef == null || amount <= 0)
                {
                    return null;
                }

                Thing prop = ThingMaker.MakeThing(propDef);
                prop.stackCount = amount;

                //选择空投位置。
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

                //使用原版运输舱空投。
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
                return null;
            }
        }


        //空投已有 Thing，position 为空时使用默认位置，返回是否成功。
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

        //批量空投已有 Thing；groupInOnePod 控制是否合舱，position 为空时使用默认位置。
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

        //按定义、数量和可选材质创建物品并空投，失败返回 null。
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

        //按 ThingDef 与数量字典创建并空投物品，groupInOnePod 控制是否合舱。
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

        //依次使用指定位置、地图 dropCell 和随机空投点。
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

            //使用随机空投点。
            return DropCellFinder.RandomDropSpot(map);
        }
    }
}
