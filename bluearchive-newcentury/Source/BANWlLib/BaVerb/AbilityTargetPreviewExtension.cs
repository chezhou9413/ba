using Verse;

namespace BANWlLib.BaVerb
{
    // 技能目标预览配置，负责让 AbilityDef 直接声明施法时显示的范围。
    public class AbilityTargetPreviewExtension : DefModExtension
    {
        // 预览形状，决定使用圆形、直线、扇形或矩形计算格子。
        public AbilityTargetPreviewShape shape = AbilityTargetPreviewShape.Circle;

        // 最大预览距离，小于等于 0 时使用技能当前有效射程。
        public float range = -1f;

        // 圆形场地半径，小于等于 0 时允许从场地控制器配置读取。
        public float radius = -1f;

        // 直线或矩形宽度，小于等于 0 时允许从穿透投射物配置读取。
        public float width = -1f;

        // 直线长度，小于等于 0 时使用 range 或技能当前有效射程。
        public float length = -1f;

        // 扇形角度。
        public float fanArc = 30f;

        // 是否绘制施法者最大射程圈。
        public bool drawCasterRange = true;

        // 是否绘制鼠标目标格高亮。
        public bool drawTargetHighlight = true;

        // 是否从脱手场地 ThingDef 的 BattleFieldControllerExtension 读取半径。
        public bool useBattleFieldRadius = false;

        // 是否从穿透投射物 ThingDef 的 PiercingProjectileExtension 读取宽度，直线长度默认使用施法射程。
        public bool usePiercingProjectileSize = false;

        // 是否在技能前摇期间强制绘制主武器朝目标瞄准。
        public bool drawPrimaryWeaponAim = false;
    }
}
