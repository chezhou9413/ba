using BANWlLib.BaDef;
using newpro;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace BANWlLib.mainUI
{
    public static class lordHudong
    {
        public static void LordArona(aronaSpineUIController aronaSpineUIController)
        {
            AronaSpine aronaSpine = DefDatabase<AronaSpine>.AllDefs.FirstOrDefault();
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
                aronaAnimation.isBlink = spineAnimation.isBlink;
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
                aronaAnimation.isBlink = spineAnimation.isBlink;
                aronaAnimation.text = spineAnimation.text;
                aronaSpineUIController.oneClick.Add(aronaAnimation);
            }
        }

        public static void LordPunara(aronaSpineUIController aronaSpineUIController)
        {
            PunaraSpine PunaraSpine = DefDatabase<PunaraSpine>.AllDefs.FirstOrDefault();
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
                aronaAnimation.isBlink = spineAnimation.isBlink;
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
                aronaAnimation.isBlink = spineAnimation.isBlink;
                aronaAnimation.text = spineAnimation.text;
                aronaSpineUIController.oneClick.Add(aronaAnimation);
            }
        }
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
