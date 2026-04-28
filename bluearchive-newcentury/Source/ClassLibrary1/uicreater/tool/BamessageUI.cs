using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual;
using DG.Tweening;
using System;
using UnityEngine;

namespace BANWlLib.uicreater.tool
{
    public static class BamessageUI
    {
        public static void ShowBaMessageUI(string title,string des,string Buttomtext)
        {
            GameObject a = UnityEngine.Object.Instantiate(ManualMapData.messageUI);
            messageUILord messageUILord = a.AddComponent<messageUILord>();
            messageUILord.title = title;
            messageUILord.des = des;
            messageUILord.Buttomtext = Buttomtext;
            RectTransform rect = a.transform.Find("UIback").GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(0, -1000f);
            rect.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// 显示一个带有确认和取消按钮的消息窗口
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="des">描述内容</param>
        /// <param name="quekText">确认按钮文字</param>
        /// <param name="closeText">关闭按钮文字（默认为“关闭”）</param>
        /// <param name="onQuekAction">点击“确认”时执行的函数</param>
        public static void ShowBaMessageUIQuek(string title, string des, string quekText, string closeText = "关闭", Action onQuekAction = null)
        {
            GameObject uiGo = UnityEngine.Object.Instantiate(ManualMapData.messageUIQuek);
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
