using BANWlLib.Dev.Menus;
using System.Collections.Generic;
using System.Linq;
using BANWlLib.CostSystem;
using LudeonTK;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能调试菜单，负责生成正式角色及手动检查新技能运行数据。
    public static class SpecialSkillDebugActions
    {
        private const string Category = "特殊技能";
        private static readonly string[] Kinds = { "BANW_Nero", "BANW_Arisu_B", "BANW_Shiroko", "BANW_Wakamo", "BANW_Hoshiro", "BANW_Kei", "BANW_Rio" };

        //在鼠标附近一次生成七名已追加测试技能的正式角色。
        [BADebugAction(Category, "生成全部七名正式角色", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnAll()
        {
            foreach (string kind in Kinds) Spawn(kind);
            FillCost();
        }

        //提供正式角色选择菜单，选择后在地图点击生成。
        [BADebugAction(Category, "选择生成一名正式角色", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static List<DebugActionNode> SpawnOne()
        {
            return Kinds.Select(kind => new DebugActionNode(
                DefDatabase<PawnKindDef>.GetNamed(kind).label, DebugActionType.ToolMap, () => Spawn(kind))).ToList();
        }

        //调用原版生成器创建正式角色和正式装备，不替换旧技能。
        private static void Spawn(string kindName)
        {
            Map map = Find.CurrentMap;
            IntVec3 center = UI.MouseCell();
            if (!center.InBounds(map)) { Log.Error("[BANW] 鼠标不在有效地图格"); return; }
            IntVec3 cell = GenRadial.RadialCellsAround(center, 8f, true)
                .Where(c => c.InBounds(map) && c.Standable(map) && c.GetFirstPawn(map) == null).DefaultIfEmpty(IntVec3.Invalid).First();
            if (!cell.IsValid) { Log.Error("[BANW] 附近没有可生成角色的空地"); return; }
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                DefDatabase<PawnKindDef>.GetNamed(kindName), Faction.OfPlayer, PawnGenerationContext.NonPlayer,
                tile: map.Tile, forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: false, mustBeCapableOfViolence: false, colonistRelationChanceFactor: 0f, forceNoIdeo: true));
            GenSpawn.Spawn(pawn, cell, map);
            pawn.jobs?.StopAll();
            if (pawn.drafter != null) pawn.drafter.Drafted = true;
            Messages.Message("已生成正式角色：" + pawn.LabelShort + "，测试新技能已追加。", pawn, MessageTypeDefOf.NeutralEvent, false);
        }

        //补满当前地图的共享COST。
        [BADebugAction(Category, "补满共享COST", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillCost()
        {
            var pool = BACostPoolService.GetPool(Find.CurrentMap);
            pool.Grant(pool.MaximumCost);
        }

        //重置选中角色的新技能阶段与计数，不清除原有技能冷却。
        [BADebugAction(Category, "重置选中角色新技能", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Reset()
        {
            foreach (var state in SelectedStates()) SpecialSkillDispatcher.Reset(state);
        }

        //输出选中角色的新技能状态、结算模式与费用。
        [BADebugAction(Category, "查看选中角色新技能状态", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Inspect()
        {
            foreach (var state in SelectedStates())
            {
                var action = state.profile.exAttack ?? state.profile.burstAttack;
                string mode = action == null ? "状态技能" : action.useBattleStats ? "BA战斗属性" : "独立基数 " + action.basePower;
                string message = state.LabelBase + "\n" + state.TipStringExtra + "\n结算：" + mode;
                Log.Message("[BANW特殊技能] " + message);
                Find.WindowStack.Add(new Dialog_MessageBox(message));
            }
        }

        //直接触发原生普通技能，仍要求角色可行动且附近存在有效敌人。
        [BADebugAction(Category, "触发选中角色普通技能", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerNormal()
        {
            foreach (var state in SelectedStates())
            {
                if (state.profile.role == SpecialSkillRole.Arisu) state.hits = state.profile.requiredHits;
                if (!SpecialSkillDispatcher.TryNormal(state))
                    Messages.Message("没有可执行的普通技能、角色不能行动或没有有效敌人。", MessageTypeDefOf.RejectInput, false);
            }
        }

        //读取选中角色的原生技能状态，没有有效选择时明确提示。
        private static List<Hediff_SpecialSkillState> SelectedStates()
        {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            var result = pawn?.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>().Where(s => s.native).ToList();
            if (result == null || result.Count == 0)
                Messages.Message("请选中一名拥有测试新技能的角色。", MessageTypeDefOf.RejectInput, false);
            return result ?? new List<Hediff_SpecialSkillState>();
        }
    }
}
