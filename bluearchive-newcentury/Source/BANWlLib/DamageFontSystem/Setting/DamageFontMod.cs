using BANWlLib.DamageFontSystem.Comp;
using BANWlLib.CostSystem;
using newpro;
using UnityEngine;
using Verse;

namespace BANWlLib.DamageFontSystem.Setting
{
    //伤害字体设置入口负责注册设置页，并在模组对象创建时安装全局 Harmony 补丁。
    public class DamageFontMod : Mod
    {
        public static DamageFontSettings settings;

        //构造函数负责读取设置并触发轻量补丁安装，避免依赖其他 Mod 入口的实例化顺序。
        public DamageFontMod(ModContentPack content) : base(content)
        {
            BANWlLib.ModMain.ApplyHarmonyPatches();
            settings = GetSettings<DamageFontSettings>();
        }

        //绘制伤害字体、入口位置和COST轮盘位置设置。
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
            if (list.ButtonText("重置COST轮盘位置", "点击此按钮把COST轮盘恢复到什亭之匣入口上方"))
            {
                string reason;
                if (!CostUiDragController.TryResetSavedPosition(out reason))
                {
                    Find.WindowStack.Add(new Dialog_MessageBox(reason));
                }
                else
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("COST轮盘位置已重置为默认"));
                }
            }
            list.End();
            base.DoSettingsWindowContents(inRect);
        }

        //返回模组设置页标题。
        public override string SettingsCategory() => "BlueArchive-NewWorld";
    }
}
