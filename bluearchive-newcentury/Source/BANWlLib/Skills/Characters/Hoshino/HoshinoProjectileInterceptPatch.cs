using HarmonyLib;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //原版弹丸拦截入口，负责把星野前向护盾加入实体弹丸飞行判定。
    [HarmonyPatch(typeof(Projectile), "CheckForFreeInterceptBetween")]
    public static class HoshinoProjectileInterceptPatch
    {
        //在原版墙体和其他护盾判断之前检查前向半圆。
        public static bool Prefix(Projectile __instance, Vector3 lastExactPos, Vector3 newExactPos, ref bool __result)
        {
            if (!HoshinoInterceptor.TryIntercept(__instance, lastExactPos, newExactPos)) return true;
            __result = true;
            return false;
        }
    }
}

