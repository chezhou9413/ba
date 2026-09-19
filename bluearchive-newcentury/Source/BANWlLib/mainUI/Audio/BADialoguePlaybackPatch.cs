using HarmonyLib;

namespace BANWlLib.mainUI.Audio
{
    //在原有 Spine 对话读取音频长度之前取得语音，保持预制体与字幕接口不变。
    [HarmonyPatch(typeof(aronaSpineUIController), nameof(aronaSpineUIController.PlayArona))]
    public static class BADialoguePlaybackPatch
    {
        //仅接管已登记语音路径的 BA 控制器，异步等待期间跳过需要音频的原播放方法。
        public static bool Prefix(aronaSpineUIController __instance, aronaAnimation __0)
        {
            BADialogueAudioPlayer player = __instance.GetComponent<BADialogueAudioPlayer>();
            return player == null || player.PlayOrQueue(__0);
        }
    }
}
