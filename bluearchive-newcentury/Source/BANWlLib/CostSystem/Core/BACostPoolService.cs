using System;
using BANWlLib.MissionRunTime;
using RimWorld;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST池服务负责解析地图规则并提供查询、回复、校验和原子扣费入口。
    public static class BACostPoolService
    {
        private static readonly BACostRules DefaultRules = new BACostRules();

        //获取地图上的共享COST池。
        public static MapComponent_BACostPool GetPool(Map map)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            return map.GetComponent<MapComponent_BACostPool>();
        }

        //读取地图当前COST值。
        public static float GetCurrentCost(Map map)
        {
            return GetPool(map).CurrentCost;
        }

        //向地图共享池直接回复指定COST并返回实际增加值。
        public static float Grant(Map map, float amount)
        {
            return GetPool(map).Grant(amount) / 10f;
        }

        //计算技能经过全部状态修正后的最终费用。
        public static AbilityCostCalculation GetEffectiveCost(Ability ability)
        {
            return AbilityCostCalculator.Calculate(ability);
        }

        //检查技能当前是否能从所在地图的共享池支付费用。
        public static bool CanSpend(Ability ability, out string reason)
        {
            AbilityCostCalculation calculation = AbilityCostCalculator.Calculate(ability);
            return CanSpend(ability, calculation, out reason);
        }

        //在最终施放点重新检查、扣除费用并消费所有参与计算的限次减费。
        public static bool TrySpend(Ability ability, out string reason)
        {
            AbilityCostCalculation calculation = AbilityCostCalculator.Calculate(ability);
            if (!CanSpend(ability, calculation, out reason))
            {
                return false;
            }

            if (calculation == null)
            {
                return true;
            }

            MapComponent_BACostPool pool = GetPool(ability.pawn.Map);
            pool.Spend(calculation.EffectiveCost * 10);
            calculation.ConsumeDiscountUses();
            return true;
        }

        //根据活动任务与地图引用解析10点或20点规则。
        public static BACostRules ResolveRules(Map map)
        {
            if (map == null || Find.World == null)
            {
                return DefaultRules;
            }

            BaMissionManager manager = Find.World.GetComponent<BaMissionManager>();
            if (manager?.activeMissions == null)
            {
                return DefaultRules;
            }

            for (int index = 0; index < manager.activeMissions.Count; index++)
            {
                BaMissionRunTimeAction mission = manager.activeMissions[index];
                if (mission != null && mission.map == map && mission.state == MissionState.Active)
                {
                    return mission.def?.costRules ?? DefaultRules;
                }
            }

            return DefaultRules;
        }

        //使用已经生成的费用快照判断普通支付或透支支付是否成立。
        private static bool CanSpend(Ability ability, AbilityCostCalculation calculation, out string reason)
        {
            reason = null;
            if (calculation == null)
            {
                return true;
            }

            Pawn pawn = ability?.pawn;
            if (!BACostStatusUtility.IsEligibleDraftedStudent(pawn) || pawn.Map == null)
            {
                reason = "COST技能只能由已征召、存活的玩家学生使用。";
                return false;
            }

            MapComponent_BACostPool pool = GetPool(pawn.Map);
            int costTenths = calculation.EffectiveCost * 10;
            int currentTenths = pool.CurrentCostTenths;
            if (currentTenths >= 0 && currentTenths >= costTenths)
            {
                return true;
            }

            int overdraftLimitTenths = BACostStatusUtility.GetOverdraftLimitTenths(pawn);
            if (overdraftLimitTenths > 0 && currentTenths - costTenths >= -overdraftLimitTenths)
            {
                return true;
            }

            reason = currentTenths < 0
                ? "共享COST处于负值，只有拥有COST过载的学生可以使用EX技能。"
                : "共享COST不足，需要 " + calculation.EffectiveCost + " 点。";
            return false;
        }
    }
}
