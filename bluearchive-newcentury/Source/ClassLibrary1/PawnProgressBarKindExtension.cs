using System.Collections.Generic;
using Verse;

namespace BANWlLib
{
    //PawnKind 星级配置，负责为每个学生 Kind 单独提供星级绘制参数。
    public class PawnProgressBarKindExtension : DefModExtension
    {
        //星星的间隔比例。
        public float starInterval = 1f;

        //是否显示属性值进度条。
        public bool showProgressBar = true;

        //星星绘制的进度比例。
        public List<float> starProgressRatios = new List<float> { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

        //星星的尺寸。
        public float starSize = 20f;

        //星星的贴图路径。
        public string starTexturePath = "UI/StarIcon";

        //起始星星经验，-1 表示使用种族组件里的通用初始经验。
        public int initialExperience = -1;

        //升星时播放的单个特效。
        public string starUpEffect = "";

        //升星时随机播放的特效列表。
        public List<string> starUpEffects = new List<string>();

        //升星时随机播放的语音。
        public List<string> starUpSounds = new List<string>();

        //升级时播放的单个特效。
        public string levelUpEffect = "";

        //升级时随机播放的特效列表。
        public List<string> levelUpEffects = new List<string>();

        //升级时随机播放的语音。
        public List<string> levelUpSounds = new List<string>();
    }
}
