using RimWorld;
using Verse;

namespace AbilityShoot
{
    //射击技能动词，在施放后为施法者附加状态。
    public class Verb_AbilityShootForPlayer : Verb_AbilityShoot
    {
        //处理指定目标的施法请求。
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            // 检查当前技能是否包含目标指定组件
            // CompAbilityEffect_WithDest用于处理需要指定位置的技能效果（如传送、爆炸、召唤等）
            CompAbilityEffect_WithDest compAbilityEffect_WithDest = Ability.CompOfType<CompAbilityEffect_WithDest>();
            
            // 如果找到了目标指定组件，并且该组件的目标类型设置为"选定目标"
            if (compAbilityEffect_WithDest != null && compAbilityEffect_WithDest.Props.destination == AbilityEffectDestination.Selected)
            {
                // 设置技能效果的目标位置
                // 这种情况通常用于需要在特定位置产生效果的技能
                compAbilityEffect_WithDest.SetTarget(target);
            }
            else
            {
                // 对于纯射击技能（没有特殊位置要求的技能）
                // 直接将射击任务加入到施法者的任务队列中
                Ability.QueueCastingJob(target, null);
            }
        }

        //预热结束后执行射击并附加自身状态。
        public override void WarmupComplete()
        {
            // 首先调用基类的WarmupComplete方法
            base.WarmupComplete();

            // 获取施法者的Pawn信息
            Pawn casterPawn = (Pawn)caster;
            
            if (casterPawn == null)
            {
                return;
            }

            // 遍历当前技能的所有效果组件，查找状态效果组件
            foreach (var comp in Ability.EffectComps)
            {
                // 检查当前组件是否为"给予状态效果"类型的组件
                if (comp is CompAbilityEffect_GiveHediff giveHediffComp)
                {
                    // 从组件配置中获取要添加的状态效果定义
                    HediffDef hediffDef = giveHediffComp.Props.hediffDef;
                    
                    if (hediffDef == null)
                    {
                        continue;
                    }

                    // 检查组件配置，决定是否替换已存在的状态效果
                    bool shouldReplace = giveHediffComp.Props.replaceExisting;

                    // 如果设置了替换已存在的状态效果，先移除旧的
                    if (shouldReplace)
                    {
                        Hediff existingHediff = casterPawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                        if (existingHediff != null)
                        {
                            casterPawn.health.RemoveHediff(existingHediff);
                        }
                    }

                    // 优先使用原生方法直接添加状态效果
                    try
                    {
                        // 使用RimWorld原生方法创建和添加状态效果
                        Hediff newHediff = HediffMaker.MakeHediff(hediffDef, casterPawn);
                        
                        // 例如：newHediff.Severity = 1.0f; // 设置严重程度
                        
                        casterPawn.health.AddHediff(newHediff);
                    }
                    catch (System.Exception)
                    {
                        // 如果原生方法失败，尝试使用组件方法作为备用
                        try
                        {
                            // 创建一个指向施法者自身的目标信息
                            LocalTargetInfo selfTarget = new LocalTargetInfo(casterPawn);
                            
                            // 使用组件的Apply方法作为备用
                            giveHediffComp.Apply(selfTarget, casterPawn);
                        }
                        catch (System.Exception)
                        {
                            // 静默处理异常，不输出错误信息
                        }
                    }
                }
            }
        }
    }
}
