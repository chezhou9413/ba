using Verse;

namespace BANWlLib.CostSystem
{
    //地图COST池负责保存共享费用、按征召状态回复费用并处理无人征召清零。
    public sealed class MapComponent_BACostPool : MapComponent
    {
        //基础回复间隔为3.6个游戏秒，每秒60个tick。
        private const int BaseRecoveryTicks = 216;
        private const int NoDraftResetTicks = 180;

        private int currentCostTenths;
        private float recoveryRemainderTenths;
        private int noDraftedStudentTicks;

        public int CurrentCostTenths => currentCostTenths;
        public float CurrentCost => currentCostTenths / 10f;
        public int MaximumCost => BACostPoolService.ResolveRules(map).maximumCost;
        public int MaximumCostTenths => MaximumCost * 10;

        //构造地图组件并绑定所属地图。
        public MapComponent_BACostPool(Map map) : base(map)
        {
        }

        //每个游戏tick检查征召条件并把回复进度换算成精确十分位COST。
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            BACostRules rules = BACostPoolService.ResolveRules(map);
            ClampToMaximum(rules.maximumCost * 10);

            if (!BACostStatusUtility.HasDraftedStudent(map))
            {
                TickWithoutDraftedStudent();
                return;
            }

            noDraftedStudentTicks = 0;
            if (currentCostTenths >= rules.maximumCost * 10)
            {
                recoveryRemainderTenths = 0f;
                return;
            }

            float teamMultiplier = BACostStatusUtility.GetTeamRecoveryMultiplier(map);
            float recoveryTenthsPerTick = 10f / BaseRecoveryTicks * rules.recoveryMultiplier * teamMultiplier;
            recoveryRemainderTenths += recoveryTenthsPerTick;

            int gainedTenths = (int)recoveryRemainderTenths;
            if (gainedTenths <= 0)
            {
                return;
            }

            recoveryRemainderTenths -= gainedTenths;
            currentCostTenths += gainedTenths;
            ClampToMaximum(rules.maximumCost * 10);
        }

        //将直接回复转换为十分位后加入共享池并返回实际增加量。
        public int Grant(float amount)
        {
            int amountTenths = UnityEngine.Mathf.RoundToInt(amount * 10f);
            if (amountTenths <= 0)
            {
                return 0;
            }

            int before = currentCostTenths;
            currentCostTenths = UnityEngine.Mathf.Min(currentCostTenths + amountTenths, MaximumCostTenths);
            return currentCostTenths - before;
        }

        //提交一次已经通过校验的技能费用扣除。
        internal void Spend(int costTenths)
        {
            currentCostTenths -= costTenths;
        }

        //保存地图池的十分位费用、回复余数和无人征召计时。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentCostTenths, "currentCostTenths", 0);
            Scribe_Values.Look(ref recoveryRemainderTenths, "recoveryRemainderTenths", 0f);
            Scribe_Values.Look(ref noDraftedStudentTicks, "noDraftedStudentTicks", 0);
        }

        //连续无人征召达到三秒时清空费用与未结算回复进度。
        private void TickWithoutDraftedStudent()
        {
            noDraftedStudentTicks++;
            if (noDraftedStudentTicks < NoDraftResetTicks)
            {
                return;
            }

            currentCostTenths = 0;
            recoveryRemainderTenths = 0f;
            noDraftedStudentTicks = NoDraftResetTicks;
        }

        //任务上限变化时限制正向COST并清理满值预存回复。
        private void ClampToMaximum(int maximumTenths)
        {
            if (currentCostTenths > maximumTenths)
            {
                currentCostTenths = maximumTenths;
            }

            if (currentCostTenths >= maximumTenths)
            {
                recoveryRemainderTenths = 0f;
            }
        }
    }
}
