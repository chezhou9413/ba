using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib.DamageFontSystem
{
    // 飘字动画组件，负责控制飘字的移动、停留和淡出。
    public class DamageFloatText : MonoBehaviour
    {
        public float moveDistance = 1.1f;
        public float moveTime = 0.3f;
        public float stayTime = 0.5f;
        public float fadeTime = 0.3f;
        private const float TextWidthPadding = 20f;

        private UnityEngine.UI.Text text;
        private Image image;
        private RectTransform textRect;
        private RectTransform selfRect;
        private Vector3 startPos;
        private Vector3 targetPos;
        private Coroutine routine;
        private Color baseTextColor;
        private Color imageColor;
        private Color configuredColor;
        private string configuredText;
        private bool hasConfiguredStyle;

        // 初始化组件引用，负责缓存文字和背景图片组件。
        void Awake()
        {
            text = transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
            image = GetComponent<Image>();
            selfRect = GetComponent<RectTransform>();
            if (text != null)
            {
                textRect = text.GetComponent<RectTransform>();
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                baseTextColor = text.color;
            }
            if (image != null)
            {
                imageColor = image.color;
            }
        }

        // 激活时重置动画状态，负责按当前样式开始播放飘字。
        void OnEnable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (text != null)
            {
                Color activeColor = hasConfiguredStyle ? configuredColor : baseTextColor;
                activeColor.a = 1f;
                text.color = activeColor;
                if (hasConfiguredStyle)
                {
                    text.text = configuredText;
                }
            }

            if (image != null)
            {
                imageColor.a = 1f;
                image.color = imageColor;
            }

            startPos = transform.position;
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            float angle = UnityEngine.Random.Range(35f, 90f) * Mathf.Deg2Rad;
            Vector3 dir = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
            targetPos = startPos + dir * moveDistance;
            routine = StartCoroutine(PlayRoutine());
        }

        // 配置飘字样式，负责在激活前设置文字内容和颜色。
        public void ConfigureStyle(Color color, string textValue)
        {
            configuredColor = color;
            configuredText = textValue;
            hasConfiguredStyle = true;
            if (text != null)
            {
                configuredColor.a = 1f;
                text.color = configuredColor;
                text.text = configuredText;
                ApplyTextWidth();
            }
        }

        // 按当前文字内容扩宽飘字区域，负责让带加号的治疗数字不会被固定宽度截断。
        private void ApplyTextWidth()
        {
            if (text == null || textRect == null)
            {
                return;
            }

            float preferredWidth = text.preferredWidth + TextWidthPadding;
            if (preferredWidth <= textRect.sizeDelta.x)
            {
                return;
            }

            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            if (selfRect != null && preferredWidth > selfRect.sizeDelta.x)
            {
                selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            }
        }

        // 播放飘字动画，负责执行上升、停留和淡出阶段。
        private IEnumerator PlayRoutine()
        {
            float timer = 0f;
            while (timer < moveTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, timer / moveTime);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            yield return new WaitForSeconds(stayTime);

            timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
                if (text != null)
                {
                    Color fadeColor = text.color;
                    fadeColor.a = alpha;
                    text.color = fadeColor;
                }
                if (image != null)
                {
                    Color fadeImageColor = image.color;
                    fadeImageColor.a = alpha;
                    image.color = fadeImageColor;
                }

                yield return null;
            }

            if (CriticalObjPool.Criticalpool != null)
            {
                gameObject.SetActive(false);
                CriticalObjPool.ReleaseCriticalPool(gameObject);
            }

            hasConfiguredStyle = false;
            routine = null;
        }
    }
}
