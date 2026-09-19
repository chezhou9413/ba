using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace BANWlLib.mainUI.Audio
{
    //按实际播放请求异步读取对话语音，缓存归属于当前控制器并随界面销毁释放。
    public sealed class BADialogueAudioPlayer : MonoBehaviour
    {
        private readonly Dictionary<aronaAnimation, string> paths = new Dictionary<aronaAnimation, string>();
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
        private aronaSpineUIController controller;
        private UnityWebRequest request;
        private Coroutine loading;
        private aronaAnimation pending;

        //绑定控制器，语音播放仍交由其原有动画、口型和字幕逻辑处理。
        internal void Initialize(aronaSpineUIController target)
        {
            controller = target;
        }

        //只登记语音路径，不在主界面初始化时读取或解码音频。
        internal void Register(aronaAnimation animation, string file)
        {
            paths.Add(animation, file);
        }

        //已缓存的语音立即播放，未加载的语音在请求完成后继续原有播放流程。
        internal bool PlayOrQueue(aronaAnimation animation)
        {
            if (animation == null || !paths.TryGetValue(animation, out string file)) return true;
            if (!isActiveAndEnabled || controller == null || !controller.isActiveAndEnabled) return false;
            if (pending == animation && request != null) return false;
            CancelPending();
            if (clips.TryGetValue(file, out AudioClip clip))
            {
                animation.aronaAudioClip = clip;
                return true;
            }
            pending = animation;
            loading = StartCoroutine(LoadAndPlay(animation, file));
            return false;
        }

        //让出主线程等待本地 OGG 请求完成，仅在当前页面仍有效时开始播放。
        private IEnumerator LoadAndPlay(aronaAnimation animation, string file)
        {
            if (!File.Exists(file))
            {
                Log.Error("[BA UI语音] 文件不存在：" + file);
                pending = null;
                yield break;
            }
            request = UnityWebRequestMultimedia.GetAudioClip(new Uri(file).AbsoluteUri, AudioType.OGGVORBIS);
            request.timeout = 30;
            yield return request.SendWebRequest();

            UnityWebRequest completed = request;
            request = null;
            loading = null;
            pending = null;
            AudioClip clip;
            using (completed)
            {
                if (completed.result != UnityWebRequest.Result.Success)
                {
                    Log.Error("[BA UI语音] 加载失败：" + file + "，" + completed.error);
                    yield break;
                }
                clip = DownloadHandlerAudioClip.GetContent(completed);
            }
            if (clip == null)
            {
                Log.Error("[BA UI语音] 音频解码结果为空：" + file);
                yield break;
            }
            clip.name = Path.GetFileNameWithoutExtension(file);
            clips.Add(file, clip);
            animation.aronaAudioClip = clip;
            if (controller != null && controller.isActiveAndEnabled)
                controller.PlayArona(animation);
        }

        //关闭页面或切换角色时取消尚未完成的播放，保留已经加载的语音供再次打开使用。
        private void OnDisable()
        {
            CancelPending();
        }

        //读档或销毁界面时停止播放并释放本控制器拥有的语音对象。
        private void OnDestroy()
        {
            CancelPending();
            if (controller != null && controller.audioSource != null) controller.audioSource.Stop();
            foreach (aronaAnimation animation in paths.Keys) animation.aronaAudioClip = null;
            foreach (AudioClip clip in clips.Values) UnityEngine.Object.Destroy(clip);
            clips.Clear();
            paths.Clear();
        }

        //终止当前协程和请求，防止旧页面或被后续点击替代的语音延迟播放。
        private void CancelPending()
        {
            if (loading != null) StopCoroutine(loading);
            loading = null;
            pending = null;
            if (request == null) return;
            request.Abort();
            request.Dispose();
            request = null;
        }
    }
}
