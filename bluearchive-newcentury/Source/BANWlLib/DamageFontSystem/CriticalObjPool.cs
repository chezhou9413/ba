using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using System.Collections.Generic;
using RimUnity;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    //飘字对象池，复用暴击和治疗飘字对象。
    public static class CriticalObjPool
    {
        public static Queue<GameObject> Criticalpool = new Queue<GameObject>();
        private static bool missingPrefabLogged;

        //获取飘字对象，优先复用对象池中的实例。
        public static GameObject getCriticalObj()
        {
            if (Criticalpool.Count > 0)
            {
                return Criticalpool.Dequeue();
            }

            GameObject gameObject = GameObject.Instantiate(FontDataBase.CriticalFont);
            //模板默认激活，先关闭再添加动画，等待位置与样式配置完成后播放。
            gameObject.SetActive(false);
            gameObject.AddComponent<DamageFloatText>();
            return gameObject;
        }

        //回收飘字对象，避免同一个对象重复入池。
        public static void ReleaseCriticalPool(GameObject game)
        {
            if (!Criticalpool.Contains(game))
            {
                game.SetActive(false);
                Criticalpool.Enqueue(game);
            }
        }

        //显示暴击飘字，使用默认暴击颜色和文本。
        public static void showCriticalShow(float amount, Pawn pawn)
        {
            ShowFloatText((int)amount, pawn, Color.white, null);
        }

        //显示治疗飘字，使用治疗颜色并支持暴疗文本。
        public static void showHealShow(float amount, Pawn pawn, bool isCrit = false)
        {
            string prefix = isCrit ? "暴疗 " : "+";
            ShowFloatText((int)amount, pawn, isCrit ? Color.yellow : Color.green, prefix);
        }

        //统一显示飘字，设置世界坐标、文本样式和对象激活。
        private static void ShowFloatText(int amount, Pawn pawn, Color color, string prefix)
        {
            //地图生成也会结算伤害；先拦截后台调用，再读取画面状态和访问飘字对象池。
            if (!UnityData.IsInMainThread || Current.ProgramState != ProgramState.Playing
                || pawn == null || !pawn.Spawned || pawn.Map != Find.CurrentMap)
            {
                return;
            }

            InitializeFontSystem.EnsureCurrentGame();
            if (FontDataBase.CriticalFont == null || FontDataBase.Canvas == null)
            {
                if (!missingPrefabLogged)
                {
                    missingPrefabLogged = true;
                    Log.Error("BA伤害飘字资源未初始化，无法显示暴击或治疗飘字。");
                }

                return;
            }

            GameObject gameObject = getCriticalObj();
            var camera = RimUnity.Camera.main;
            FontDataBase.Canvas.GetComponent<Canvas>().worldCamera = camera;
            gameObject.transform.SetParent(FontDataBase.Canvas.transform, false);
            Vector3 worldPos = new(pawn.DrawPos.x, AltitudeLayer.MetaOverlays.AltitudeFor() + 0.03f, pawn.DrawPos.z);
            gameObject.transform.position = worldPos;
            gameObject.transform.rotation = camera.transform.rotation;
            gameObject.transform.localScale = Vector3.one * 0.01f;

            DamageFloatText floatText = gameObject.GetComponent<DamageFloatText>();
            if (floatText != null)
            {
                floatText.ConfigureStyle(color, (prefix ?? string.Empty) + amount);
            }
            else
            {
                gameObject.transform.Find("Text").GetComponent<Engine.UI.Text>().text = (prefix ?? string.Empty) + amount;
            }

            gameObject.SetActive(true);
        }
    }
}
