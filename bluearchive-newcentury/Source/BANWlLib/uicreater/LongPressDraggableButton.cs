using BANWlLib.DamageFontSystem.Comp;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;

public class LongPressDraggableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public static bool isMove = false;               // 是否处于拖动状态（供外部Harmony使用）
    public float longPressThreshold = 0.3f;          // 长按阈值（秒）

    private bool isPointerDown = false;              // 是否按下
    private bool isDragging = false;                 // 是否正在拖动
    private float pointerDownTimer = 0f;             // 按下计时器

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        // 读取保存位置
        DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
        rectTransform.anchoredPosition = new Vector2(comp.savePosX, comp.savePosY);
    }

    private void Update()
    {
        // 检查长按计时
        if (isPointerDown && !isDragging)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= longPressThreshold)
            {
                // 长按触发拖动模式
                isDragging = true;
                isMove = true;

                // 拖动时禁用按钮交互，防止 Unity 内部触发 onClick
                var btn = GetComponent<Button>();
                if (btn != null)
                    btn.interactable = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;

        // 计算偏移
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null, // Screen Space - Overlay 模式无需相机
            out Vector2 localMousePos
        );

        offset = rectTransform.anchoredPosition - localMousePos;

        // 阻止事件继续传递给 Unity 的 Button，避免提前触发 onClick
        eventData.Use();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 仅当长按后进入拖动状态才移动
        if (!isDragging || canvas == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + offset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        pointerDownTimer = 0f;

        var btn = GetComponent<Button>();

        // 如果拖动了，就不要立刻恢复交互，也不要执行点击
        if (isDragging)
        {
            StartCoroutine(ResetAfterDrag(btn));
        }
        else
        {
            // 没拖动才触发点击
            btn?.onClick.Invoke();
            if (btn != null)
                btn.interactable = true;
        }

        // 重置状态
        isDragging = false;
        isMove = false;

        // 保存按钮位置
        DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
        comp.savePosX = rectTransform.anchoredPosition.x;
        comp.savePosY = rectTransform.anchoredPosition.y;
    }

    private IEnumerator ResetAfterDrag(Button btn)
    {
        yield return null; // 延迟一帧
        if (btn != null)
            btn.interactable = true;
    }
}

public class CloseBoxDize
{
    [HarmonyPatch(typeof(DragBox), nameof(DragBox.DragBoxOnGUI))]
    public static class DragBox_DragBoxOnGUI_Patch
    {
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }

    [HarmonyPatch(typeof(WorldDragBox), nameof(WorldDragBox.DragBoxOnGUI))]
    public static class WorldDragBox_DragBoxOnGUI_Patch
    {
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }

    // 禁用地图拖拽框选择物品
    [HarmonyPatch(typeof(Selector), "SelectInsideDragBox")]
    public static class Selector_SelectInsideDragBox_Patch
    {
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }

    // 禁用世界地图拖拽框选择物品
    [HarmonyPatch(typeof(WorldSelector), "SelectInsideDragBox")]
    public static class WorldSelector_SelectInsideDragBox_Patch
    {
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }
}
