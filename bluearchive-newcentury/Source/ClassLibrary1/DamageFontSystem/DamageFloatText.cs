using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.DamageFontSystem
{
    public class DamageFloatText : MonoBehaviour
    {
        [Header("动画参数")]
        public float moveDistance = 1.1f;   // 飘字移动距离
        public float moveTime = 0.3f;       // 移动时长
        public float stayTime = 0.5f;       // 停留时间
        public float fadeTime = 0.3f;       // 淡出时间

        private UnityEngine.UI.Text text;
        private Image image;

        private Vector3 startPos;
        private Vector3 targetPos;
        private Coroutine routine;

        private Color textColor;
        private Color imageColor;

        void Awake()
        {
            text = this.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
            image = GetComponent<Image>();

            if (text != null) textColor = text.color;
            if (image != null) imageColor = image.color;
        }

        void OnEnable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
            if (text != null)
            {
                textColor.a = 1f;
                text.color = textColor; // 确保一开始不透明
            }
            if (image != null)
            {
                imageColor.a = 1f;
                image.color = imageColor;
            }

            startPos = transform.position;
            Vector3 right = transform.right;  // Canvas的右边方向
            Vector3 up = transform.up;        // Canvas的上边方向
            float angle = UnityEngine.Random.Range(35f, 90f) * Mathf.Deg2Rad;
            Vector3 dir = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
            targetPos = startPos + dir * moveDistance;
            routine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            float timer = 0f;

            // ====== 第一阶段：上升移动 ======
            while (timer < moveTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, timer / moveTime);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            // ====== 第二阶段：停留 ======
            yield return new WaitForSeconds(stayTime);

            // ====== 第三阶段：淡出 ======
            timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float t = timer / fadeTime;
                float alpha = Mathf.Lerp(1f, 0f, t);

                if (text != null)
                {
                    textColor.a = alpha;
                    text.color = textColor;  // ✅ 一定要重新赋值
                }
                if (image != null)
                {
                    imageColor.a = alpha;
                    image.color = imageColor;  // ✅ 同上
                }

                yield return null;
            }

            // 回收
            if (CriticalObjPool.Criticalpool != null)
            {
                this.gameObject.SetActive(false);
                CriticalObjPool.ReleaseCriticalPool(gameObject);
            }

            routine = null;
        }
    }
}
