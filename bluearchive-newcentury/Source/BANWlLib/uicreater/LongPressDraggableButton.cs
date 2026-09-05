using BANWlLib.DamageFontSystem.Comp;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;

//入口按钮控制器负责长按拖动和保存位置，普通点击交由按钮组件处理。
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

    //缓存拖动所需的矩形与画布。
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    //从当前存档恢复入口按钮位置。
    private void Start()
    {
        // 读取保存位置
        DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
        rectTransform.anchoredPosition = new Vector2(comp.savePosX, comp.savePosY);
    }

    //达到长按阈值后进入拖动并禁用按钮点击。
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

    //记录左键按下和相对画布的拖动偏移。
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

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

    //在长按拖动期间更新入口按钮坐标。
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

    //结束本按钮发起的按压并保存位置，拖动后的点击保持禁用到下一帧。
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown || eventData.button != PointerEventData.InputButton.Left)
            return;

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
            //普通点击由按钮组件派发一次，抬起事件只恢复交互状态。
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

    //等待当前指针事件处理结束后恢复按钮交互。
    private IEnumerator ResetAfterDrag(Button btn)
    {
        yield return null; // 延迟一帧
        if (btn != null)
            btn.interactable = true;
    }
}

//地图框选补丁容器负责在拖动模组按钮期间屏蔽框选。
public class CloseBoxDize
{
    //地图框选绘制补丁负责依据拖动状态决定是否绘制框选。
    [HarmonyPatch(typeof(DragBox), nameof(DragBox.DragBoxOnGUI))]
    public static class DragBox_DragBoxOnGUI_Patch
    {
        //仅在未拖动按钮时执行原版地图框选绘制。
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }

    //世界地图框选绘制补丁负责屏蔽按钮拖动期间的框选。
    [HarmonyPatch(typeof(WorldDragBox), nameof(WorldDragBox.DragBoxOnGUI))]
    public static class WorldDragBox_DragBoxOnGUI_Patch
    {
        //仅在未拖动按钮时执行原版世界框选绘制。
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }

    // 禁用地图拖拽框选择物品
    [HarmonyPatch(typeof(Selector), "SelectInsideDragBox")]
    public static class Selector_SelectInsideDragBox_Patch
    {
        //仅在未拖动按钮时提交地图框选结果。
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }

    // 禁用世界地图拖拽框选择物品
    [HarmonyPatch(typeof(WorldSelector), "SelectInsideDragBox")]
    public static class WorldSelector_SelectInsideDragBox_Patch
    {
        //仅在未拖动按钮时提交世界地图框选结果。
        static bool Prefix()
        {
            return !LongPressDraggableButton.isMove;
        }
    }
}
