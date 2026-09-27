using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    //战斗场地组件配置，负责指定生成实体和持续时间覆盖值。
    public class CompProperties_AbilitySpawnBattleField : CompProperties_AbilityEffect
    {
        public ThingDef fieldThingDef;
        public int durationTicksOverride = -1;

        //绑定实际创建战斗场地的能力组件。
        public CompProperties_AbilitySpawnBattleField()
        {
            compClass = typeof(CompAbilityEffect_SpawnBattleField);
        }
    }

    //战斗场地能力组件，负责创建、初始化场地并通知角色技能模块。
    public class CompAbilityEffect_SpawnBattleField : CompAbilityEffect
    {
        //读取当前组件的场地配置。
        public new CompProperties_AbilitySpawnBattleField Props
        {
            get
            {
                return (CompProperties_AbilitySpawnBattleField)props;
            }
        }

        //在技能指定位置建立场地，再连接依赖场地生命周期的角色技能。
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            if (pawn == null || pawn.Map == null || Props.fieldThingDef == null)
            {
                return;
            }

            Thing thing = ThingMaker.MakeThing(Props.fieldThingDef);
            Thing_BattleFieldController controller = thing as Thing_BattleFieldController;
            if (controller == null)
            {
                Log.Error($"场地控制器 {Props.fieldThingDef.defName} 不是 Thing_BattleFieldController");
                return;
            }

            GenSpawn.Spawn(controller, target.Cell, pawn.Map);
            controller.Setup(pawn, Props.durationTicksOverride);
            Skills.KeiFieldBinding.NotifySpawned(parent, controller);
        }
    }
}
