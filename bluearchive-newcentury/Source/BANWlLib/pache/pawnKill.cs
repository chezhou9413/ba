using BANWlLib;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using HarmonyLib;
using System;
using Verse;

namespace BANWlLib.pache
{
    //Pawn 死亡补丁，负责在学生真正死亡后保存养成数据并移出已拥有名册。
    public class pawnKill
    {
        public static ManualDataGameComp tracker;

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
        public static class Patch_Pawn_Kill_DropGreenstone
        {
            //Pawn.Kill 后置处理，负责只在死亡已经成立时执行学生死亡清册。
            public static void Postfix(Pawn __instance)
            {
                try
                {
                    if (__instance != null &&
                        __instance.Dead &&
                        StudentIdentityUtility.IsConfiguredStudentKind(__instance))
                    {
                        tracker = Current.Game.GetComponent<ManualDataGameComp>();
                        StudentRosterUtility.MarkStudentDeadAndUnowned(__instance);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[BANW] 学生死亡清册处理失败：" + e);
                }
            }
        }
    }
}
