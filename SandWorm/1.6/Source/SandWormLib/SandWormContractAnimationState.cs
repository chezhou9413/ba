using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SandWormLib
{
    /// <summary>
    /// 职责：集中管理沙海巨虫挑战合约窗口的 DOTween 动画数值和生命周期。
    /// </summary>
    public sealed class SandWormContractAnimationState
    {
        private readonly List<Tween> tweens = new List<Tween>();
        private readonly Dictionary<string, float> hoverValues = new Dictionary<string, float>();
        private readonly Dictionary<string, bool> hoverTargets = new Dictionary<string, bool>();
        private readonly Dictionary<string, float> entryValues = new Dictionary<string, float>();
        private readonly Dictionary<string, float> flashValues = new Dictionary<string, float>();
        private readonly Dictionary<string, float> exitValues = new Dictionary<string, float>();
        private readonly HashSet<string> exitingKeys = new HashSet<string>();
        private Sequence openSequence;
        private Sequence launchSequence;
        private Tween buttonPulseTween;
        private Tween detailTween;
        private string detailTargetKey;

        public float WindowAlpha { get; private set; }
        public float WindowOffsetY { get; private set; } = 18f;
        public float WindowScale { get; private set; } = 0.965f;
        public float LeftPanelSlide { get; private set; } = 34f;
        public float MatrixPanelSlide { get; private set; } = 30f;
        public float RightPanelSlide { get; private set; } = 34f;
        public float FooterSlide { get; private set; } = 24f;
        public float StartupSweep { get; private set; } = -0.25f;
        public float InvalidShake { get; private set; }
        public float StartButtonPulse { get; private set; }
        public float LaunchAlpha { get; private set; }
        public float LaunchProgress { get; private set; }
        public float LaunchSweep { get; private set; } = -0.2f;
        public bool LaunchVisible { get; private set; }
        public float DetailAlpha { get; private set; }
        public float DetailOffsetY { get; private set; } = 8f;
        public string DetailKey { get; private set; }

        /// <summary>
        /// 职责：播放窗口打开时的淡入、缩放、面板错峰入场和扫描扫光。
        /// </summary>
        public void PlayOpen()
        {
            KillOpenSequence();
            WindowAlpha = 0f;
            WindowOffsetY = 18f;
            WindowScale = 0.965f;
            LeftPanelSlide = 34f;
            MatrixPanelSlide = 30f;
            RightPanelSlide = 34f;
            FooterSlide = 24f;
            StartupSweep = -0.25f;

            openSequence = DOTween.Sequence().SetUpdate(true);
            openSequence.Join(DOTween.To(() => WindowAlpha, value => WindowAlpha = value, 1f, 0.34f).SetEase(Ease.OutCubic));
            openSequence.Join(DOTween.To(() => WindowOffsetY, value => WindowOffsetY = value, 0f, 0.48f).SetEase(Ease.OutBack));
            openSequence.Join(DOTween.To(() => WindowScale, value => WindowScale = value, 1f, 0.48f).SetEase(Ease.OutBack));
            openSequence.Insert(0.06f, DOTween.To(() => LeftPanelSlide, value => LeftPanelSlide = value, 0f, 0.46f).SetEase(Ease.OutExpo));
            openSequence.Insert(0.12f, DOTween.To(() => MatrixPanelSlide, value => MatrixPanelSlide = value, 0f, 0.50f).SetEase(Ease.OutExpo));
            openSequence.Insert(0.18f, DOTween.To(() => RightPanelSlide, value => RightPanelSlide = value, 0f, 0.54f).SetEase(Ease.OutExpo));
            openSequence.Insert(0.24f, DOTween.To(() => FooterSlide, value => FooterSlide = value, 0f, 0.42f).SetEase(Ease.OutExpo));
            openSequence.Join(DOTween.To(() => StartupSweep, value => StartupSweep = value, 1.25f, 0.9f).SetEase(Ease.OutCubic));
            tweens.Add(openSequence);

            KillButtonPulse();
            buttonPulseTween = DOTween.To(() => StartButtonPulse, value => StartButtonPulse = value, 1f, 0.9f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
            tweens.Add(buttonPulseTween);
        }

        /// <summary>
        /// 职责：读取并维护指定控件的悬停动画值。
        /// </summary>
        public float Hover(string key, bool active)
        {
            if (!hoverValues.TryGetValue(key, out float current))
            {
                hoverValues[key] = current = 0f;
            }

            if (!hoverTargets.TryGetValue(key, out bool lastTarget) || lastTarget != active)
            {
                hoverTargets[key] = active;
                float target = active ? 1f : 0f;
                AddTween(DOTween.To(() => hoverValues[key], value => hoverValues[key] = value, target, 0.14f).SetEase(Ease.OutSine));
            }

            return current;
        }

        /// <summary>
        /// 职责：读取或启动列表项、节点项的逐条入场动画。
        /// </summary>
        public float Entry(string key, int index, float delayStep, float duration)
        {
            if (entryValues.TryGetValue(key, out float value))
            {
                return value;
            }

            entryValues[key] = 0f;
            float delay = Mathf.Min(0.55f, index * delayStep);
            AddTween(DOTween.To(() => entryValues[key], next => entryValues[key] = next, 1f, duration)
                .SetDelay(delay)
                .SetEase(Ease.OutBack));
            return 0f;
        }

        /// <summary>
        /// 职责：播放指定控件的短促高亮闪烁。
        /// </summary>
        public void Flash(string key)
        {
            flashValues[key] = 1f;
            AddTween(DOTween.To(() => flashValues[key], value => flashValues[key] = value, 0f, 0.45f).SetEase(Ease.OutCubic));
        }

        /// <summary>
        /// 职责：读取指定控件当前的闪烁强度。
        /// </summary>
        public float FlashValue(string key)
        {
            return flashValues.TryGetValue(key, out float value) ? value : 0f;
        }

        /// <summary>
        /// 职责：启动指定列表槽的右键滑出动画，并在结束后执行移除逻辑。
        /// </summary>
        public void PlayExit(string key, Action onComplete)
        {
            if (string.IsNullOrEmpty(key) || exitingKeys.Contains(key))
            {
                return;
            }

            exitingKeys.Add(key);
            exitValues[key] = 0f;
            AddTween(DOTween.To(() => exitValues[key], value => exitValues[key] = value, 1f, 0.24f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    exitingKeys.Remove(key);
                    exitValues.Remove(key);
                    entryValues.Remove(key);
                    hoverValues.Remove(key);
                    hoverTargets.Remove(key);
                    flashValues.Remove(key);
                    onComplete?.Invoke();
                }));
        }

        /// <summary>
        /// 职责：读取指定列表槽当前的滑出进度。
        /// </summary>
        public float ExitValue(string key)
        {
            return exitValues.TryGetValue(key, out float value) ? value : 0f;
        }

        /// <summary>
        /// 职责：判断指定列表槽是否正在执行右键滑出动画。
        /// </summary>
        public bool IsExiting(string key)
        {
            return exitingKeys.Contains(key);
        }

        /// <summary>
        /// 职责：根据当前悬停词条维护详情弹窗的淡入和上浮动画值。
        /// </summary>
        public void SetDetailTarget(string key)
        {
            if (detailTargetKey == key)
            {
                return;
            }

            detailTargetKey = key;
            if (detailTween != null && detailTween.IsActive())
            {
                detailTween.Kill();
            }

            Sequence sequence = DOTween.Sequence().SetUpdate(true);
            if (string.IsNullOrEmpty(key))
            {
                sequence.Join(DOTween.To(() => DetailAlpha, value => DetailAlpha = value, 0f, 0.10f).SetEase(Ease.InSine));
                sequence.Join(DOTween.To(() => DetailOffsetY, value => DetailOffsetY = value, 8f, 0.10f).SetEase(Ease.InSine));
                sequence.OnComplete(() => DetailKey = null);
            }
            else
            {
                DetailKey = key;
                DetailAlpha = 0f;
                DetailOffsetY = 8f;
                sequence.Join(DOTween.To(() => DetailAlpha, value => DetailAlpha = value, 1f, 0.16f).SetEase(Ease.OutCubic));
                sequence.Join(DOTween.To(() => DetailOffsetY, value => DetailOffsetY = value, 0f, 0.18f).SetEase(Ease.OutCubic));
            }

            detailTween = sequence;
            AddTween(sequence);
        }

        /// <summary>
        /// 职责：播放非法操作时的短促震动。
        /// </summary>
        public void PlayInvalidShake()
        {
            InvalidShake = 1f;
            AddTween(DOTween.To(() => InvalidShake, value => InvalidShake = value, 0f, 0.24f).SetEase(Ease.OutElastic));
        }

        /// <summary>
        /// 职责：播放开始挑战时的确认扫描覆盖层，并在结束后回调窗口关闭逻辑。
        /// </summary>
        public void PlayLaunch(Action onComplete)
        {
            KillLaunchSequence();
            LaunchVisible = true;
            LaunchAlpha = 0f;
            LaunchProgress = 0f;
            LaunchSweep = -0.2f;

            launchSequence = DOTween.Sequence().SetUpdate(true);
            launchSequence.Append(DOTween.To(() => LaunchAlpha, value => LaunchAlpha = value, 1f, 0.16f).SetEase(Ease.OutCubic));
            launchSequence.Insert(0.10f, DOTween.To(() => LaunchProgress, value => LaunchProgress = value, 0.32f, 0.20f).SetEase(Ease.OutCubic));
            launchSequence.Insert(0.30f, DOTween.To(() => LaunchProgress, value => LaunchProgress = value, 0.78f, 0.34f).SetEase(Ease.InOutCubic));
            launchSequence.Insert(0.62f, DOTween.To(() => LaunchProgress, value => LaunchProgress = value, 1f, 0.22f).SetEase(Ease.OutExpo));
            launchSequence.Insert(0.24f, DOTween.To(() => LaunchSweep, value => LaunchSweep = value, 1.25f, 0.48f).SetEase(Ease.OutCubic));
            launchSequence.Insert(0.78f, DOTween.To(() => LaunchAlpha, value => LaunchAlpha = value, 0f, 0.20f).SetEase(Ease.InCubic));
            launchSequence.OnComplete(() =>
            {
                LaunchVisible = false;
                onComplete?.Invoke();
            });
            tweens.Add(launchSequence);
        }

        /// <summary>
        /// 职责：释放窗口关闭前仍在运行的 Tween 和缓存状态。
        /// </summary>
        public void Dispose()
        {
            KillOpenSequence();
            KillLaunchSequence();
            KillButtonPulse();
            KillDetailTween();
            for (int i = 0; i < tweens.Count; i++)
            {
                Tween tween = tweens[i];
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }

            tweens.Clear();
            hoverValues.Clear();
            hoverTargets.Clear();
            entryValues.Clear();
            flashValues.Clear();
            exitValues.Clear();
            exitingKeys.Clear();
            detailTargetKey = null;
            DetailKey = null;
            DetailAlpha = 0f;
            DetailOffsetY = 8f;
        }

        /// <summary>
        /// 职责：登记 Tween 并清理已经失效的引用。
        /// </summary>
        private void AddTween(Tween tween)
        {
            if (tween == null)
            {
                return;
            }

            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                Tween existing = tweens[i];
                if (existing == null || !existing.IsActive())
                {
                    tweens.RemoveAt(i);
                }
            }

            tweens.Add(tween.SetUpdate(true));
        }

        /// <summary>
        /// 职责：停止窗口打开组合动画。
        /// </summary>
        private void KillOpenSequence()
        {
            if (openSequence != null && openSequence.IsActive())
            {
                openSequence.Kill();
            }

            openSequence = null;
        }

        /// <summary>
        /// 职责：停止开始挑战确认覆盖层动画。
        /// </summary>
        private void KillLaunchSequence()
        {
            if (launchSequence != null && launchSequence.IsActive())
            {
                launchSequence.Kill();
            }

            launchSequence = null;
            LaunchVisible = false;
        }

        /// <summary>
        /// 职责：停止开始按钮的循环脉冲动画。
        /// </summary>
        private void KillButtonPulse()
        {
            if (buttonPulseTween != null && buttonPulseTween.IsActive())
            {
                buttonPulseTween.Kill();
            }

            buttonPulseTween = null;
        }

        /// <summary>
        /// 职责：停止词条详情弹窗动画。
        /// </summary>
        private void KillDetailTween()
        {
            if (detailTween != null && detailTween.IsActive())
            {
                detailTween.Kill();
            }

            detailTween = null;
        }
    }
}
