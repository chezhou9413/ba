using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib.CostSystem
{
    //COST环视图负责把0到10点进度分配给十个环形分段。
    internal sealed class CostRingView : IDisposable
    {
        private const float SegmentHandoff = 0.08f;
        private static readonly Color DebtColor = new Color(1f, 0.38f, 0.06f, 1f);

        private readonly GameObject root;
        private readonly Image background;
        private readonly List<CostRingSegmentView> segments = new List<CostRingSegmentView>();

        //从固定命名的环节点构造十段运行时视图。
        public CostRingView(Transform ringRoot, string segmentPrefix)
        {
            if (ringRoot == null)
            {
                throw new ArgumentNullException(nameof(ringRoot));
            }

            root = ringRoot.gameObject;
            background = ringRoot.GetComponent<Image>();
            if (background != null)
            {
                background.raycastTarget = false;
            }

            for (int index = 1; index <= 10; index++)
            {
                Transform segment = ringRoot.Find(segmentPrefix + index.ToString("00"));
                if (segment == null)
                {
                    throw new InvalidOperationException("CostUI缺少环形分段：" + segmentPrefix + index.ToString("00"));
                }

                segments.Add(new CostRingSegmentView(segment.GetComponent<Image>()));
            }
        }

        //控制整圈在当前10点模式中是否显示。
        public void SetActive(bool active)
        {
            if (!root.activeSelf)
            {
                root.SetActive(true);
            }

            if (background != null)
            {
                background.enabled = active;
            }

            for (int index = 0; index < segments.Count; index++)
            {
                segments[index].SetActive(active);
            }
        }

        //将费用分配到各段，并处理反向债务与端点交接。
        public void SetValue(float value, bool reverseOrder, bool debtMode)
        {
            float completedSegments = Mathf.Clamp(value, 0f, segments.Count);
            int activeSegment = Mathf.FloorToInt(completedSegments);
            float activeProgress = completedSegments - activeSegment;
            float handoff = Mathf.Clamp01(activeProgress / SegmentHandoff);
            float incomingGlow = Mathf.SmoothStep(0f, 1f, handoff);
            float outgoingGlow = 1f - incomingGlow;

            if (background != null)
            {
                background.color = debtMode ? DebtColor : Color.white;
            }

            for (int physicalIndex = 0; physicalIndex < segments.Count; physicalIndex++)
            {
                int logicalIndex = reverseOrder ? segments.Count - 1 - physicalIndex : physicalIndex;
                float segmentProgress = completedSegments - logicalIndex;
                float glow = CalculateGlow(logicalIndex, activeSegment, incomingGlow, outgoingGlow, completedSegments);
                segments[physicalIndex].SetVisual(
                    segmentProgress,
                    reverseOrder,
                    debtMode,
                    glow);
            }
        }

        //让指定逻辑序号范围内的新满格同时开始一秒白色完成光效。
        public void FlashCompletedRange(int firstIndex, int lastIndex)
        {
            int clampedFirst = Mathf.Clamp(firstIndex, 0, segments.Count - 1);
            int clampedLast = Mathf.Clamp(lastIndex, 0, segments.Count - 1);
            for (int index = clampedFirst; index <= clampedLast; index++)
            {
                segments[index].TriggerCompletionFlash();
            }
        }

        //释放十个分段各自持有的运行时材质实例。
        public void Dispose()
        {
            for (int index = 0; index < segments.Count; index++)
            {
                segments[index].Dispose();
            }
        }

        //计算换格期间旧端点淡出与新端点淡入的连续亮度。
        private static float CalculateGlow(
            int logicalIndex,
            int activeSegment,
            float incomingGlow,
            float outgoingGlow,
            float completedSegments)
        {
            if (completedSegments <= 0f)
            {
                return 0f;
            }

            if (activeSegment >= 10)
            {
                return logicalIndex == 9 ? 1f : 0f;
            }

            if (logicalIndex == activeSegment)
            {
                return incomingGlow;
            }

            return logicalIndex == activeSegment - 1 ? outgoingGlow : 0f;
        }
    }

    //COST环分段视图负责维护单段材质参数和一次满格白色闪光。
    internal sealed class CostRingSegmentView : IDisposable
    {
        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private static readonly int ReverseId = Shader.PropertyToID("_Reverse");
        private static readonly int GlowEnabledId = Shader.PropertyToID("_GlowEnabled");
        private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
        private static readonly int CompletionColorId = Shader.PropertyToID("_CompletionColor");
        private static readonly int CompletionStartTimeId = Shader.PropertyToID("_CompletionStartTime");
        private static readonly int FillColorId = Shader.PropertyToID("_FillColor");
        private static readonly int FillColorStrengthId = Shader.PropertyToID("_FillColorStrength");
        private static readonly int SpriteAspectId = Shader.PropertyToID("_SpriteAspect");
        private static readonly int UvRectId = Shader.PropertyToID("_UvRect");

        private static readonly Color DebtColor = new Color(2.2f, 0.48f, 0.04f, 1f);
        private static readonly Color DebtCompletionColor = new Color(2.2f, 1.05f, 0.35f, 1f);

        private readonly Image image;
        private readonly Material material;
        //为Image克隆独立材质并写入Sprite实际UV范围。
        public CostRingSegmentView(Image image)
        {
            if (image == null || image.material == null)
            {
                throw new InvalidOperationException("CostUI环形分段缺少Image或进度材质。");
            }

            this.image = image;
            image.raycastTarget = false;
            material = new Material(image.material)
            {
                name = image.material.name + " (Runtime " + image.gameObject.name + ")",
                hideFlags = HideFlags.HideAndDontSave
            };
            image.material = material;
            ApplySpriteData();
        }

        //同步进度、方向、债务配色和当前端点光效。
        public void SetVisual(float value, bool reverse, bool debtMode, float glow)
        {
            float clamped = Mathf.Clamp01(value);
            material.SetFloat(ProgressId, clamped);
            material.SetFloat(ReverseId, reverse ? 1f : 0f);
            material.SetFloat(GlowEnabledId, Mathf.Clamp01(glow));
            material.SetColor(FillColorId, debtMode ? DebtColor : Color.white);
            material.SetFloat(FillColorStrengthId, debtMode ? 1f : 0f);
            material.SetColor(GlowColorId, debtMode ? DebtColor : new Color(0.9f, 1.4f, 1.8f, 1f));
            material.SetColor(CompletionColorId, debtMode ? DebtCompletionColor : new Color(1.3f, 1.5f, 1.8f, 1f));
        }

        //控制该分段图片显示状态，同时保留中央数字所在的父节点。
        public void SetActive(bool active)
        {
            image.enabled = active;
        }

        //从当前时间开始播放该分段的一秒白色完成光效。
        public void TriggerCompletionFlash()
        {
            material.SetFloat(CompletionStartTimeId, Time.timeSinceLevelLoad);
        }

        //销毁该分段的运行时材质实例。
        public void Dispose()
        {
            if (material != null)
            {
                UnityEngine.Object.Destroy(material);
            }
        }

        //把Sprite在纹理中的矩形与宽高比传入Shader。
        private void ApplySpriteData()
        {
            Sprite sprite = image.sprite;
            if (sprite == null)
            {
                throw new InvalidOperationException("CostUI环形分段缺少Sprite：" + image.gameObject.name);
            }

            Rect textureRect = sprite.textureRect;
            Texture texture = sprite.texture;
            material.SetFloat(SpriteAspectId, textureRect.width / textureRect.height);
            material.SetVector(UvRectId, new Vector4(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height));
        }
    }
}
