#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using HarmonyLib;
using newpro;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using global::UnityEngine.Networking;
#else
using UnityEngine.Networking;
#endif
using Verse;

namespace MyCoolMusicMod
{
    using HarmonyLib;
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
#if RW_1_7
    using RimUnity;
#else
    using UnityEngine;
#endif
    using global::UnityEngine.Networking;
    using Verse;
    using static RimWorld.FleshTypeDef;

    namespace MyCoolMusicMod
    {
        //音频入口类保留 RimWorld 对旧 Mod 类的实例化路径，并执行轻量全局初始化。
        public class LordBgmData : Mod
        {
            //构造函数只记录模组根目录并安装补丁，音频解码延后到 BA UI 初始化阶段。
            public LordBgmData(ModContentPack content) : base(content)
            {
                UiMapData.modRootPath = content.RootDir;
                BANWlLib.ModMain.ApplyHarmonyPatches();
            }
        }

        public static class LoopBGMManager
        {
            public static Dictionary<string, AudioClip> bgmPool = new Dictionary<string, AudioClip>();

            public static Dictionary<string, AudioClip> audioPool = new Dictionary<string, AudioClip>();
            private static bool audioLoadStarted;

            //启动 UI 音频加载，保证 BGM 和音效只在 BA UI 初始化阶段异步加载一次。
            public static void EnsureAudioLoaded()
            {
                if (audioLoadStarted)
                {
                    return;
                }

                audioLoadStarted = true;
                LoadAllBgm();
                LoadAllAudio();
            }

            public static async void LoadAllBgm()
            {
                // 防空检查
                if (string.IsNullOrEmpty(UiMapData.modRootPath))
                {
                    Log.Error("UiMapData.modRootPath 尚未初始化！");
                    return;
                }
                string bgmFolderPath = Path.Combine(UiMapData.modRootPath, "Sounds", "Songs");
                if (!Directory.Exists(bgmFolderPath))
                {
                    Log.Warning($"找不到BGM文件夹: {bgmFolderPath}");
                    return;
                }
                string[] files = Directory.GetFiles(bgmFolderPath, "*.ogg", SearchOption.TopDirectoryOnly);

                foreach (string filePath in files)
                {
                    string fileNameKey = Path.GetFileNameWithoutExtension(filePath);

                    if (bgmPool.ContainsKey(fileNameKey)) continue;
                    AudioClip clip = await LoadAudioAsync(filePath);
                    if (clip != null)
                    {
                        clip.name = fileNameKey;
                        bgmPool.Add(fileNameKey, clip);
                    }
                }
            }

            public static async void PlayClipDirectly(string relativePath)
            {
                if (string.IsNullOrEmpty(UiMapData.modRootPath))
                {
                    Log.Error("UiMapData.modRootPath 尚未初始化！");
                    return;
                }
                if (!relativePath.EndsWith(".ogg"))
                {
                    relativePath += ".ogg";
                }
                string fullPath = Path.Combine(UiMapData.modRootPath, relativePath);

                if (!File.Exists(fullPath))
                {
                    Log.Warning($"试图播放不存在的音频文件: {fullPath}");
                    return;
                }

                //异步加载音频。
                AudioClip clip = await LoadAudioAsync(fullPath);

                if (clip != null && UiMapData.mainAudioPlay != null)
                {
                    //按文件名设置音频名称，便于日志定位。
                    clip.name = Path.GetFileNameWithoutExtension(fullPath);

                    // 更新音量 (以防设置变了)
                    SetVolumeToGamePrefs();

                    // 赋值并播放
                    UiMapData.mainAudioPlay.clip = clip;
                    UiMapData.mainAudioPlay.Stop();
                    UiMapData.mainAudioPlay.Play();
                }
            }
            public static void SetVolumeToGamePrefs()
            {

                float gameVolume = Prefs.VolumeGame;
                UiMapData.mainAudioPlay.volume = gameVolume;
                UiMapData.mainBgmPlay.volume = Prefs.VolumeMusic;
            }
            public static async void LoadAllAudio()
            {
                // 防空检查
                if (string.IsNullOrEmpty(UiMapData.modRootPath))
                {
                    Log.Error("UiMapData.modRootPath 尚未初始化！");
                    return;
                }
                string bgmFolderPath = Path.Combine(UiMapData.modRootPath, "Sounds", "UIAudio");
                if (!Directory.Exists(bgmFolderPath))
                {
                    return;
                }
                string[] files = Directory.GetFiles(bgmFolderPath, "*.ogg", SearchOption.TopDirectoryOnly);

                foreach (string filePath in files)
                {
                    string fileNameKey = Path.GetFileNameWithoutExtension(filePath);

                    if (audioPool.ContainsKey(fileNameKey)) continue;
                    AudioClip clip = await LoadAudioAsync(filePath);

                    if (clip != null)
                    {
                        clip.name = fileNameKey;
                        audioPool.Add(fileNameKey, clip);
                    }
                }
            }
            public static void switchUiBgm(string bgmName)
            {
                SetVolumeToGamePrefs();
                if (bgmPool.TryGetValue(bgmName, out AudioClip audioClip))
                {
                    if (UiMapData.mainBgmPlay != null)
                    {
                        //关闭界面会停止声部，同一曲目再次打开时也需要恢复播放。
                        if (UiMapData.mainBgmPlay.clip != audioClip || !UiMapData.mainBgmPlay.isPlaying)
                        {
                            UiMapData.mainBgmPlay.clip = audioClip;
                            UiMapData.mainBgmPlay.Play();
                        }
                    }
                }
                else
                {
                    Log.Warning($"试图播放不存在的BGM: {bgmName}");
                }
            }

            //播放音效或 BGM，volume 范围为 0～1，默认 1。
            public static void playEffAudio(string AudioName, float volume = 1.0f)
            {
                SetVolumeToGamePrefs();

                if (audioPool.TryGetValue(AudioName, out AudioClip audioClip))
                {
                    if (UiMapData.mainAudioPlay != null)
                    {
                        UiMapData.mainAudioPlay.clip = audioClip;
                        UiMapData.mainAudioPlay.volume = Mathf.Clamp01(volume); // 限制在 0~1 范围内
                        UiMapData.mainAudioPlay.Play();
                    }
                }
                else
                {
                    Log.Warning($"试图播放不存在的BGM: {AudioName}");
                }
            }
            public static async Task<AudioClip> LoadAudioAsync(string absolutePath)
            {
#if RW_1_7
                return AudioClip.LoadFromFile(absolutePath, streaming: true);
#else
                string url = "file://" + absolutePath;
                using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS))
                {
                    ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true;
                    var operation = uwr.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }
                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Log.Error($"加载音频失败: {uwr.error}");
                        return null;
                    }
                    return DownloadHandlerAudioClip.GetContent(uwr);
                }
#endif
            }
        }
    }
}
