using BANWlLib;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.Tool;
using HarmonyLib;
using System;
using Verse;

namespace BANWlLib.pache
{
    public class pawnKill
    {
        public static ManualDataGameComp tracker;

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
        public static class Patch_Pawn_Kill_DropGreenstone
        {
            public static void Postfix(Pawn __instance)
            {
                try
                {
                    // ✅ 关键修复：只有学生真正死亡时才移除
                    // 这样可以防止不死机制触发时学生被错误移除
                    if (__instance.Dead)
                    {
                        HumanIntPropertyComp humanIntProperty = __instance.GetComp<HumanIntPropertyComp>();
                        if (humanIntProperty != null)
                        {
                            tracker = Current.Game.GetComponent<ManualDataGameComp>();
                            pawnUtils.setStudentSave(__instance, tracker);
                            tracker.HaveStudent.RemoveAll(student => __instance.def.defName == student.DefName);
                            tracker.StudentCollect.RemoveAll(defName => __instance.def.defName == defName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[GreenstoneDrop] error in Pawn.Kill Postfix: {e}");
                }
            }
        }
    }
}
