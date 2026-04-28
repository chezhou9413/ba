using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using BANWlLib.Tool;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    public class StudentResetList:MonoBehaviour
    {
        void Awake()
        {
            StudentManualEvents.resetUIlist += SortAllStudentItems;
        }

        void SortAllStudentItems()
        {
            ManualDataGameComp tracker = Current.Game?.GetComponent<ManualDataGameComp>();
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);

            // 1. 获取所有子物体中的 StudentListShow 组件
            var studentItems = GetComponentsInChildren<StudentListShow>();

            // 2. 使用统一规则排序。
            //    - 收藏靠前
            //    - 已拥有靠前
            //    - 已拥有且正常状态的学生靠前，死亡灰态靠后
            //    - 同组内等级高的靠前
            //    - 最后按名字稳定排序
            var sortedItems = studentItems
                .OrderByDescending(s => s.SelfisCollect)
                .ThenByDescending(s => s.HasOwnedStudent())
                .ThenByDescending(s => s.IsStudentAvailable())
                .ThenByDescending(s => s.GetSortLevel())
                .ThenBy(s => s.BaStudentUI.StudentName)
                .ToList();

            // 3. 重新设置UI层级中的实际顺序。
            //    通过循环调用 `SetAsLastSibling()`，我们可以按照排好序的列表，
            //    依次将UI项放到父物体的最末尾，最终构建出正确的从上到下的显示顺序。
            foreach (var item in sortedItems)
            {
                item.transform.SetAsLastSibling();
            }
        }
        void OnDestroy()
        {
            StudentManualEvents.resetUIlist -= SortAllStudentItems;
        }

    }
}

