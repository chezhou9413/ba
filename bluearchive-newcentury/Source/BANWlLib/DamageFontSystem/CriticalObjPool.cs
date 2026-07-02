using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    // 飘字对象池，负责复用暴击和治疗飘字对象。
    public static class CriticalObjPool
    {
        public static Queue<GameObject> Criticalpool = new Queue<GameObject>();
        private static bool missingPrefabLogged;

        // 获取飘字对象，负责优先复用对象池中的实例。
        public static GameObject getCriticalObj()
        {
            if (Criticalpool.Count > 0)
            {
                return Criticalpool.Dequeue();
            }

            GameObject gameObject = GameObject.Instantiate(FontDataBase.CriticalFont);
            gameObject.AddComponent<DamageFloatText>();
            return gameObject;
        }

        // 回收飘字对象，负责避免同一个对象重复入池。
        public static void ReleaseCriticalPool(GameObject game)
        {
            if (!Criticalpool.Contains(game))
            {
                game.SetActive(false);
                Criticalpool.Enqueue(game);
            }
        }

        // 显示暴击飘字，负责使用默认暴击颜色和文本。
        public static void showCriticalShow(float amount, Pawn pawn)
        {
            ShowFloatText((int)amount, pawn, Color.white, null);
        }

        // 显示治疗飘字，负责使用治疗颜色并支持暴疗文本。
        public static void showHealShow(float amount, Pawn pawn, bool isCrit = false)
        {
            string prefix = isCrit ? "暴疗 " : "+";
            ShowFloatText((int)amount, pawn, isCrit ? Color.yellow : Color.green, prefix);
        }

        // 统一显示飘字，负责设置世界坐标、文本样式和对象激活。
        private static void ShowFloatText(int amount, Pawn pawn, Color color, string prefix)
        {
            if (pawn == null || FontDataBase.CriticalFont == null || FontDataBase.Canvas == null)
            {
                if (!missingPrefabLogged)
                {
                    missingPrefabLogged = true;
                    Log.Error("BA伤害飘字资源未初始化，无法显示暴击或治疗飘字。");
                }

                return;
            }

            GameObject gameObject = getCriticalObj();
            if (FontDataBase.Canvas.GetComponent<Canvas>().worldCamera != Find.Camera)
            {
                FontDataBase.Canvas.GetComponent<Canvas>().worldCamera = Find.Camera;
                FontDataBase.Canvas.GetComponent<Canvas>().sortingOrder = 1000;
            }

            gameObject.transform.SetParent(FontDataBase.Canvas.transform, false);
            Vector3 worldPos = pawn.DrawPos;
            worldPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.03f;
            gameObject.transform.position = worldPos;
            gameObject.transform.LookAt(gameObject.transform.position + Find.Camera.transform.rotation * Vector3.forward, Find.Camera.transform.rotation * Vector3.up);
            gameObject.transform.localScale = Vector3.one * 0.01f;

            DamageFloatText floatText = gameObject.GetComponent<DamageFloatText>();
            if (floatText != null)
            {
                floatText.ConfigureStyle(color, (prefix ?? string.Empty) + amount);
            }
            else
            {
                gameObject.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = (prefix ?? string.Empty) + amount;
            }

            gameObject.SetActive(true);
        }
    }
}
