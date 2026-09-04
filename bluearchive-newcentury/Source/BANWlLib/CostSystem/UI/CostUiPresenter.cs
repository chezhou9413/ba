using System;
using UnityEngine;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST界面Presenter负责把当前地图共享池平滑同步到双环、债务颜色和中央数字。
    [DisallowMultipleComponent]
    public sealed class CostUiPresenter : MonoBehaviour
    {
        private const float SmoothTime = 0.15f;

        private CanvasGroup canvasGroup;
        private CostRingView outerRing;
        private CostRingView innerRing;
        private CostDigitView digitView;
        private Map currentMap;
        private float displayedCost;
        private float displayVelocity;
        private int previousTargetTenths;
        private bool initialized;

        //使用已加载的主UI资源包解析预制体节点和数字图集。
        public void Initialize(AssetBundle bundle)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                throw new InvalidOperationException("CostUI根节点缺少CanvasGroup。" );
            }

            Transform outer = transform.Find("CostRoot/OuterRing");
            Transform inner = outer?.Find("InnerRing");
            Transform value = inner?.Find("CostValue");
            if (outer == null || inner == null || value == null)
            {
                throw new InvalidOperationException("CostUI预制体层级不完整。" );
            }

            outerRing = new CostRingView(outer, "OuterSegment_");
            innerRing = new CostRingView(inner, "InnerSegment_");
            digitView = new CostDigitView(value, bundle);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            initialized = true;
        }

        //每帧读取当前地图池，平滑进度并更新数字弹跳。
        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null)
            {
                canvasGroup.alpha = 0f;
                return;
            }

            canvasGroup.alpha = 1f;
            Map map = Find.CurrentMap;
            MapComponent_BACostPool pool = BACostPoolService.GetPool(map);
            bool mapChanged = currentMap != map;
            if (mapChanged)
            {
                currentMap = map;
                displayedCost = pool.CurrentCost;
                displayVelocity = 0f;
                previousTargetTenths = pool.CurrentCostTenths;
            }
            else
            {
                DetectCompletedCost(pool.CurrentCostTenths);
                displayedCost = Mathf.SmoothDamp(
                    displayedCost,
                    pool.CurrentCost,
                    ref displayVelocity,
                    SmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);
                if (Mathf.Abs(displayedCost - pool.CurrentCost) < 0.001f)
                {
                    displayedCost = pool.CurrentCost;
                }
            }

            ApplyVisuals(pool);
            digitView.UpdateBounce(Time.unscaledDeltaTime);
        }

        //释放由环形分段创建的全部运行时材质。
        private void OnDestroy()
        {
            outerRing?.Dispose();
            innerRing?.Dispose();
        }

        //在目标COST向上跨过整数边界时触发一次中央数字弹跳。
        private void DetectCompletedCost(int targetTenths)
        {
            if (targetTenths > previousTargetTenths)
            {
                int previousWhole = Mathf.FloorToInt(previousTargetTenths / 10f);
                int currentWhole = Mathf.FloorToInt(targetTenths / 10f);
                if (currentWhole > previousWhole)
                {
                    digitView.PlayBounce();
                    FlashCompletedCosts(previousWhole, currentWhole);
                }
            }

            previousTargetTenths = targetTenths;
        }

        //让一次回复跨越的所有正向整数格同时开始白色完成光效。
        private void FlashCompletedCosts(int previousWhole, int currentWhole)
        {
            int firstCompletedCost = Mathf.Max(1, previousWhole + 1);
            int lastCompletedCost = Mathf.Min(20, currentWhole);
            if (firstCompletedCost > lastCompletedCost)
            {
                return;
            }

            if (firstCompletedCost <= 10)
            {
                outerRing.FlashCompletedRange(firstCompletedCost - 1, Mathf.Min(10, lastCompletedCost) - 1);
            }

            if (lastCompletedCost > 10)
            {
                innerRing.FlashCompletedRange(Mathf.Max(11, firstCompletedCost) - 11, lastCompletedCost - 11);
            }
        }

        //把平滑值分配给普通外圈、20点内圈或最多5格反向债务。
        private void ApplyVisuals(MapComponent_BACostPool pool)
        {
            bool debtMode = displayedCost < 0f;
            if (debtMode)
            {
                outerRing.SetValue(Mathf.Min(5f, -displayedCost), true, true);
                innerRing.SetActive(false);
            }
            else
            {
                outerRing.SetValue(Mathf.Min(10f, displayedCost), false, false);
                bool showInner = pool.MaximumCost > 10;
                innerRing.SetActive(showInner);
                if (showInner)
                {
                    innerRing.SetValue(Mathf.Max(0f, displayedCost - 10f), false, false);
                }
            }

            digitView.SetValue(pool.CurrentCostTenths);
        }
    }
}
