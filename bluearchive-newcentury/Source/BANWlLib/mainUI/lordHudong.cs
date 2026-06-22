using BANWlLib.BaDef;
using newpro;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace BANWlLib.mainUI
{
    /// <summary>
    /// 负责把阿洛娜与普拉娜的 Spine 对话配置加载到 UI 控制器。
    /// </summary>
    public static class lordHudong
    {
        /// <summary>
        /// 负责加载阿洛娜的启动与点击对话动画配置。
        /// </summary>
        public static void LordArona(aronaSpineUIController aronaSpineUIController)
        {
            AronaSpine aronaSpine = DefDatabase<AronaSpine>.AllDefs.FirstOrDefault();
            if (aronaSpine == null || aronaSpineUIController == null)
            {
                Log.Warning("[BAUI] 阿洛娜 Spine 配置或控制器为空。");
                return;
            }

            foreach (SpineAnimation spineAnimation in aronaSpine.startClickspineAnimationNames)
            {
                string audioClipPath = Path.Combine(UiMapData.modRootPath, spineAnimation.aronaAudioClipPath);
                Log.Message(audioClipPath);
                AudioClip audioClip = LoadAudioClipBlocking(audioClipPath);
                aronaAnimation aronaAnimation = new aronaAnimation();
                aronaAnimation.spineAnimationName = spineAnimation.spineAnimationName;
                aronaAnimation.Mouth_Tex = spineAnimation.Mouth_Tex;
                aronaAnimation.defMouse_Tex = spineAnimation.defMouse_Tex;
                aronaAnimation.aronaAudioClip = audioClip;
                aronaAnimation.isBlink = false;
                aronaAnimation.text = spineAnimation.text;
                aronaSpineUIController.start.Add(aronaAnimation);
            }
            foreach (SpineAnimation spineAnimation in aronaSpine.onClickspineAnimationNames)
            {
                string audioClipPath = Path.Combine(UiMapData.modRootPath, spineAnimation.aronaAudioClipPath);
                Log.Message(audioClipPath);
                AudioClip audioClip = LoadAudioClipBlocking(audioClipPath);
                aronaAnimation aronaAnimation = new aronaAnimation();
                aronaAnimation.spineAnimationName = spineAnimation.spineAnimationName;
                aronaAnimation.Mouth_Tex = spineAnimation.Mouth_Tex;
                aronaAnimation.defMouse_Tex = spineAnimation.defMouse_Tex;
                aronaAnimation.aronaAudioClip = audioClip;
                aronaAnimation.isBlink = false;
                aronaAnimation.text = spineAnimation.text;
                aronaSpineUIController.oneClick.Add(aronaAnimation);
            }
        }

        /// <summary>
        /// 负责加载普拉娜的启动与点击对话动画配置。
        /// </summary>
        public static void LordPunara(aronaSpineUIController aronaSpineUIController)
        {
            PunaraSpine PunaraSpine = DefDatabase<PunaraSpine>.AllDefs.FirstOrDefault();
            if (PunaraSpine == null || aronaSpineUIController == null)
            {
                Log.Warning("[BAUI] 普拉娜 Spine 配置或控制器为空。");
                return;
            }

            foreach (SpineAnimation spineAnimation in PunaraSpine.startClickspineAnimationNames)
            {
                string audioClipPath = Path.Combine(UiMapData.modRootPath, spineAnimation.aronaAudioClipPath);
                Log.Message(audioClipPath);
                AudioClip audioClip = LoadAudioClipBlocking(audioClipPath);
                aronaAnimation aronaAnimation = new aronaAnimation();
                aronaAnimation.spineAnimationName = spineAnimation.spineAnimationName;
                aronaAnimation.Mouth_Tex = spineAnimation.Mouth_Tex;
                aronaAnimation.defMouse_Tex = spineAnimation.defMouse_Tex;
                aronaAnimation.aronaAudioClip = audioClip;
                aronaAnimation.isBlink = false;
                aronaAnimation.text = spineAnimation.text;
                aronaSpineUIController.start.Add(aronaAnimation);
            }
            foreach (SpineAnimation spineAnimation in PunaraSpine.onClickspineAnimationNames)
            {
                string audioClipPath = Path.Combine(UiMapData.modRootPath, spineAnimation.aronaAudioClipPath);
                Log.Message(audioClipPath);
                AudioClip audioClip = LoadAudioClipBlocking(audioClipPath);
                aronaAnimation aronaAnimation = new aronaAnimation();
                aronaAnimation.spineAnimationName = spineAnimation.spineAnimationName;
                aronaAnimation.Mouth_Tex = spineAnimation.Mouth_Tex;
                aronaAnimation.defMouse_Tex = spineAnimation.defMouse_Tex;
                aronaAnimation.aronaAudioClip = audioClip;
                aronaAnimation.isBlink = false;
                aronaAnimation.text = spineAnimation.text;
                aronaSpineUIController.oneClick.Add(aronaAnimation);
            }
        }

        /// <summary>
        /// 负责同步读取本地 OGG 音频文件并返回 Unity 音频对象。
        /// </summary>
        public static AudioClip LoadAudioClipBlocking(string filePath)
        {
            string uri = new System.Uri(filePath).AbsoluteUri;
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
            {
                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone){}
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerAudioClip.GetContent(request);
                }
                else
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// 负责保存单条 Spine 对话动画、口型、音频与文本配置。
    /// </summary>
    public class SpineAnimation
    {
        public string spineAnimationName;
        public string Mouth_Tex;
        public string defMouse_Tex;
        public string aronaAudioClipPath;
        public bool isBlink = true;
        public string text;
    }
}
