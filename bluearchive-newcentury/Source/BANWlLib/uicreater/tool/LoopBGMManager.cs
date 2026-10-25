using Engine = RimUnity;
using newpro;
using BANWlLib.mainUI.Audio;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RimUnity;
using Verse;

namespace MyCoolMusicMod
{
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

        //缓存界面音效，并把背景音乐切换交给原版播放器。
        public static class LoopBGMManager
        {
            public static Dictionary<string, AudioClip> audioPool = new Dictionary<string, AudioClip>();
            private static bool audioLoadStarted;
            private static Task audioLoadTask;
            private static int effectRequestVersion;
            private static AudioClip directClip;

            //公共音效只在 BA UI 初始化阶段加载一次，音乐使用原版内容缓存。
            public static void EnsureAudioLoaded()
            {
                if (audioLoadStarted)
                {
                    return;
                }

                audioLoadStarted = true;
                audioLoadTask = LoadAllAudio();
            }

            //按学生配置加载并播放拖动等独立音效。
            public static async void PlayClipDirectly(string relativePath)
            {
                int requestVersion = ++effectRequestVersion;
                AudioSource source = UiMapData.mainAudioPlay;
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

                if (clip == null) return;
                if (requestVersion != effectRequestVersion || source == null ||
                    source != UiMapData.mainAudioPlay || !source.isActiveAndEnabled)
                {
                    Engine.Object.Destroy(clip);
                    return;
                }
                source.Stop();
                ReleaseDirectClip();
                directClip = clip;
                clip.name = Path.GetFileNameWithoutExtension(fullPath);
                source.clip = clip;
                try
                {
                    SetVolumeToGamePrefs();
                    source.Play();
                }
                catch (Exception error)
                {
                    source.Stop();
                    ReleaseDirectClip();
                    Log.Error($"[BA UI音频] 独立音效播放失败：{fullPath}\n{error}");
                }
            }
            //关闭或重建界面时取消延迟请求，并释放独立加载的临时音频。
            public static void StopUiAudio()
            {
                BAUiMusic.Stop();
                ++effectRequestVersion;
                UiMapData.mainAudioPlay?.Stop();
                ReleaseDirectClip();
            }

            //仅释放直接加载的片段，公共音效缓存由音乐管理器继续持有。
            private static void ReleaseDirectClip()
            {
                if (directClip == null) return;
                if (UiMapData.mainAudioPlay != null && UiMapData.mainAudioPlay.clip == directClip)
                    UiMapData.mainAudioPlay.clip = null;
                Engine.Object.Destroy(directClip);
                directClip = null;
            }
            //按钮与角色独立音效沿用游戏音量。
            public static void SetVolumeToGamePrefs()
            {

                float gameVolume = Prefs.VolumeGame;
                UiMapData.mainAudioPlay.volume = gameVolume;
            }
            //缓存公共按钮音效，单个文件失败由加载器记录，不阻断其他音效。
            public static async Task LoadAllAudio()
            {
                if (string.IsNullOrEmpty(UiMapData.modRootPath))
                {
                    Log.Error("UiMapData.modRootPath 尚未初始化！");
                    return;
                }
                string audioFolderPath = Path.Combine(UiMapData.modRootPath, "Sounds", "UIAudio");
                if (!Directory.Exists(audioFolderPath))
                {
                    Log.Warning($"找不到UI音效文件夹: {audioFolderPath}");
                    return;
                }
                try
                {
                    foreach (string filePath in Directory.GetFiles(audioFolderPath, "*.ogg", SearchOption.TopDirectoryOnly))
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
                catch (Exception error)
                {
                    Log.Error($"[BA UI音频] 无法读取UI音效目录：{audioFolderPath}\n{error}");
                }
            }
            //分页直接切换原版播放器中的曲目，不依赖页面上的 AudioSource 活动状态。
            public static void switchUiBgm(string bgmName)
            {
                try
                {
                    BAUiMusic.Play(bgmName);
                }
                catch (Exception error)
                {
                    BAUiMusic.Stop();
                    Log.Error($"[BA UI音频] BGM播放失败：{bgmName}\n{error}");
                }
            }

            //首次播放等待音效加载，关闭页面或后续请求会取消旧播放请求。
            public static async void playEffAudio(string AudioName, float volume = 1.0f)
            {
                try
                {
                    int requestVersion = ++effectRequestVersion;
                    AudioSource source = UiMapData.mainAudioPlay;
                    EnsureAudioLoaded();
                    await audioLoadTask;
                    if (requestVersion != effectRequestVersion || source == null ||
                        source != UiMapData.mainAudioPlay || !source.isActiveAndEnabled) return;
                    if (audioPool.TryGetValue(AudioName, out AudioClip audioClip))
                    {
                        source.Stop();
                        ReleaseDirectClip();
                        source.clip = audioClip;
                        source.volume = Prefs.VolumeGame * Mathf.Clamp01(volume);
                        source.Play();
                    }
                    else
                    {
                        Log.Warning($"[BA UI音频] UI音效未能加载：{AudioName}，请检查前面的音频加载日志。");
                    }
                }
                catch (Exception error)
                {
                    Log.Error($"[BA UI音频] UI音效播放失败：{AudioName}\n{error}");
                }
            }
            //按版本使用实际音频后端，解码失败时记录路径和异常，继续处理其他文件。
            public static async Task<AudioClip> LoadAudioAsync(string absolutePath)
            {
                try
                {
                    return AudioClip.LoadFromFile(absolutePath, streaming: true);
                }
                catch (Exception error)
                {
                    Log.Error($"[BA UI音频] 解码失败：{absolutePath}\n{error}");
                    return null;
                }
            }
        }
    }
}
