using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimUnity;
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

            var studentItems = GetComponentsInChildren<StudentListShow>();

            //按学生排序规则排列。
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

            //同步 UI 层级顺序。
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

