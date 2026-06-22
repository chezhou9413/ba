using BANWlLib.BANWGamecomp;
using BANWlLib.mainUI.Gaka;
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
    public static class GachaDebugActions
    {
        [DebugAction("BA", "抽卡必出三星", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        private static void ToggleGuarantee3Star()
        {
            GachaSystem.DebugGuarantee3Star = !GachaSystem.DebugGuarantee3Star;
            string status = GachaSystem.DebugGuarantee3Star ? "开启" : "关闭";
            Messages.Message($"抽卡必出三星: {status}", MessageTypeDefOf.NeutralEvent, false);
        }

        [DebugAction("BA", "增加100点招募点数", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        private static void addpoit()
        {
            Gamecomp_gakaAction gamecomp_Gaka = Current.Game.GetComponent<Gamecomp_gakaAction>();
            gamecomp_Gaka.updataGacaPoit(100);
            Messages.Message("增加了100招募点数",MessageTypeDefOf.NeutralEvent, false);
        }
    }
}
