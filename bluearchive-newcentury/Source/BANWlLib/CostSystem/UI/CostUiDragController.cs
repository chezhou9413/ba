using System;
using BANWlLib.DamageFontSystem.Comp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST轮盘拖动控制器负责接收鼠标拖动、保存相对入口按钮的位置并执行位置复位。
    public sealed class CostUiDragController : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public const float DefaultPositionX = 0f;
        public const float DefaultPositionY = 115f;

        private static CostUiDragController activeInstance;

        private RectTransform targetRect;
        private RectTransform parentRect;
        private Vector2 pointerStartLocal;
        private Vector2 positionStart;
        private bool dragging;

        //创建与轮盘可见范围一致的透明命中层，并读取当前存档中的轮盘位置。
        public void Initialize(RectTransform target, RectTransform hitArea)
        {
            targetRect = target ?? throw new ArgumentNullException(nameof(target));
            parentRect = target.parent as RectTransform;
            if (parentRect == null)
            {
                throw new InvalidOperationException("CostUI父节点缺少RectTransform。");
            }

            if (hitArea == null)
            {
                throw new ArgumentNullException(nameof(hitArea));
            }

            CreateDragSurface(hitArea);
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                throw new InvalidOperationException("CostUI根节点缺少CanvasGroup。");
            }

            group.interactable = true;
            group.blocksRaycasts = true;
            activeInstance = this;
            ApplySavedPosition();
        }

        //截获轮盘区域的按下事件，避免事件继续命中后方的什亭之匣入口按钮。
        public void OnPointerDown(PointerEventData eventData)
        {
        }

        //截获轮盘区域的抬起事件，保持轮盘点击与入口按钮点击互相独立。
        public void OnPointerUp(PointerEventData eventData)
        {
        }

        //消费轮盘点击，让点击目标停留在轮盘而不查找父级入口按钮。
        public void OnPointerClick(PointerEventData eventData)
        {
            eventData.Use();
        }

        //记录拖动开始时的指针和轮盘坐标，并暂停地图框选输入。
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!TryGetPointerLocal(eventData, out pointerStartLocal))
            {
                return;
            }

            positionStart = targetRect.anchoredPosition;
            dragging = true;
            LongPressDraggableButton.isMove = true;
        }

        //根据父节点局部坐标移动整个CostUI根节点。
        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || !TryGetPointerLocal(eventData, out Vector2 pointerLocal))
            {
                return;
            }

            targetRect.anchoredPosition = positionStart + pointerLocal - pointerStartLocal;
        }

        //结束拖动后把坐标写入游戏组件，供下一次保存和读档恢复。
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            LongPressDraggableButton.isMove = false;
            SavePosition();
        }

        //组件失活时释放全局拖动标记，避免地图框选保持禁用。
        private void OnDisable()
        {
            dragging = false;
            LongPressDraggableButton.isMove = false;
        }

        //组件销毁时清理当前轮盘实例引用。
        private void OnDestroy()
        {
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        //把当前存档位置恢复为默认值，并立即移动已经创建的轮盘。
        public static bool TryResetSavedPosition(out string reason)
        {
            reason = null;
            if (Current.ProgramState != ProgramState.Playing || Current.Game == null)
            {
                reason = "请进入游戏地图后再复位COST轮盘。";
                return false;
            }

            DisableCriticalComp component = Current.Game.GetComponent<DisableCriticalComp>();
            if (component == null)
            {
                reason = "游戏存档组件尚未初始化。";
                return false;
            }

            component.costUiPosX = DefaultPositionX;
            component.costUiPosY = DefaultPositionY;
            if (activeInstance != null && activeInstance.targetRect != null)
            {
                activeInstance.targetRect.anchoredPosition = DefaultPosition;
            }

            return true;
        }

        private static Vector2 DefaultPosition => new Vector2(DefaultPositionX, DefaultPositionY);

        //创建铺满CostRoot的透明Image，使只有轮盘实际显示区域能够接收拖动。
        private static void CreateDragSurface(RectTransform hitArea)
        {
            const string surfaceName = "CostDragSurface";
            Transform existing = hitArea.Find(surfaceName);
            GameObject surfaceObject;
            if (existing == null)
            {
                surfaceObject = new GameObject(
                    surfaceName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                surfaceObject.transform.SetParent(hitArea, false);
            }
            else
            {
                surfaceObject = existing.gameObject;
            }

            RectTransform surfaceRect = surfaceObject.GetComponent<RectTransform>();
            surfaceRect.anchorMin = Vector2.zero;
            surfaceRect.anchorMax = Vector2.one;
            surfaceRect.pivot = new Vector2(0.5f, 0.5f);
            surfaceRect.anchoredPosition = Vector2.zero;
            surfaceRect.sizeDelta = Vector2.zero;

            Image surface = surfaceObject.GetComponent<Image>();
            surface.color = Color.clear;
            surface.raycastTarget = true;
            //透明命中层覆盖装饰节点，统一由轮盘控制器处理指针事件。
            surfaceObject.transform.SetAsLastSibling();
        }

        //读取当前存档保存的轮盘相对坐标，没有游戏组件时采用默认位置。
        private void ApplySavedPosition()
        {
            DisableCriticalComp component = Current.Game?.GetComponent<DisableCriticalComp>();
            targetRect.anchoredPosition = component == null
                ? DefaultPosition
                : new Vector2(component.costUiPosX, component.costUiPosY);
        }

        //保存当前轮盘相对入口按钮的位置。
        private void SavePosition()
        {
            DisableCriticalComp component = Current.Game?.GetComponent<DisableCriticalComp>();
            if (component == null)
            {
                Log.Error("[BA COST] 无法保存轮盘位置：游戏存档组件尚未初始化。");
                return;
            }

            component.costUiPosX = targetRect.anchoredPosition.x;
            component.costUiPosY = targetRect.anchoredPosition.y;
        }

        //把屏幕指针换算成入口按钮父坐标系中的局部位置。
        private bool TryGetPointerLocal(PointerEventData eventData, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);
        }
    }
}
