using System.Linq;
using RimWorld;
using Verse;

namespace SandWormLib
{
    // SandWormQuestGameComponent 负责保存沙虫委托的全局进度，并按固定间隔触发委托与报酬逻辑。
    public sealed class SandWormQuestGameComponent : GameComponent
    {
        private const float RequiredWealth = 500000f;
        private const int CheckIntervalTicks = 600;
        private const int RewardDelayTicks = 120000;

        private bool questOffered;
        private bool luciferiumRewardPending;
        private bool syndicateButtonUnlocked;
        private int luciferiumRewardTick = -1;

        // SandWormQuestGameComponent 负责让 RimWorld 在创建或读取存档组件时实例化委托状态容器。
        public SandWormQuestGameComponent(Game game)
        {
        }

        // GameComponentTick 负责周期性检查委托触发条件，并在延迟结束后投放辛迪加报酬。
        public override void GameComponentTick()
        {
            TryDeliverPendingLuciferiumReward();

            if (Find.TickManager.TicksGame <= 0 || Find.TickManager.TicksGame % CheckIntervalTicks != 0)
            {
                return;
            }

            if (questOffered && SandWormQuestUtility.HasBlockingLeviathanQuest())
            {
                return;
            }

            questOffered = false;

            if (!Find.Maps.Any(delegate(Map map) { return map.IsPlayerHome; }))
            {
                return;
            }

            float wealth = 0f;
            foreach (Map map in Find.Maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                wealth += map.wealthWatcher.WealthTotal;
            }

            if (wealth < RequiredWealth)
            {
                return;
            }

            if (SandWormQuestUtility.HasBlockingLeviathanQuest())
            {
                questOffered = true;
                return;
            }

            if (SandWormQuestUtility.TryCreateLeviathanQuest())
            {
                questOffered = true;
            }
        }

        // ExposeData 负责把委托是否出现、报酬延迟和辛迪加按钮解锁状态写入存档。
        public override void ExposeData()
        {
            Scribe_Values.Look(ref questOffered, "questOffered", defaultValue: false);
            Scribe_Values.Look(ref luciferiumRewardPending, "luciferiumRewardPending", defaultValue: false);
            Scribe_Values.Look(ref syndicateButtonUnlocked, "syndicateButtonUnlocked", defaultValue: false);
            Scribe_Values.Look(ref luciferiumRewardTick, "luciferiumRewardTick", -1);
        }

        // ScheduleLuciferiumReward 负责在利维坦猎杀完成后安排辛迪加延迟投放魔鬼素报酬。
        public void ScheduleLuciferiumReward()
        {
            if (luciferiumRewardPending)
            {
                return;
            }

            luciferiumRewardPending = true;
            luciferiumRewardTick = Find.TickManager.TicksGame + RewardDelayTicks;
        }

        // UnlockSyndicateButton 负责永久解锁底栏中的辛迪加主按钮。
        public void UnlockSyndicateButton()
        {
            syndicateButtonUnlocked = true;
        }

        // SyndicateButtonUnlocked 负责提供当前存档是否已经显示辛迪加主按钮。
        public bool SyndicateButtonUnlocked()
        {
            return syndicateButtonUnlocked;
        }

        // TryDeliverPendingLuciferiumReward 负责在报酬到期时寻找主殖民地并投放辛迪加付款。
        private void TryDeliverPendingLuciferiumReward()
        {
            if (!luciferiumRewardPending || luciferiumRewardTick < 0 || Find.TickManager.TicksGame < luciferiumRewardTick)
            {
                return;
            }

            if (SandWormQuestUtility.TryDropLuciferiumReward())
            {
                luciferiumRewardPending = false;
                luciferiumRewardTick = -1;
            }
        }
    }
}
