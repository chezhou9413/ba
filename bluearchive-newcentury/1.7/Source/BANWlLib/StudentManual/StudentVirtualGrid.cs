using System;
using System.Collections.Generic;
using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.Tool;
using RimUnity;
using RimUnity.UI;
using Verse;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    //用可见行和前后预留行复用学生卡片，滚动成本不随名册总量增加。
    public sealed class StudentVirtualGrid : MonoBehaviour
    {
        private readonly List<StudentListShow> cards = new();
        private readonly List<Entry> students = new();
        private readonly Dictionary<string, StudentData> owned = new(StringComparer.Ordinal);
        private readonly HashSet<string> collected = new(StringComparer.Ordinal);
        private readonly Vector3[] corners = new Vector3[4];
        private RectTransform content, viewport;
        private GridLayoutGroup grid;
        private ScrollRect scroll;
        private ManualDataGameComp tracker;
        private bool dirty = true;
        private int filter = -1, columns, windowStart = -1, windowEnd = -1;
        private Vector2 lastSize, lastViewportSize, lastPosition;

        //记录排序所需的状态，比较时不反复搜索名册或查询 Pawn。
        private struct Entry
        {
            public BaStudentUI Ui;
            public StudentData Data;
            public bool Collected, Available;
            public int Order;
        }

        //沿用预制体网格参数，由虚拟窗口接管尺寸和位置。
        private void Awake()
        {
            content = (RectTransform)transform;
            scroll = GetComponentInParent<ScrollRect>();
            viewport = scroll.viewport ?? (RectTransform)scroll.transform;
            grid = GetComponent<GridLayoutGroup>();
            if (grid.startAxis != GridLayoutGroup.Axis.Horizontal ||
                grid.startCorner != GridLayoutGroup.Corner.UpperLeft ||
                grid.constraint == GridLayoutGroup.Constraint.FixedRowCount ||
                grid.cellSize.x + grid.spacing.x <= 0 || grid.cellSize.y + grid.spacing.y <= 0)
                throw new InvalidOperationException("学生虚拟网格需要从左上角横向排列、正数单元步长的网格。");
            grid.enabled = false;
            tracker = Current.Game.GetComponent<ManualDataGameComp>();
            scroll.onValueChanged.AddListener(OnScroll);
            StudentManualEvents.OnRefreshAllStudentData += MarkDirty;
            StudentManualEvents.resetUIlist += MarkDirty;
        }

        //重新打开页面时合并刷新名册，恢复图片引用。
        private void OnEnable() => MarkDirty();

        //同一操作发出的多个刷新事件只在本帧末处理一次。
        private void MarkDirty() => dirty = true;

        //每帧只检查窗口尺寸；数据刷新与卡片换绑按实际变化执行。
        private void LateUpdate()
        {
            bool refresh = dirty;
            if (refresh)
            {
                dirty = false;
                RebuildStudents();
            }
            UpdateWindow(refresh);
        }

        //滚轮和惯性改变位置时立即调整窗口，避免等待下一帧才补齐卡片。
        private void OnScroll(Vector2 value)
        {
            if (isActiveAndEnabled && !dirty) UpdateWindow(false);
        }

        //先筛选数据，再按收藏、拥有、可用状态、等级和姓名稳定排序。
        private void RebuildStudents()
        {
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            owned.Clear();
            collected.Clear();
            foreach (StudentData data in tracker.HaveStudent)
                if (data != null && !string.IsNullOrEmpty(data.DefName))
                    owned[StudentIdentityUtility.GetStudentId(data.DefName)] = data;
            foreach (string id in tracker.StudentCollect) collected.Add(id);
            students.Clear();
            for (int i = 0; i < ManualMapData.studentUIList.Count; i++)
            {
                BaStudentUI ui = ManualMapData.studentUIList[i];
                owned.TryGetValue(ui.StudentId, out StudentData data);
                if (ManualMapData.selectStudentIndex == 1 && data == null ||
                    ManualMapData.selectStudentIndex == 2 && data != null) continue;
                students.Add(new Entry
                {
                    Ui = ui, Data = data, Collected = collected.Contains(ui.StudentId), Order = i,
                    Available = data != null && (data.StudentPawn == null || data.StudentPawn.DestroyedOrNull() || !data.StudentPawn.Dead)
                });
            }
            students.Sort(CompareStudents);
            if (filter != ManualMapData.selectStudentIndex)
            {
                filter = ManualMapData.selectStudentIndex;
                scroll.StopMovement();
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0);
            }
        }

        //同名条目沿用原名册顺序，确保连续刷新不会交换位置。
        private static int CompareStudents(Entry a, Entry b)
        {
            int order = b.Collected.CompareTo(a.Collected);
            if (order == 0) order = (b.Data != null).CompareTo(a.Data != null);
            if (order == 0) order = b.Available.CompareTo(a.Available);
            if (order == 0) order = (b.Data?.StudentLv ?? 0).CompareTo(a.Data?.StudentLv ?? 0);
            if (order == 0) order = StringComparer.CurrentCulture.Compare(a.Ui.StudentName, b.Ui.StudentName);
            return order == 0 ? a.Order.CompareTo(b.Order) : order;
        }

        //以内容局部坐标计算视口，保留画布缩放后的单元尺寸与滚动范围。
        private void UpdateWindow(bool refresh)
        {
            Vector2 viewSize = viewport.rect.size;
            float width = content.rect.width;
            if (!refresh && lastViewportSize == viewSize && lastSize.x == width && lastPosition == content.anchoredPosition) return;
            lastViewportSize = viewSize;
            viewport.GetWorldCorners(corners);
            float top = float.NegativeInfinity, bottom = float.PositiveInfinity;
            foreach (Vector3 corner in corners)
            {
                float y = content.InverseTransformPoint(corner).y;
                top = Mathf.Max(top, y);
                bottom = Mathf.Min(bottom, y);
            }
            float height = top - bottom;
            Vector2 size = new(width, height);
            if (size.x <= 0 || size.y <= 0) return;
            bool resized = size != lastSize;
            float stepX = grid.cellSize.x + grid.spacing.x;
            float stepY = grid.cellSize.y + grid.spacing.y;
            if (resized || refresh)
            {
                lastSize = size;
                columns = grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                    ? Mathf.Max(1, grid.constraintCount)
                    : Mathf.Max(1, Mathf.FloorToInt((size.x - grid.padding.horizontal + grid.spacing.x + 0.001f) / stepX));
                int rows = (students.Count + columns - 1) / columns;
                float contentHeight = Mathf.Max(height, grid.padding.vertical + rows * stepY - (rows > 0 ? grid.spacing.y : 0));
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
                Vector2 position = content.anchoredPosition;
                position.y = Mathf.Clamp(position.y, 0, Mathf.Max(0, contentHeight - height));
                content.anchoredPosition = position;
                //尺寸或筛选改变后位置可能被夹回边界，重新获取局部可见顶部。
                top = float.NegativeInfinity;
                foreach (Vector3 corner in corners) top = Mathf.Max(top, content.InverseTransformPoint(corner).y);
            }

            int poolRows = Mathf.CeilToInt(height / stepY) + 3;
            int capacity = Mathf.Min(students.Count, poolRows * columns);
            EnsureCards(capacity);
            int firstRow = Mathf.Max(0, Mathf.FloorToInt((content.rect.yMax - top - grid.padding.top) / stepY) - 1);
            int first = Mathf.Min(firstRow * columns, Mathf.Max(0, students.Count - 1));
            int end = Mathf.Min(students.Count, first + capacity);
            lastPosition = content.anchoredPosition;
            if (!refresh && !resized && first == windowStart && end == windowEnd) return;
            windowStart = first;
            windowEnd = end;
            float usedWidth = Mathf.Min(columns, students.Count) * stepX - grid.spacing.x;
            float left = grid.padding.left + (size.x - grid.padding.horizontal - usedWidth) * ((int)grid.childAlignment % 3) * 0.5f;
            for (int slot = 0; slot < cards.Count; slot++)
            {
                //按数据索引循环复用；跨一行只换绑移出窗口的那一行。
                int index = first + (slot - first % cards.Count + cards.Count) % cards.Count;
                StudentListShow card = cards[slot];
                if (index >= end)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }
                Entry entry = students[index];
                RectTransform rect = (RectTransform)card.transform;
                rect.anchoredPosition = new Vector2(left + index % columns * stepX + grid.cellSize.x * rect.pivot.x,
                    -grid.padding.top - index / columns * stepY - grid.cellSize.y * (1 - rect.pivot.y));
                if (refresh || card.BaStudentUI != entry.Ui || card.studentData != entry.Data)
                    card.BindStudent(entry.Ui, entry.Data);
                card.gameObject.SetActive(true);
            }
        }

        //仅在视口能容纳更多卡片时扩容，普通滚动不创建或销毁对象。
        private void EnsureCards(int count)
        {
            while (cards.Count < count)
            {
                GameObject item = GameObject.Instantiate(ManualMapData.StudentListOBJ);
                item.transform.SetParent(content, false);
                RectTransform rect = (RectTransform)item.transform;
                rect.anchorMin = rect.anchorMax = Vector2.up;
                rect.sizeDelta = grid.cellSize;
                cards.Add(item.AddComponent<StudentListShow>());
            }
        }

        //销毁页面时解除事件，卡片自身负责归还图片引用。
        private void OnDestroy()
        {
            scroll.onValueChanged.RemoveListener(OnScroll);
            StudentManualEvents.OnRefreshAllStudentData -= MarkDirty;
            StudentManualEvents.resetUIlist -= MarkDirty;
        }
    }
}
