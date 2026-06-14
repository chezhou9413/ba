using BANWlLib.BaDef;
using RimWorld;
using System.Linq;
using Verse;

namespace BANWlLib.Tool
{
    /// <summary>
    /// 学生身份工具，负责按当前 BaStudentDef、studentId 和 PawnKindDef 解析学生身份。
    /// </summary>
    public static class StudentIdentityUtility
    {
        /// <summary>
        /// 从 Pawn 获取学生身份，负责通过 PawnKindDef 找到当前学生 ID。
        /// </summary>
        public static string GetStudentId(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            return GetStudentId(pawn.kindDef);
        }

        /// <summary>
        /// 从 PawnKindDef 获取学生身份，负责把绑定的 BaStudentDef 转成当前学生 ID。
        /// </summary>
        public static string GetStudentId(PawnKindDef kindDef)
        {
            if (kindDef == null)
            {
                return null;
            }

            BaStudentDef studentDef = DefDatabase<BaStudentDef>.AllDefsListForReading
                .FirstOrDefault(def => def != null && def.kindDef == kindDef);
            if (studentDef != null)
            {
                return GetStudentId(studentDef);
            }

            return kindDef.defName;
        }

        /// <summary>
        /// 从学生数据 Def 获取学生身份，负责优先使用当前 studentId 字段。
        /// </summary>
        public static string GetStudentId(BaStudentDef studentDef)
        {
            if (studentDef == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(studentDef.studentId))
            {
                return studentDef.studentId;
            }

            if (studentDef.kindDef != null)
            {
                return studentDef.kindDef.defName;
            }

            if (!string.IsNullOrEmpty(studentDef.defName))
            {
                return studentDef.defName;
            }

            return null;
        }

        /// <summary>
        /// 从当前学生 Def 名称或学生 ID 获取学生身份，负责支持当前 XML 的两种新写法。
        /// </summary>
        public static string GetStudentId(string studentDefNameOrStudentId)
        {
            if (string.IsNullOrEmpty(studentDefNameOrStudentId))
            {
                return studentDefNameOrStudentId;
            }

            BaStudentDef studentDef;
            if (TryGetStudentDef(studentDefNameOrStudentId, out studentDef))
            {
                return GetStudentId(studentDef);
            }

            return studentDefNameOrStudentId;
        }

        /// <summary>
        /// 尝试解析学生数据 Def，负责支持当前 studentId、BaStudentDef.defName 和 PawnKindDef.defName。
        /// </summary>
        public static bool TryGetStudentDef(string studentDefNameOrStudentId, out BaStudentDef studentDef)
        {
            studentDef = null;
            if (string.IsNullOrEmpty(studentDefNameOrStudentId))
            {
                return false;
            }

            studentDef = DefDatabase<BaStudentDef>.GetNamedSilentFail(studentDefNameOrStudentId);
            if (studentDef != null)
            {
                return true;
            }

            studentDef = DefDatabase<BaStudentDef>.AllDefsListForReading.FirstOrDefault(def =>
                def != null &&
                (def.studentId == studentDefNameOrStudentId ||
                 def.kindDef?.defName == studentDefNameOrStudentId));

            return studentDef != null;
        }

        /// <summary>
        /// 尝试解析 PawnKindDef，负责按当前学生数据或 PawnKindDef.defName 查找。
        /// </summary>
        public static bool TryGetPawnKindDef(string studentDefNameOrStudentId, out PawnKindDef kindDef)
        {
            kindDef = null;
            if (string.IsNullOrEmpty(studentDefNameOrStudentId))
            {
                return false;
            }

            BaStudentDef studentDef;
            if (TryGetStudentDef(studentDefNameOrStudentId, out studentDef) && studentDef.kindDef != null)
            {
                kindDef = studentDef.kindDef;
                return true;
            }

            kindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(studentDefNameOrStudentId);
            if (kindDef != null)
            {
                return true;
            }

            return kindDef != null;
        }

        /// <summary>
        /// 判断 Pawn 是否属于已配置学生 Kind，负责区分同一种族下的真实学生和非学生 Kind。
        /// </summary>
        public static bool IsConfiguredStudentKind(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            return IsConfiguredStudentKind(pawn.kindDef);
        }

        /// <summary>
        /// 判断 PawnKindDef 是否被学生数据 Def 显式绑定，负责作为星级系统的 Kind 级开关。
        /// </summary>
        public static bool IsConfiguredStudentKind(PawnKindDef kindDef)
        {
            if (kindDef == null)
            {
                return false;
            }

            return DefDatabase<BaStudentDef>.AllDefsListForReading.Any(def => def != null && def.kindDef == kindDef);
        }

        /// <summary>
        /// 获取学生显示名称，负责统一新学生 Def 和 PawnKind 的名称回退顺序。
        /// </summary>
        public static string GetStudentLabel(BaStudentDef studentDef, PawnKindDef kindDef)
        {
            if (!string.IsNullOrEmpty(studentDef?.label))
            {
                return studentDef.label;
            }

            if (!string.IsNullOrEmpty(kindDef?.label))
            {
                return kindDef.label;
            }

            return GetStudentId(studentDef) ?? GetStudentId(kindDef);
        }
    }
}
