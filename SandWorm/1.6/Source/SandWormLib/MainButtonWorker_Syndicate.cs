using RimWorld;
using Verse;

namespace SandWormLib
{
    // MainButtonWorker_Syndicate 负责控制辛迪加底栏按钮的显示条件，并沿用原版主标签页开关行为。
    public sealed class MainButtonWorker_Syndicate : MainButtonWorker_ToggleTab
    {
        // Visible 负责在利维坦委托完成后显示辛迪加按钮，同时兼容已经完成委托的旧存档。
        public override bool Visible
        {
            get
            {
                return base.Visible && (DebugSettings.godMode || SandWormQuestUtility.IsSyndicateButtonUnlocked());
            }
        }
    }
}
