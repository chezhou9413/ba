using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    public static class CriticalObjPool
    {
        public static Queue<GameObject> Criticalpool = new Queue<GameObject>();

        public static GameObject getCriticalObj()
        {
            if (Criticalpool.Count > 0)
            {
                GameObject gameObject = Criticalpool.Dequeue();
                return gameObject;
            }
            else
            {
                GameObject gameObject = GameObject.Instantiate(FontDataBase.CriticalFont);
                gameObject.AddComponent<DamageFloatText>();
                return gameObject;
            }
        }

        public static void ReleaseCriticalPool(GameObject game)
        {
            if (!Criticalpool.Contains(game))
            {
                game.SetActive(false);
                Criticalpool.Enqueue(game);
            }
        }

        public static void showCriticalShow(float a, Pawn pawn)
        {
            int aInt = (int)(a);
            GameObject gameObject = CriticalObjPool.getCriticalObj();
            if (FontDataBase.Canvas.GetComponent<Canvas>().worldCamera != Find.Camera)
            {
                FontDataBase.Canvas.GetComponent<Canvas>().worldCamera = Find.Camera;
                FontDataBase.Canvas.GetComponent<Canvas>().sortingOrder = 1000;
            }
            gameObject.transform.SetParent(FontDataBase.Canvas.transform, false);
            Vector3 worldPos = pawn.DrawPos;
            worldPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.03f; // 比特效再高一点
            gameObject.transform.position = worldPos;
            gameObject.transform.LookAt(gameObject.transform.position + Find.Camera.transform.rotation * Vector3.forward, Find.Camera.transform.rotation * Vector3.up);
            gameObject.transform.localScale = Vector3.one * 0.01f;
            gameObject.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = aInt.ToString();
            gameObject.SetActive(true);
        }
    }
}
