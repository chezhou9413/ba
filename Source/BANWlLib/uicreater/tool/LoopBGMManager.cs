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
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace MyCoolMusicMod
{
    using HarmonyLib;
    // using newpro; // 假设这是你的其他命名空间
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Verse;
    using static RimWorld.FleshTypeDef;

    namespace MyCoolMusicMod
    {
        public class LordBgmData : Mod
        {
            public LordBgmData(ModContentPack content) : base(content)
            {
                UiMapData.modRootPath = content.RootDir;
                LoopBGMManager.LoadAllBgm();
                LoopBGMManager.LoadAllAudio();
            }
        }

        public static class LoopBGMManager
        {
            public static Dictionary<string, AudioClip> bgmPool = new Dictionary<string, AudioClip>();

            public static Dictionary<string, AudioClip> audioPool = new Dictionary<string, AudioClip>();
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
                // 1. 防空检查
                if (string.IsNullOrEmpty(UiMapData.modRootPath))
                {
                    Log.Error("UiMapData.modRootPath 尚未初始化！");
                    return;
                }
                if (!relativePath.EndsWith(".ogg"))
                {
                    relativePath += ".ogg";
                }
                // 2. 组合完整路径
                string fullPath = Path.Combine(UiMapData.modRootPath, relativePath);

                // 3. 检查文件是否存在
                if (!File.Exists(fullPath))
                {
                    Log.Warning($"试图播放不存在的音频文件: {fullPath}");
                    return;
                }

                // 4. 异步加载音频
                AudioClip clip = await LoadAudioAsync(fullPath);

                // 5. 播放
                if (clip != null && UiMapData.mainAudioPlay != null)
                {
                    // 这里的关键是 clip 的名称要设好，方便调试
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
                UiMapData.mainBgmPlay.volume = gameVolume;
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
                        if (UiMapData.mainBgmPlay.clip != audioClip)
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

            /// <summary>
            /// 播放音效/BGM
            /// </summary>
            /// <param name="AudioName">音频名称</param>
            /// <param name="volume">音量 (0.0 ~ 1.0)，默认为1.0</param>
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
            }
        }
    }
}
