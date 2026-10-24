using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //仅为星野的指定盾牌服装安装形态绘制规则。
    [HarmonyPatch(typeof(DynamicPawnRenderNodeSetup_Apparel), nameof(DynamicPawnRenderNodeSetup_Apparel.GetDynamicNodes))]
    public static class HoshinoShieldApparelPatch
    {
        //修改当前角色新建的节点属性，不改变共享服装定义或其他角色节点。
        public static IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> Postfix(
            IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> __result, Pawn pawn)
        {
            var state = Hediff_SpecialSkillState.Find(pawn, SpecialSkillRole.Hoshino);
            foreach (var entry in __result)
            {
                if (state != null && entry.node.apparel?.def.defName == "BANW_Body_Hoshiro_Shield" &&
                    entry.node.apparel.def == state.profile.hoshino.shieldApparel)
                    entry.node.Props.workerClass = typeof(PawnRenderNodeWorker_HoshinoShield);
                yield return entry;
            }
        }
    }
}
