namespace BANWlLib.BaVerb
{
    // 技能预览运行参数，负责把 Def 配置和自动推导结果整理成绘制工具可直接使用的数据。
    internal class BattleTargetPreviewData
    {
        public AbilityTargetPreviewShape shape;
        public float range;
        public float radius;
        public float width;
        public float length;
        public float fanArc;
        public bool drawCasterRange = true;
        public bool drawTargetHighlight = true;
    }
}
