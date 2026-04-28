using BANWlLib.BaDef;
using BANWlLib.DamageFontSystem.Comp;
using MyCoolMusicMod.MyCoolMusicMod;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BANWlLib.mainUI.Mission.MonoComp
{
    // 1. 添加 IBeginDragHandler 接口
    internal class MonoComp_BaLongPressDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler
    {
        public static bool isMove = false;
        [Header("移动设置")]
        public RectTransform targetToMove;

        // 不需要 isPointerDown 和 timer 了，我们要的是即时响应
        private bool isDragging = false;

        private RectTransform moveTransform;
        private RectTransform parentRect; // 当前的父物体（可能是原来的，也可能是Canvas）
        private Canvas canvas;
        private Vector2 offset;

        public Vector2 initoffset;

        private Transform originalParent;
        private int originalSiblingIndex;
        public BaStudentRaceDef studentRaceDef;

        private void Start()
        {
            if (targetToMove == null) moveTransform = GetComponent<RectTransform>();
            else moveTransform = targetToMove;

            canvas = GetComponentInParent<Canvas>();

            // 初始父物体
            if (moveTransform != null) parentRect = moveTransform.parent as RectTransform;
            if (parentRect == null && canvas != null) parentRect = canvas.transform as RectTransform;

            initoffset = moveTransform.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 这里只做简单的按下反馈，或者什么都不做
            // 真正的坐标计算移到 OnBeginDrag，因为那时候父物体会改变
        }

        // 2. 核心逻辑：开始拖拽的一瞬间触发
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (moveTransform == null || canvas == null) return;

            isDragging = true;
            isMove = true;

            // 播放音效
            if (studentRaceDef?.baStudentData?.DraggableAudio != null)
            {
                LoopBGMManager.PlayClipDirectly(studentRaceDef.baStudentData.DraggableAudio);
            }

            // --- A. 记录老家 ---
            originalParent = moveTransform.parent;
            originalSiblingIndex = moveTransform.GetSiblingIndex();

            // --- B. 搬家到 Canvas (提层级) ---
            moveTransform.SetParent(canvas.transform, true);
            moveTransform.SetAsLastSibling();

            // --- C. 更新当前的父物体引用为 Canvas ---
            parentRect = canvas.transform as RectTransform;

            // --- D. 关键：父物体变了，必须重新计算 Offset ---
            // 否则物体会瞬间闪烁或跳动
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out localMousePos
            );

            // 计算由于“提层级”后的新坐标下的偏移量
            offset = moveTransform.anchoredPosition - localMousePos;
        }

        // 3. 持续拖拽中
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || moveTransform == null || parentRect == null) return;

            Vector2 localMousePos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out localMousePos))
            {
                // 跟随鼠标
                moveTransform.anchoredPosition = localMousePos + offset;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 只有当真正发生过拖拽时，才执行归位逻辑
            if (isDragging)
            {
                var btn = GetComponent<Button>();

                // --- 恢复老家 ---
                if (moveTransform != null && originalParent != null)
                {
                    moveTransform.SetParent(originalParent, true);
                    moveTransform.SetSiblingIndex(originalSiblingIndex);

                    // 恢复 parentRect 引用
                    parentRect = originalParent as RectTransform;
                }

                // 归位到初始坐标
                moveTransform.anchoredPosition = initoffset;

                StartCoroutine(ResetAfterDrag(btn));
            }

            // 重置状态
            isDragging = false;
            isMove = false;
        }

        private IEnumerator ResetAfterDrag(Button btn)
        {
            yield return null;
            if (btn != null) btn.interactable = true;
        }
    }
}