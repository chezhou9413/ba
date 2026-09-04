using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib.CostSystem
{
    //COST数字视图负责使用数字图集排版整数、小数和负号并播放数字弹跳。
    internal sealed class CostDigitView
    {
        private const float DigitHeight = 82f;
        private const float DigitSpacing = -4f;
        private const float BounceGrowDuration = 0.1f;
        private const float BounceReturnDuration = 0.18f;
        private static readonly Color DebtColor = new Color(1f, 0.38f, 0.06f, 1f);

        private readonly Transform root;
        private readonly Vector3 baseScale;
        private readonly Image sign;
        private readonly Image tens;
        private readonly Image units;
        private readonly Image decimalPoint;
        private readonly Image tenths;
        private readonly Sprite[] digitSprites = new Sprite[10];

        private float bounceAge = -1f;

        //从预制体固定节点和AssetBundle数字图集中构造数字显示。
        public CostDigitView(Transform root, AssetBundle bundle)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
            baseScale = root.localScale;
            sign = RequireImage(root, "MinusSign");
            tens = RequireImage(root, "TensDigit");
            units = RequireImage(root, "UnitsDigit");
            decimalPoint = RequireImage(root, "DecimalPoint");
            tenths = RequireImage(root, "TenthsDigit");
            LoadDigits(bundle);
        }

        //按整数十分位显示最多20.0且最低-5.0的COST。
        public void SetValue(int valueTenths)
        {
            bool negative = valueTenths < 0;
            int absoluteTenths = Mathf.Abs(valueTenths);
            int whole = absoluteTenths / 10;
            int fraction = absoluteTenths % 10;
            bool showTens = whole >= 10;
            bool showFraction = fraction != 0;
            Color color = negative ? DebtColor : Color.white;

            var activeImages = new List<Image>();
            var widths = new List<float>();

            ConfigureSolid(sign, negative, 24f, 7f, color, activeImages, widths);
            ConfigureDigit(tens, showTens, whole / 10, color, activeImages, widths);
            ConfigureDigit(units, true, whole % 10, color, activeImages, widths);
            ConfigureSolid(decimalPoint, showFraction, 9f, 9f, color, activeImages, widths);
            ConfigureDigit(tenths, showFraction, fraction, color, activeImages, widths);
            Arrange(activeImages, widths);
        }

        //从第一帧重新开始一次数字放大回弹。
        public void PlayBounce()
        {
            bounceAge = 0f;
        }

        //按未缩放帧时间更新一次1到1.18再回到1的弹跳。
        public void UpdateBounce(float deltaTime)
        {
            if (bounceAge < 0f)
            {
                return;
            }

            bounceAge += deltaTime;
            float factor;
            if (bounceAge <= BounceGrowDuration)
            {
                factor = Mathf.Lerp(1f, 1.18f, Mathf.SmoothStep(0f, 1f, bounceAge / BounceGrowDuration));
            }
            else
            {
                float returnAge = bounceAge - BounceGrowDuration;
                factor = Mathf.Lerp(1.18f, 1f, Mathf.SmoothStep(0f, 1f, returnAge / BounceReturnDuration));
                if (returnAge >= BounceReturnDuration)
                {
                    factor = 1f;
                    bounceAge = -1f;
                }
            }

            root.localScale = baseScale * factor;
        }

        //从AssetBundle载入十个命名为CostDigit_0到CostDigit_9的Sprite。
        private void LoadDigits(AssetBundle bundle)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            Sprite[] sprites = bundle.LoadAssetWithSubAssets<Sprite>(
                "Assets/Scenes/Resources/Cost/CostDigitsAtlas.png");
            for (int index = 0; index < sprites.Length; index++)
            {
                Sprite sprite = sprites[index];
                const string prefix = "CostDigit_";
                if (sprite.name.StartsWith(prefix) && int.TryParse(sprite.name.Substring(prefix.Length), out int digit))
                {
                    if (digit >= 0 && digit <= 9)
                    {
                        digitSprites[digit] = sprite;
                    }
                }
            }

            for (int digit = 0; digit <= 9; digit++)
            {
                if (digitSprites[digit] == null)
                {
                    throw new InvalidOperationException("COST数字图集缺少CostDigit_" + digit + "。" );
                }
            }
        }

        //取得预制体中必须存在的Image节点。
        private static Image RequireImage(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            Image image = child?.GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException("CostUI缺少数字节点：" + childName);
            }

            image.raycastTarget = false;
            return image;
        }

        //配置一个数字Sprite并记录其等比显示宽度。
        private void ConfigureDigit(
            Image image,
            bool active,
            int digit,
            Color color,
            List<Image> activeImages,
            List<float> widths)
        {
            image.gameObject.SetActive(active);
            if (!active)
            {
                return;
            }

            Sprite sprite = digitSprites[digit];
            float width = DigitHeight * sprite.rect.width / sprite.rect.height;
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            image.rectTransform.sizeDelta = new Vector2(width, DigitHeight);
            activeImages.Add(image);
            widths.Add(width);
        }

        //配置负号或小数点的纯色Image并记录固定宽度。
        private static void ConfigureSolid(
            Image image,
            bool active,
            float width,
            float height,
            Color color,
            List<Image> activeImages,
            List<float> widths)
        {
            image.gameObject.SetActive(active);
            if (!active)
            {
                return;
            }

            image.sprite = null;
            image.color = color;
            image.preserveAspect = false;
            image.rectTransform.sizeDelta = new Vector2(width, height);
            activeImages.Add(image);
            widths.Add(width);
        }

        //把当前启用的符号与数字作为一行居中排列。
        private static void Arrange(List<Image> images, List<float> widths)
        {
            float totalWidth = 0f;
            for (int index = 0; index < widths.Count; index++)
            {
                totalWidth += widths[index];
            }

            totalWidth += DigitSpacing * Mathf.Max(0, widths.Count - 1);
            float cursor = -totalWidth * 0.5f;
            for (int index = 0; index < images.Count; index++)
            {
                float width = widths[index];
                RectTransform rect = images[index].rectTransform;
                rect.anchoredPosition = new Vector2(cursor + width * 0.5f, 0f);
                cursor += width + DigitSpacing;
            }
        }
    }
}
