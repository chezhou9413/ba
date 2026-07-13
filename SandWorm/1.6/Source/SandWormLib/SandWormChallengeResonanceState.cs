using Verse;

namespace SandWormLib
{
    // SandWormChallengeResonanceState 负责保存和推进挑战拖延压力的触发等级、次数和下一次触发时间。
    public sealed class SandWormChallengeResonanceState : IExposable
    {
        private const int LevelOneInitialDelayTicks = 4800;
        private const int LevelOneIntervalTicks = 3600;
        private const int LevelOneMaxTriggers = 3;
        private const int LevelTwoInitialDelayTicks = 3600;
        private const int LevelTwoIntervalTicks = 2400;
        private const int LevelTwoMaxTriggers = 5;

        private int level;
        private int nextTriggerTick = -1;
        private int triggeredCount;

        public int Level => level;

        public int TriggeredCount => triggeredCount;

        // ExposeData 负责把共振倒计时状态写入存档，保证挑战中读档后继续按原节奏推进。
        public void ExposeData()
        {
            Scribe_Values.Look(ref level, "level", 0);
            Scribe_Values.Look(ref nextTriggerTick, "nextTriggerTick", -1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        // Configure 负责在挑战 Boss 出现时按词条等级启动共振倒计时。
        public void Configure(int newLevel, int startTick)
        {
            if (newLevel <= 0)
            {
                Reset();
                return;
            }

            level = newLevel >= 2 ? 2 : 1;
            triggeredCount = 0;
            nextTriggerTick = startTick + InitialDelayTicks(level);
        }

        // Reset 负责清空共振倒计时，避免挑战结束后继续触发增援。
        public void Reset()
        {
            level = 0;
            nextTriggerTick = -1;
            triggeredCount = 0;
        }

        // CanTrigger 负责判断当前 tick 是否已经达到下一次共振增援时间。
        public bool CanTrigger(int currentTick)
        {
            return level > 0
                && nextTriggerTick > 0
                && triggeredCount < MaxTriggers(level)
                && currentTick >= nextTriggerTick;
        }

        // NotifyTriggered 负责记录一次共振触发，并安排下一次触发时间或关闭倒计时。
        public void NotifyTriggered(int currentTick)
        {
            if (level <= 0)
            {
                Reset();
                return;
            }

            triggeredCount++;
            if (triggeredCount >= MaxTriggers(level))
            {
                nextTriggerTick = -1;
                return;
            }

            nextTriggerTick = currentTick + IntervalTicks(level);
        }

        // InitialDelayTicks 负责按等级返回 Boss 出现后的首次增援延迟。
        private static int InitialDelayTicks(int level)
        {
            return level >= 2 ? LevelTwoInitialDelayTicks : LevelOneInitialDelayTicks;
        }

        // IntervalTicks 负责按等级返回每次共振增援之间的间隔。
        private static int IntervalTicks(int level)
        {
            return level >= 2 ? LevelTwoIntervalTicks : LevelOneIntervalTicks;
        }

        // MaxTriggers 负责按等级返回本次挑战最多触发多少次共振增援。
        private static int MaxTriggers(int level)
        {
            return level >= 2 ? LevelTwoMaxTriggers : LevelOneMaxTriggers;
        }
    }
}
