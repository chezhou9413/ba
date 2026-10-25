using System;
using System.Collections.Generic;
using HarmonyLib;
using newpro;
using RimWorld;
using Verse;
using Verse.Profile;

namespace BANWlLib.mainUI.Audio
{
    //界面曲目借用原版音乐播放器，只在什亭之匣打开期间接管选曲。
    internal static class BAUiMusic
    {
        private static readonly Dictionary<string, SongDef> songs = new Dictionary<string, SongDef>();
        private static MusicManagerPlay owner;
        private static SongDef selectedSong;

        //曲目不加入 DefDatabase，原版随机选曲、兜底选曲和其他场景均无法选中它们。
        private static SongDef GetSong(string name)
        {
            if (songs.TryGetValue(name, out SongDef song)) return song;
            float volume;
            switch (name)
            {
                case "bgm": case "bgm2": case "bgm3":
                case "bgm4": case "bgm5": case "bgm6": volume = 1f; break;
                default: throw new ArgumentException("未配置的界面曲目：" + name, nameof(name));
            }

            string path = "Songs/" + name;
            UnityEngine.AudioClip clip = ContentFinder<UnityEngine.AudioClip>.Get(path, reportFailure: false);
            if (clip == null) throw new InvalidOperationException("无法加载界面音乐：Sounds/" + path + ".ogg");
            UnityStubs.ImportDefaults.KeepStereo(clip);
            song = new SongDef
            {
                defName = "BANW_UI_" + name,
                clipPath = path,
                clip = clip,
                volume = volume,
                playOnMap = false,
                commonality = 0f
            };
            songs.Add(name, song);
            return song;
        }

        //切页直接调用原版强制切曲；相同曲目继续播放，首次打开会替换地图音乐。
        internal static void Play(string name)
        {
            if (!UiMapData.uiclose) return;
            MusicManagerPlay manager = Find.MusicManagerPlay;
            if (manager == null) throw new InvalidOperationException("原版音乐播放器尚未初始化。");
            SongDef song = GetSong(name);
            if (owner == manager && selectedSong == song && manager.CurrentSong == song && manager.IsPlaying) return;
            owner = manager;
            selectedSong = song;
            manager.Stop();
            manager.ForcePlaySong(song, ignorePrefsVolume: false);
        }

        //关闭界面或卸载存档时撤销接管，由原版按当前地图与危险状态重新选曲。
        internal static void Stop()
        {
            MusicManagerPlay manager = owner;
            owner = null;
            selectedSong = null;
            if (manager != null && Current.ProgramState == ProgramState.Playing &&
                Current.Game != null && manager == Find.MusicManagerPlay)
            {
                manager.Stop();
                manager.ScheduleNewSong();
            }
        }

        //所有补丁只作用于当前打开的界面及其所属播放器。
        private static bool Owns(MusicManagerPlay manager)
        {
            return selectedSong != null && manager == owner && UiMapData.uiclose &&
                Current.ProgramState == ProgramState.Playing;
        }

        //曲目播放完后立即通过同一个原版播放器重播，不等待地图曲目的间隔。
        [HarmonyPatch(typeof(MusicManagerPlay), nameof(MusicManagerPlay.MusicUpdate))]
        private static class LoopSong
        {
            //保留原版声部、音量更新和播放状态计算。
            private static void Postfix(MusicManagerPlay __instance)
            {
                if (__instance != owner) return;
                if (!Owns(__instance)) Stop();
                else if (!__instance.IsPlaying || __instance.CurrentSong != selectedSong)
                    __instance.ForcePlaySong(selectedSong, ignorePrefsVolume: false);
            }
        }

        //原版在曲目结束当帧尝试选歌时，也只播放当前界面曲目。
        [HarmonyPatch(typeof(MusicManagerPlay), nameof(MusicManagerPlay.StartNewSong))]
        private static class SelectUiSong
        {
            //关闭界面后完全交还原版选曲。
            private static bool Prefix(MusicManagerPlay __instance)
            {
                if (!Owns(__instance)) return true;
                __instance.ForcePlaySong(selectedSong, ignorePrefsVolume: false);
                return false;
            }
        }

        //界面打开期间不让袭击或仪式的自动音乐过渡覆盖界面曲目。
        [HarmonyPatch(typeof(MusicManagerPlay), nameof(MusicManagerPlay.CheckTransitions))]
        private static class KeepUiSong
        {
            //仅暂停选曲过渡，原版播放器本身继续正常更新。
            private static bool Prefix(MusicManagerPlay __instance) => !Owns(__instance);
        }

        //界面音乐遵从音乐音量，地图乐器的距离衰减和旧曲淡出不应使它静音。
        [HarmonyPatch(typeof(MusicManagerPlay), "CurVolume", MethodType.Getter)]
        private static class UiVolume
        {
            //总音量仍由原版音频监听器处理，不重复乘算。
            private static bool Prefix(MusicManagerPlay __instance, ref float __result)
            {
                if (!Owns(__instance)) return true;
                __result = selectedSong.volume * Prefs.VolumeMusic;
                return false;
            }
        }

        //退出到主菜单和加载其他存档时，清掉旧界面的音乐请求。
        [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
        private static class EndSession
        {
            //在地图被清理前停止原版声部，避免残留的静态引用跨存档生效。
            private static void Prefix() => Stop();
        }
    }
}
