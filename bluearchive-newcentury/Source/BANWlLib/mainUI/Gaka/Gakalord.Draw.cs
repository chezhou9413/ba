#if RW_1_7
using RimUnity;
#else
using UnityEngine;
#endif
using System;
using BANWlLib.BaDef;
using System.Collections.Generic;
using System.Linq;
using BANWlLib.mainUI.Gaka.MonoComp;
using BANWlLib.uicreater.tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using Verse;

namespace BANWlLib.mainUI.Gaka
{
    //统一首抽与再次抽取的资源准备、扣费、结算和展示交接。
    public static partial class Gakalord
    {
        //入口只提示本次费用，确认之后重新检查余额。
        private static void ConfirmDraw(bool tenGacha)
        {
            LoopBGMManager.playEffAudio("鼠标点击音效");
            int cost = tenGacha ? 1200 : 120;
            bool free = Prefs.DevMode && DebugSettings.godMode;
            int current = ItemUtility.GetTotalItemCount("QinghuiStone");
            if (!free && current < cost)
            {
                BamessageUI.ShowBaMessageUI("提示", $"青辉石不足！\n需要：{cost}\n当前：{current}", "确认");
                return;
            }
            string kind = tenGacha ? "十连抽" : "单抽";
            string message = free ? $"【上帝模式】是否进行一次{kind}？"
                : $"是否花费{cost}青辉石进行一次{kind}？\n当前持有：{current}";
            BamessageUI.ShowBaMessageUIQuek("提示", message, "确认", "取消", () => TryStartDraw(tenGacha));
        }

        //所有资源与控件就绪后才扣费，失败保留原结果页或恢复招募页面。
        public static bool TryStartDraw(bool tenGacha)
        {
            MonoComp_GakaAnimationPofab previous = GakaMapData.activeAnimation;
            if (previous != null && !previous.IsShowingResults) return false;
            MonoComp_GakaAnimationPofab prepared = null;
            bool paymentAttempted = false;
            bool settlementStarted = false;
            bool settled = false;
            try
            {
                int cost = tenGacha ? 1200 : 120;
                bool free = Prefs.DevMode && DebugSettings.godMode;
                if (!free && ItemUtility.GetTotalItemCount("QinghuiStone") < cost)
                {
                    BamessageUI.ShowBaMessageUI("提示", $"青辉石不足！本次需要 {cost}。", "确认");
                    return false;
                }
                if (GakaMapData.selectGachaDef == null)
                    throw new InvalidOperationException("当前没有可用卡池。");

                prepared = PrepareAnimation(tenGacha);
                List<BaStudentDef> students = tenGacha ? DrawTen() : new List<BaStudentDef> { DrawOne() };
                if (students.Count != (tenGacha ? 10 : 1) || students.Any(student => student == null || student.baStudentData == null))
                    throw new InvalidOperationException("卡池返回了缺失的学生或学生数据，请检查卡池定义。");

                if (!free)
                {
                    paymentAttempted = true;
                    if (!ItemUtility.TryRemoveItem("QinghuiStone", cost))
                    {
                        BamessageUI.ShowBaMessageUI("提示", "青辉石不足，未执行抽卡。", "确认");
                        return false;
                    }
                }
                settlementStarted = true;
                CloseAnimation(previous);
                if (tenGacha) ExtGakaData(students);
                else ExtGakaData(students[0]);
                GakaMapData.gamecomp_GakaAction.updataGacaPoit(tenGacha ? 10 : 1);
                settled = true;

                GakaMapData.activeAnimation = prepared;
                UiMapData.isLocKBack = true;
                prepared.gameObject.SetActive(true);
                prepared = null;
                return true;
            }
            catch (Exception error)
            {
                if (settlementStarted)
                {
                    GakaMapData.activeAnimation = null;
                    UiMapData.isLocKBack = false;
                }
                Log.Error($"[抽卡UI] 抽卡失败（扣费开始={paymentAttempted}，结算开始={settlementStarted}，结算完成={settled}）：\n{error}");
                string message = settled ? "学生、奖励和招募点数已结算，但抽卡展示失败。请返回学生列表查看结果。"
                    : settlementStarted ? "抽卡结算中断，可能已有部分结果。请保留存档并提供 Player.log，勿重复确认。"
                    : paymentAttempted ? "青辉石扣除过程未完成。请检查余额并提供 Player.log。"
                    : "抽卡资源或卡池准备失败，尚未扣除青辉石。请检查前置模组版本，并提供 Player.log。";
                BamessageUI.ShowBaMessageUI("抽卡失败", message, "确认");
                return false;
            }
            finally
            {
                if (prepared != null) CloseAnimation(prepared);
            }
        }

        //隐藏旧实例后再延迟销毁，避免同一帧继续截获点击。
        internal static void CloseAnimation(MonoComp_GakaAnimationPofab animation)
        {
            if (animation == null) return;
            if (GakaMapData.activeAnimation == animation)
            {
                GakaMapData.activeAnimation = null;
                UiMapData.isLocKBack = false;
            }
            animation.gameObject.SetActive(false);
            GameObject.Destroy(animation.gameObject);
        }

        //播放启动失败时保留已经结算的结果，并恢复返回和后续操作。
        internal static void AbortPresentation(MonoComp_GakaAnimationPofab animation, Exception error)
        {
            CloseAnimation(animation);
            Log.Error($"[抽卡UI] 结果已结算，过场启动失败：\n{error}");
            BamessageUI.ShowBaMessageUI("抽卡展示失败", "学生、奖励和招募点数已结算，请返回学生列表查看结果。详细原因见 Player.log。", "确认");
        }
    }
}
