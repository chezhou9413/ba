using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Tool;

//宿主生成新 Pawn 时未检查 ForceNoIdeo，头像预览在生成健康状况前恢复这个请求。
[HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
internal static class PreviewPawnIdeologyPatch
{
    //只处理本工具创建的临时头像，避免玩家意识形态给预览对象添加伤疤或失明。
    private static void Prefix(Pawn pawn, PawnGenerationRequest request)
    {
        if (RimWorldUISpriteUtil.GeneratingPreview && request.ForceNoIdeo && pawn.ideo != null)
            pawn.ideo = new Pawn_IdeoTracker(pawn);
    }
}
