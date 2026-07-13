using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace SandWormLib
{
    public sealed class QuestNode_Root_SandWormLeviathan : QuestNode
    {
        private const int ChallengeRating = 4;

        // RunInt 负责创建沙虫根任务，并让任务保持可接受状态直到玩家主动处理。
        protected override void RunInt()
        {
            Quest quest = QuestGen.quest;

            quest.name = SandWormQuestText.GetQuestName();
            quest.description = SandWormQuestText.GetQuestDescription();
            quest.challengeRating = ChallengeRating;
            quest.tags.Add(SandWormQuestDefs.LeviathanQuestTag);
            quest.hidden = false;
            quest.hiddenInUI = false;

            QuestPart_LeviathanAccept acceptPart = new QuestPart_LeviathanAccept();
            acceptPart.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
            quest.AddPart(acceptPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return Find.AnyPlayerHomeMap != null && !SandWormQuestUtility.HasBlockingLeviathanQuest();
        }
    }
}
