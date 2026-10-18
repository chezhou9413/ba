#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual;
#if RW_1_7
using RimUnity.Tweening;
#else
using DG.Tweening;
#endif
using System;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif

namespace BANWlLib.uicreater.tool
{
    public static class BamessageUI
    {
        public static void ShowBaMessageUI(string title,string des,string Buttomtext)
        {
            GameObject a = Engine.Object.Instantiate(ManualMapData.messageUI);
#if RW_1_7
            BANWlLib.mainUI.Initialization.BAFullscreenLayout.ConfigureDialog(a);
#endif
            messageUILord messageUILord = a.AddComponent<messageUILord>();
            messageUILord.title = title;
            messageUILord.des = des;
            messageUILord.Buttomtext = Buttomtext;
            RectTransform rect = a.transform.Find("UIback").GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(0, -1000f);
            rect.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutCubic);
        }

        //显示确认／取消窗口，确认时调用 onQuekAction。
        public static void ShowBaMessageUIQuek(string title, string des, string quekText, string closeText = "关闭", Action onQuekAction = null)
        {
            GameObject uiGo = Engine.Object.Instantiate(ManualMapData.messageUIQuek);
#if RW_1_7
            BANWlLib.mainUI.Initialization.BAFullscreenLayout.ConfigureDialog(uiGo);
#endif
            messageUILordQuek script = uiGo.GetComponent<messageUILordQuek>() ?? uiGo.AddComponent<messageUILordQuek>();
            script.title = title;
            script.des = des;
            script.QuekButtomtext = quekText;
            script.CloseButtomtext = closeText;
            script.onQuek = onQuekAction;
            RectTransform rect = uiGo.transform.Find("UIback").GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -1000f);
            rect.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutCubic);
        }
    }
}
