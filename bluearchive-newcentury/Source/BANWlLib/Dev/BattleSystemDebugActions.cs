using System;
using System.Collections.Generic;
using BANWlLib.BattleSystem;
using BANWlLib.Tool;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Dev
{
    // 战斗系统测试入口，负责在开发者模式下快速生成可复现的战斗场景。
    public static class BattleSystemDebugActions
    {
        private const string Category = "BA测试/战斗系统";

        // 生成完整测试场景，负责一次性摆放常用测试对象。
        [DebugAction(Category, "生成完整战斗测试场景", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnFullBattleTestScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            Pawn nozomi = SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center + new IntVec3(-8, 0, 0), map, "Nozomi 测试施法者");
            Pawn serina = SpawnStudent("BANW_Serina", Faction.OfPlayer, center + new IntVec3(-8, 0, 2), map, "Serina 治疗测试者");
            Pawn heavyTarget = SpawnStudent("BANW_Chise", GetHostileFaction(), center + new IntVec3(8, 0, 0), map, "重装目标");
            Pawn lightTarget = SpawnStudent("BANW_Serina", GetHostileFaction(), center + new IntVec3(8, 0, 2), map, "轻装目标");
            Pawn healTarget = SpawnStudent("BANW_Hikali", Faction.OfPlayer, center + new IntVec3(-4, 0, 3), map, "受伤友方");

            BuildWallLine(center + new IntVec3(0, 0, -2), map, 5);
            InjurePawn(healTarget, 40f);
            SetStar(nozomi, 5);
            SetStar(serina, 5);

            Log.Message($"[BANW测试] 已生成完整场景。Nozomi={nozomi?.LabelShort}, Serina={serina?.LabelShort}, 重装={heavyTarget?.LabelShort}, 轻装={lightTarget?.LabelShort}, 受伤友方={healTarget?.LabelShort}");
            Messages.Message("已生成完整战斗测试场景：左侧玩家，右侧敌人，中间墙体。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成直线穿墙测试，负责摆放施法者、墙体和墙后目标。
        [DebugAction(Category, "场景：直线穿墙弹", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnPiercingProjectileScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center + new IntVec3(-8, 0, 0), map, "直线弹施法者");
            BuildWallLine(center + new IntVec3(0, 0, -1), map, 3);
            SpawnStudent("BANW_Chise", GetHostileFaction(), center + new IntVec3(5, 0, 0), map, "墙后重装目标");
            SpawnStudent("BANW_Serina", GetHostileFaction(), center + new IntVec3(7, 0, 1), map, "墙后轻装目标");

            Messages.Message("直线穿墙弹场景已生成：用 Nozomi 的 BAWN_Nozomi_Train_Test 朝右侧发射。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成脱手场地测试，负责摆放施法者和密集敌人。
        [DebugAction(Category, "场景：脱手场地", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnBattleFieldScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center + new IntVec3(-6, 0, 0), map, "场地施法者");
            SpawnStudent("BANW_Chise", GetHostileFaction(), center + new IntVec3(1, 0, 0), map, "场地目标 A");
            SpawnStudent("BANW_Marina", GetHostileFaction(), center + new IntVec3(2, 0, 1), map, "场地目标 B");
            SpawnStudent("BANW_Serina", GetHostileFaction(), center + new IntVec3(1, 0, -1), map, "场地目标 C");

            Messages.Message("脱手场地场景已生成：用 Nozomi 的 BAWN_Nozomi_Field_Test 点敌人中心。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成叠层 Buff 测试，负责生成 Nozomi 并提示技能连放验证层数。
        [DebugAction(Category, "场景：叠层攻击Buff", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnStackBuffScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            Pawn nozomi = SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center, map, "叠层测试 Nozomi");
            SpawnStudent("BANW_Chise", GetHostileFaction(), center + new IntVec3(6, 0, 0), map, "叠层伤害目标");
            LogBattleStats(nozomi, "叠层测试初始属性");
            Messages.Message("叠层场景已生成：连续使用 BAWN_Nozomi_StackBuff_Test，预期 30%/70%/100%。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 直接施加叠层 Buff，负责不用手动点技能也能验证层数。
        [DebugAction(Category, "执行：给选中Pawn叠1层攻击Buff", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddOneAttackStackToSelectedPawn()
        {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            if (pawn == null)
            {
                Messages.Message("请先选中一个 Pawn。", MessageTypeDefOf.RejectInput, false);
                return;
            }

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("BANW_BattleStack_Attack_Test");
            BattleStackHediffUtility.ApplyStackedHediff(pawn, hediffDef);
            LogBattleStats(pawn, "叠层后属性");
            Messages.Message("已给选中 Pawn 施加 1 层叠层攻击 Buff。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成治疗测试，负责摆放 Serina 和一个受伤友方。
        [DebugAction(Category, "场景：治疗力与受回复", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnHealingScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            Pawn serina = SpawnStudent("BANW_Serina", Faction.OfPlayer, center + new IntVec3(-4, 0, 0), map, "治疗者 Serina");
            Pawn target = SpawnStudent("BANW_Hikali", Faction.OfPlayer, center + new IntVec3(1, 0, 0), map, "受伤目标");
            InjurePawn(target, 60f);
            SetStar(serina, 5);
            LogBattleStats(serina, "治疗者属性");

            Messages.Message("治疗场景已生成：用 Serina 技能治疗受伤目标，或执行受回复测试。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 直接设置受回复测试状态，负责验证目标受疗率倍率。
        [DebugAction(Category, "执行：给选中Pawn造成测试伤口", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void InjureSelectedPawn()
        {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            if (pawn == null)
            {
                Messages.Message("请先选中一个 Pawn。", MessageTypeDefOf.RejectInput, false);
                return;
            }

            InjurePawn(pawn, 50f);
            Messages.Message("已给选中 Pawn 添加测试伤口。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成属性克制测试，负责摆放贯通攻击者、重装目标和轻装目标。
        [DebugAction(Category, "场景：属性克制", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnAffinityScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            Pawn attacker = SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center + new IntVec3(-6, 0, 0), map, "贯通攻击者");
            Pawn heavy = SpawnStudent("BANW_Chise", GetHostileFaction(), center + new IntVec3(4, 0, 0), map, "重装克制目标");
            Pawn light = SpawnStudent("BANW_Serina", GetHostileFaction(), center + new IntVec3(4, 0, 2), map, "轻装中性目标");

            float heavyMultiplier = BattleStatUtility.GetAffinityMultiplier(attacker, heavy);
            float lightMultiplier = BattleStatUtility.GetAffinityMultiplier(attacker, light);
            Log.Message($"[BANW测试] 属性克制倍率：贯通攻击者 -> 重装 {heavyMultiplier}，贯通攻击者 -> 轻装 {lightMultiplier}");
            Messages.Message("属性克制场景已生成：日志已输出重装和轻装倍率。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成阶级成长测试，负责生成同角色并设置不同阶级输出属性。
        [DebugAction(Category, "场景：阶级成长", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnStarGrowthScene()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!PrepareCell(map, center))
            {
                return;
            }

            Pawn rankOne = SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center + new IntVec3(-2, 0, 0), map, "1阶 Nozomi");
            Pawn rankThree = SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center, map, "3阶 Nozomi");
            Pawn rankFive = SpawnStudent("BANW_Nozomi", Faction.OfPlayer, center + new IntVec3(2, 0, 0), map, "5阶 Nozomi");
            SetRank(rankOne, 1);
            SetRank(rankThree, 3);
            SetRank(rankFive, 5);

            LogBattleStats(rankOne, "1阶 Nozomi");
            LogBattleStats(rankThree, "3阶 Nozomi");
            LogBattleStats(rankFive, "5阶 Nozomi");
            Messages.Message("阶级成长场景已生成：日志已输出 1/3/5 阶攻击和治疗数据。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 输出选中 Pawn 的战斗属性，负责快速核对 Buff、星级和治疗力。
        [DebugAction(Category, "查看选中Pawn战斗属性", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogSelectedPawnBattleStats()
        {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            if (pawn == null)
            {
                Messages.Message("请先选中一个 Pawn。", MessageTypeDefOf.RejectInput, false);
                return;
            }

            LogBattleStats(pawn, "选中 Pawn");
        }

        // 切换公式调试日志，负责把每次伤害和治疗的预估、最终值与实际值输出到控制台。
        [DebugAction(Category, "切换公式调试日志", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleBattleFormulaDebugLog()
        {
            bool nextState = !BattleFormulaDebugUtility.IsEnabled();
            BattleFormulaDebugUtility.SetEnabled(nextState);
            Messages.Message("公式调试日志已" + (nextState ? "开启" : "关闭") + "。", MessageTypeDefOf.NeutralEvent, false);
        }

        // 生成学生 Pawn，负责统一学生测试对象的创建和落点。
        private static Pawn SpawnStudent(string pawnKindDefName, Faction faction, IntVec3 cell, Map map, string label)
        {
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKindDefName);
            if (kindDef == null)
            {
                Log.Error($"[BANW测试] 找不到 PawnKindDef: {pawnKindDefName}");
                return null;
            }

            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                kindDef,
                faction,
                PawnGenerationContext.NonPlayer,
                tile: map.Tile,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f,
                forceNoIdeo: true));

            IntVec3 spawnCell = FindSpawnCellNear(cell, map);
            GenSpawn.Spawn(pawn, spawnCell, map, Rot4.South);
            pawn.Name = new NameSingle(label);
            pawn.jobs?.StopAll();
            if (pawn.drafter != null)
            {
                pawn.drafter.Drafted = true;
            }
            return pawn;
        }

        // 生成墙体线，负责直线弹穿墙测试。
        private static void BuildWallLine(IntVec3 start, Map map, int length)
        {
            ThingDef wallDef = ThingDefOf.Wall;
            ThingDef stuffDef = ThingDefOf.Steel;
            for (int i = 0; i < length; i++)
            {
                IntVec3 cell = start + new IntVec3(0, 0, i);
                if (!cell.InBounds(map))
                {
                    continue;
                }

                ClearCell(cell, map);
                Thing wall = ThingMaker.MakeThing(wallDef, stuffDef);
                wall.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(wall, cell, map);
            }
        }

        // 添加测试伤口，负责治疗系统验证。
        private static void InjurePawn(Pawn pawn, float amount)
        {
            if (pawn == null)
            {
                return;
            }

            BodyPartRecord part = pawn.health.hediffSet.GetNotMissingParts().FirstOrFallback();
            if (part == null)
            {
                return;
            }

            Hediff_Injury injury = (Hediff_Injury)HediffMaker.MakeHediff(RimWorld.HediffDefOf.Cut, pawn, part);
            injury.Severity = amount;
            pawn.health.AddHediff(injury, part);
        }

        // 设置稀有度星级，负责只影响学生档案和抽卡语义。
        private static void SetStar(Pawn pawn, int star)
        {
            if (pawn != null)
            {
                StudentRosterUtility.SetCurrentStarLevel(pawn, star);
            }
        }

        // 设置测试阶级，负责让同名 Pawn 在 Debug 场景中拥有独立面板星星和成长属性。
        private static void SetRank(Pawn pawn, int rank)
        {
            if (pawn != null)
            {
                StudentRankUtility.SetRankByExperience(pawn, rank);
                StudentInitializationDebugUtility.MarkDebugInitialized(pawn);
            }
        }

        // 输出战斗属性，负责验证阶级、Buff、治疗力和受疗率是否进入统一层。
        private static void LogBattleStats(Pawn pawn, string title)
        {
            if (pawn == null)
            {
                Log.Warning($"[BANW测试] {title}: Pawn 为空");
                return;
            }

            Log.Message($"[BANW测试] {title}: 阶级={BattleStatUtility.GetCurrentRankLevel(pawn)}, 稀有度={StudentRosterUtility.GetCurrentStarLevel(pawn)}, 攻击倍率={BattleStatUtility.GetAttackMultiplier(pawn):F2}, 最终攻击={BattleStatUtility.GetFinalAttackPower(pawn):F2}, 治愈升级倍率={BattleStatUtility.GetHealLevelMultiplier(pawn):F2}, 治愈升星倍率={BattleStatUtility.GetHealStarMultiplier(pawn):F2}, 治愈加成={BattleStatUtility.GetHealBonusMultiplier(pawn):F2}, 最终治疗={BattleStatUtility.GetFinalHealPower(pawn):F2}, 受疗={BattleStatUtility.GetHealReceivedMultiplier(pawn):F2}");
        }

        // 获取敌对派系，负责生成稳定敌人目标。
        private static Faction GetHostileFaction()
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile);
            return faction ?? Find.FactionManager.RandomEnemyFaction();
        }

        // 准备鼠标位置，负责拒绝无效地图格。
        private static bool PrepareCell(Map map, IntVec3 cell)
        {
            if (map == null || !cell.InBounds(map))
            {
                Messages.Message("鼠标位置不是有效地图格。", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        // 寻找可生成格，负责避免目标格被占用时生成失败。
        private static IntVec3 FindSpawnCellNear(IntVec3 root, Map map)
        {
            IntVec3 result;
            if (CellFinder.TryFindRandomCellNear(root, map, 3, cell => cell.Standable(map) && !cell.Fogged(map), out result))
            {
                return result;
            }

            return root;
        }

        // 清理格子，负责给墙体测试提供确定位置。
        private static void ClearCell(IntVec3 cell, Map map)
        {
            List<Thing> things = new List<Thing>(cell.GetThingList(map));
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing.def.category == ThingCategory.Building && thing.def.destroyable)
                {
                    thing.Destroy();
                }
            }
        }
    }
}
