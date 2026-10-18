#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.BaDef;
using MyCoolMusicMod.MyCoolMusicMod;
using System.Collections;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using RimUnity.EventSystems;
#else
using UnityEngine.EventSystems;
#endif
#if RW_1_7
using RimUnity.UI;
#else
using UnityEngine.UI;
#endif

namespace BANWlLib.mainUI.Mission.MonoComp
{
    internal class MonoComp_BaLongPressDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler
    {
        public static bool isMove = false;
        [Header("移动设置")]
        public RectTransform targetToMove;

        private bool isDragging = false;

        private RectTransform moveTransform;
        private RectTransform parentRect; // 当前的父物体（可能是原来的，也可能是Canvas）
        private Canvas canvas;
        private Vector2 offset;

        public Vector2 initoffset;

        private Transform originalParent;
        private int originalSiblingIndex;
        public BaStudentDef studentDef;

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
            // 真正的坐标计算移到 OnBeginDrag，因为那时候父物体会改变
        }

        //开始拖拽。
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (moveTransform == null || canvas == null) return;

            isDragging = true;
            isMove = true;

            // 播放音效
            if (studentDef?.baStudentData?.DraggableAudio != null)
            {
                LoopBGMManager.PlayClipDirectly(studentDef.baStudentData.DraggableAudio);
            }

            //记录原父节点。
            originalParent = moveTransform.parent;
            originalSiblingIndex = moveTransform.GetSiblingIndex();

            //移到 Canvas 顶层。
            moveTransform.SetParent(canvas.transform, true);
            moveTransform.SetAsLastSibling();

            parentRect = canvas.transform as RectTransform;

            //父节点改变后重新计算拖拽偏移。
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

        //更新拖拽位置。
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

                //恢复原父节点。
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
