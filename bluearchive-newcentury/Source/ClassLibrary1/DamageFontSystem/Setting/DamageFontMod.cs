using BANWlLib.DamageFontSystem.Comp;
using newpro;
using UnityEngine;
using Verse;

namespace BANWlLib.DamageFontSystem.Setting
{
    public class DamageFontMod : Mod
    {
        public static DamageFontSettings settings;

        public DamageFontMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<DamageFontSettings>();
        }

        // ✅ 显示设置界面
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.CheckboxLabeled(
                "启用暴击伤害飘字系统",
                ref settings.enableDamageFloat,
                "关闭此选项将禁用暴击伤害飘字显示"
            );
            list.CheckboxLabeled(
    "启用爆发型粒子效果",
    ref settings.enableBurstParticle,
    "如果你的显卡为 AMD 或 Intel 核显，关闭此选项可能改善卡顿"
);
            if (list.ButtonText("重置UI位置", "点击此按钮把UI入口恢复默认设置"))
            {
                if (Current.ProgramState != ProgramState.Playing || Current.Game == null)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("请进入游戏地图后再使用此功能"));
                    return;
                }
                DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
                if (comp == null)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("游戏组件未初始化，请进入地图后再试。"));
                    return;
                }
                if (UiMapData.openUIBUTT == null)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("UI 入口未初始化，请打开地图界面后再试。"));
                    return;
                }
                RectTransform rect = UiMapData.openUIBUTT.GetComponent<RectTransform>();
                if (rect == null)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("UI 对象缺少 RectTransform 组件。"));
                    return;
                }
                settings.dfPosX = 780.6f;
                settings.dfPosY = -477.1f;
                rect.anchoredPosition = new Vector2(settings.dfPosX, settings.dfPosY);
                comp.savePosX = settings.dfPosX;
                comp.savePosY = settings.dfPosY;

                Find.WindowStack.Add(new Dialog_MessageBox("UI 位置已重置为默认"));
            }
            list.End();
            base.DoSettingsWindowContents(inRect);
        }

        // ✅ 设置界面标题
        public override string SettingsCategory() => "BlueArchive-NewWorld";
    }
}
