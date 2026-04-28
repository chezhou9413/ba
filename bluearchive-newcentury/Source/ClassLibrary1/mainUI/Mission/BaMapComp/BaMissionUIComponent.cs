using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BANWlLib.MissionRunTime
{
    // 这是一个依附于地图的组件，专门用来画图
    public class BaMissionUIComponent : MapComponent
    {
        public BaMissionUIComponent(Map map) : base(map) { }

        public override void MapComponentOnGUI()
        {
            var manager = Find.World.GetComponent<BaMissionManager>();
            if (manager == null || manager.activeMissions.NullOrEmpty()) return;

            // UI 基础参数
            float width = 200f;                 // 整体宽度
            float rightMargin = 10f;            // 距离屏幕右边距
            float startX = UI.screenWidth - width - rightMargin;
            float curY = 200f;                  // 起始高度

            float lineHeight = 24f;             // 单行高度
            float itemSpacing = 10f;            // 任务与任务之间的间距

            // 使用倒序循环，安全删除
            for (int i = manager.activeMissions.Count - 1; i >= 0; i--)
            {
                var mission = manager.activeMissions[i];

                if (mission.state == MissionState.Active)
                {
                    // --- 1. 数据准备 ---
                    int secondsLeft = mission.GetRemainingSeconds();
                    string timeStr = $"{secondsLeft / 60:D2}:{secondsLeft % 60:D2}";
                    string label = $"任务倒计时: {timeStr}";

                    Color textColor = Color.white;
                    if (secondsLeft < 30) textColor = Color.red;

                    // --- 2. 第一行：绘制倒计时文本 ---
                    Rect textRect = new Rect(startX, curY, width, lineHeight);

                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleRight; // 文本靠右对齐
                    GUI.color = textColor;

                    Widgets.Label(textRect, label);

                    GUI.color = Color.white; // 颜色还原

                    // --- 3. 换行，准备画按钮 ---
                    curY += lineHeight; // Y轴向下移动一行

                    // --- 4. 第二行：绘制按钮 ---
                    Rect btnRect = new Rect(startX, curY, width, lineHeight);

                    Text.Anchor = TextAnchor.MiddleCenter; // 按钮文字居中

                    // 这里我把按钮改稍微小一点点或者颜色变一下也可以，但保持默认最简单
                    if (Widgets.ButtonText(btnRect, "强制结束任务"))
                    {
                        mission.colseMission();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }

                    // --- 5. 准备下一个任务的起始位置 ---
                    // 当前 Y + 按钮高度 + 额外的空隙
                    curY += lineHeight + itemSpacing;
                }
            }

            // 恢复全局设置
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}