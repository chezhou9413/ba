using BANWlLib.BaDef;
using BANWlLib.mainUI.Audio;
using newpro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace BANWlLib.mainUI
{
    //把阿洛娜与普拉娜的对话配置绑定到控制器，音频由播放请求按需取得。
    public static class lordHudong
    {
        //读取阿洛娜的启动与点击对话配置。
        public static void LordArona(aronaSpineUIController controller)
        {
            AronaSpine config = DefDatabase<AronaSpine>.AllDefs.FirstOrDefault();
            if (config == null || controller == null)
            {
                Log.Error("[BAUI] 阿洛娜 Spine 配置或控制器为空。");
                return;
            }
            Bind(controller, config.startClickspineAnimationNames, config.onClickspineAnimationNames);
        }

        //读取普拉娜的启动与点击对话配置。
        public static void LordPunara(aronaSpineUIController controller)
        {
            PunaraSpine config = DefDatabase<PunaraSpine>.AllDefs.FirstOrDefault();
            if (config == null || controller == null)
            {
                Log.Error("[BAUI] 普拉娜 Spine 配置或控制器为空。");
                return;
            }
            Bind(controller, config.startClickspineAnimationNames, config.onClickspineAnimationNames);
        }

        //登记动画和音频路径，不在界面初始化阶段解码全部语音。
        private static void Bind(aronaSpineUIController controller, List<SpineAnimation> start, List<SpineAnimation> clicks)
        {
            BADialogueAudioPlayer audio = controller.gameObject.AddComponent<BADialogueAudioPlayer>();
            audio.Initialize(controller);
            controller.start.Clear();
            controller.oneClick.Clear();
            AddAnimations(audio, start, controller.start);
            AddAnimations(audio, clicks, controller.oneClick);
        }

        //创建每个控制器独立的动画数据，并把原始 Def 中的语音路径交给加载器。
        private static void AddAnimations(BADialogueAudioPlayer audio, List<SpineAnimation> source, List<aronaAnimation> target)
        {
            foreach (SpineAnimation config in source)
            {
                var animation = new aronaAnimation
                {
                    spineAnimationName = config.spineAnimationName,
                    Mouth_Tex = config.Mouth_Tex,
                    defMouse_Tex = config.defMouse_Tex,
                    isBlink = false,
                    text = config.text
                };
                audio.Register(animation, Path.Combine(UiMapData.modRootPath, config.aronaAudioClipPath));
                target.Add(animation);
            }
        }
    }
}
