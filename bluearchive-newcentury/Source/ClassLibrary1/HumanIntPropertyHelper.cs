using Verse;
using RimWorld;
using System.Collections.Generic;

namespace BANWlLib
{
    // 人类自定义整数属性辅助类
    // 提供一些方便的方法来操作角色的自定义属性
    public static class HumanIntPropertyHelper
    {
        // 获取指定角色的自定义整数属性值
        public static int GetCustomIntValue(Pawn pawn)
        {
            if (pawn == null) return -1;
            
            // 获取自定义属性组件（支持所有种族）
            HumanIntPropertyComp comp = pawn.TryGetComp<HumanIntPropertyComp>();
            if (comp == null) return -1;
            
            return comp.CustomIntValue;
        }
        
        // 设置指定角色的自定义整数属性值
        public static bool SetCustomIntValue(Pawn pawn, int value)
        {
            if (pawn == null) return false;
            
            // 获取自定义属性组件（支持所有种族）
            HumanIntPropertyComp comp = pawn.TryGetComp<HumanIntPropertyComp>();
            if (comp == null) return false;
            
            comp.SetValue(value);
            return true;
        }
        
        // 增加指定角色的自定义整数属性值
        public static bool IncreaseCustomIntValue(Pawn pawn, int amount)
        {
            if (pawn == null) return false;
            
            // 获取自定义属性组件（支持所有种族）
            HumanIntPropertyComp comp = pawn.TryGetComp<HumanIntPropertyComp>();
            if (comp == null) return false;
            
            comp.IncreaseValue(amount);
            return true;
        }
        
        // 减少指定角色的自定义整数属性值
        public static bool DecreaseCustomIntValue(Pawn pawn, int amount)
        {
            if (pawn == null) return false;
            
            // 获取自定义属性组件（支持所有种族）
            HumanIntPropertyComp comp = pawn.TryGetComp<HumanIntPropertyComp>();
            if (comp == null) return false;
            
            comp.DecreaseValue(amount);
            return true;
        }
        
        // 检查指定角色是否有自定义整数属性组件
        public static bool HasCustomIntProperty(Pawn pawn)
        {
            if (pawn == null) return false;
            
            // 检查是否有组件（支持所有种族）
            return pawn.TryGetComp<HumanIntPropertyComp>() != null;
        }
        
        // 获取指定角色的自定义属性组件
        public static HumanIntPropertyComp GetCustomIntPropertyComp(Pawn pawn)
        {
            if (pawn == null) return null;
            
            // 直接返回组件（支持所有种族）
            return pawn.TryGetComp<HumanIntPropertyComp>();
        }
        
        // 获取所有有自定义属性的角色
        public static List<Pawn> GetAllHumansWithCustomProperty()
        {
            var result = new List<Pawn>();
            
            // 遍历所有地图上的角色
            foreach (Map map in Find.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns)
                {
                    if (HasCustomIntProperty(pawn))
                    {
                        result.Add(pawn);
                    }
                }
            }
            
            return result;
        }
    }
} 