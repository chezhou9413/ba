using BANWlLib.BaDef;
using BANWlLib.BANWGamecomp;
using BANWlLib.mainUI.Mission.GameComp;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.Dev
{
    public static class MissionDebug
    {
        [DebugAction("BA", "强制刷新卡池", actionType = DebugActionType.Action)]
        public static void Debug_ForceRefreshPool()
        {
            var comp = Current.Game.GetComponent<Gamecomp_gakaAction>();
            if (comp == null)
            {
                Log.Error("[BANW] Gamecomp_gakaAction 未找到");
                return;
            }
            comp.Debug_ForceNextPool();
        }
        /// <summary>
        /// 查看所有任务节点信息
        /// </summary>
        [DebugAction("BA", "查看所有任务节点", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogAllMissionNodes()
        {
            List<BaMissionNode> allMissions = DefDatabase<BaMissionNode>.AllDefs.OrderBy(x => x.oder).ToList();

            if (allMissions == null || allMissions.Count == 0)
            {
                Log.Warning("未找到任何任务节点!");
                return;
            }

            Log.Warning("========== 所有任务节点信息 ==========");
            foreach (var mission in allMissions)
            {
                Log.Message($"任务名称: {mission.defName}");
                Log.Message($"  - 任务ID: {mission.MissionID}");
                Log.Message($"  - 任务标题: {mission.MissionTitle}");
                Log.Message($"  - 任务类型: {mission.MissionType?.defName ?? "未设置"}");
                Log.Message($"  - 顺序: {mission.oder}");
                Log.Message($"  - 前置任务: {mission.UnlockedOn?.defName ?? "无"}");
                Log.Message("---");
            }
            Log.Warning("========== 任务节点信息查看完毕 ==========");
        }

        /// <summary>
        /// 完成指定的任务
        /// </summary>
        [DebugAction("BA", "完成指定任务", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void CompleteMission()
        {
            List<BaMissionNode> allMissions = DefDatabase<BaMissionNode>.AllDefs.OrderBy(x => x.oder).ToList();

            if (allMissions == null || allMissions.Count == 0)
            {
                Log.Warning("未找到任何任务节点!");
                return;
            }

            // 创建选择菜单
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            foreach (var mission in allMissions)
            {
                BaMissionNode missionCopy = mission; // 闭包捕获
                string label = $"{mission.defName} - {mission.MissionTitle}";
                
                options.Add(new FloatMenuOption(label, () =>
                {
                    CompleteMissionInternal(missionCopy);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>
        /// 内部方法：完成任务的具体逻辑
        /// </summary>
        public static void CompleteMissionInternal(BaMissionNode mission)
        {
            GameComp_TaskQuest taskComp = Current.Game.GetComponent<GameComp_TaskQuest>();

            if (taskComp == null)
            {
                Log.Warning("GameComp_TaskQuest 未找到!");
                return;
            }

            // 检查任务是否已经完成
            if (taskComp.MissionQuest.Contains(mission))
            {
                Log.Warning($"任务 {mission.defName} 已经完成过了!");
                return;
            }

            // 添加任务到已完成列表
            taskComp.MissionQuest.Add(mission);
            Log.Message($"✓ 成功完成任务: {mission.defName} ({mission.MissionTitle})");
            Log.Message($"  - 任务ID: {mission.MissionID}");
            Log.Message($"  - 任务类型: {mission.MissionType?.defName ?? "未设置"}");
        }

        /// <summary>
        /// 查看已完成的任务列表
        /// </summary>
        [DebugAction("BA", "查看已完成的任务", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogCompletedMissions()
        {
            GameComp_TaskQuest taskComp = Current.Game.GetComponent<GameComp_TaskQuest>();

            if (taskComp == null)
            {
                Log.Warning("GameComp_TaskQuest 未找到!");
                return;
            }

            if (taskComp.MissionQuest == null || taskComp.MissionQuest.Count == 0)
            {
                Log.Warning("还没有完成任何任务!");
                return;
            }

            Log.Warning("========== 已完成的任务列表 ==========");
            foreach (var mission in taskComp.MissionQuest)
            {
                Log.Message($"✓ {mission.defName} - {mission.MissionTitle}");
                Log.Message($"  - 任务ID: {mission.MissionID}");
                Log.Message($"  - 任务类型: {mission.MissionType?.defName ?? "未设置"}");
            }
            Log.Warning($"========== 共完成 {taskComp.MissionQuest.Count} 个任务 ==========");
        }

        /// <summary>
        /// 重置所有任务（清空已完成列表）
        /// </summary>
        [DebugAction("BA", "重置所有任务", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetAllMissions()
        {
            GameComp_TaskQuest taskComp = Current.Game.GetComponent<GameComp_TaskQuest>();

            if (taskComp == null)
            {
                Log.Warning("GameComp_TaskQuest 未找到!");
                return;
            }

            int count = taskComp.MissionQuest.Count;
            taskComp.MissionQuest.Clear();
            Log.Message($"✓ 已重置所有任务，清空了 {count} 个已完成的任务记录");
        }

        /// <summary>
        /// 完成所有任务
        /// </summary>
        [DebugAction("BA", "完成所有任务", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void CompleteAllMissions()
        {
            GameComp_TaskQuest taskComp = Current.Game.GetComponent<GameComp_TaskQuest>();

            if (taskComp == null)
            {
                Log.Warning("GameComp_TaskQuest 未找到!");
                return;
            }

            List<BaMissionNode> allMissions = DefDatabase<BaMissionNode>.AllDefs.ToList();

            if (allMissions == null || allMissions.Count == 0)
            {
                Log.Warning("未找到任何任务节点!");
                return;
            }

            int completedCount = 0;
            foreach (var mission in allMissions)
            {
                if (!taskComp.MissionQuest.Contains(mission))
                {
                    taskComp.MissionQuest.Add(mission);
                    completedCount++;
                }
            }

            Log.Message($"✓ 成功完成所有任务!");
            Log.Message($"  - 新完成: {completedCount} 个任务");
            Log.Message($"  - 总计: {taskComp.MissionQuest.Count} 个任务");
        }

        /// <summary>
        /// 删除指定任务的完成记录
        /// </summary>
        [DebugAction("BA", "删除指定任务的完成记录", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveMissionCompletion()
        {
            GameComp_TaskQuest taskComp = Current.Game.GetComponent<GameComp_TaskQuest>();

            if (taskComp == null)
            {
                Log.Warning("GameComp_TaskQuest 未找到!");
                return;
            }

            if (taskComp.MissionQuest == null || taskComp.MissionQuest.Count == 0)
            {
                Log.Warning("还没有完成任何任务!");
                return;
            }

            // 创建选择菜单
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            foreach (var mission in taskComp.MissionQuest)
            {
                BaMissionNode missionCopy = mission; // 闭包捕获
                string label = $"{mission.defName} - {mission.MissionTitle}";
                
                options.Add(new FloatMenuOption(label, () =>
                {
                    taskComp.MissionQuest.Remove(missionCopy);
                    Log.Message($"✓ 已删除任务 {missionCopy.defName} 的完成记录");
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}

