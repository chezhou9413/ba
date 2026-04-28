using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using System;
using Verse.AI;

namespace BANWlLib
{
    /// <summary>
    /// 道具使用效果组件配置类
    /// 这个类定义了道具使用时对HumanIntPropertyComp的影响
    /// 可以在XML文件中配置这些属性
    /// </summary>
    public class CompProperties_UseEffectUpdateCustomInt : CompProperties_UseEffect
    {
        // 使用道具时增加或减少的数值（正数增加，负数减少）
        public int updateValue = 0;
        
        // 检测使用者的种族，留空为所有种族都能使用
        // 常见种族：Human（人类）、Muffalo（毛牛）、Thrumbo（敲击兽）等
        public string Race = "";
        
        // 是否检查最大值限制
        public bool checkMaxLimit = true;
        
        // 是否检查最小值限制
        public bool checkMinLimit = true;

        // 批量使用的数量
        public List<int> batchUseCount = new List<int>();


        /// <summary>
        /// 构造函数
        /// 告诉游戏这个配置对应哪个组件类
        /// </summary>
        public CompProperties_UseEffectUpdateCustomInt()
        {
            this.compClass = typeof(CompUseEffect_UpdateCustomInt);
        }
    }

    /// <summary>
    /// 道具使用效果组件
    /// 当道具被使用时，这个组件会修改使用者的HumanIntPropertyComp中的自定义属性值
    /// </summary>
    public class CompUseEffect_UpdateCustomInt : CompUseEffect
    {
        /// <summary>
        /// 获取组件配置属性
        /// 这些配置来自XML文件中的设置
        /// </summary>
        public CompProperties_UseEffectUpdateCustomInt Props => (CompProperties_UseEffectUpdateCustomInt)this.props;

        /// <summary>
        /// 安全获取角色的种族名称
        /// </summary>
        /// <param name="pawn">要检查的角色</param>
        /// <returns>种族名称，如果无法获取则返回"Unknown"</returns>
        private string GetRaceDefName(Pawn pawn)
        {
            try
            {
                // 直接获取种族定义名称
                if (pawn.def != null)
                {
                    return pawn.def.defName;
                }
                
                // 如果无法获取种族信息，返回未知
                return "Unknown";
            }
            catch
            {
                // 如果出现任何错误，返回错误标识
                return "ERR";
            }
        }

        /// <summary>
        /// 提供右键菜单选项
        /// 当玩家右键点击道具时显示批量使用选项
        /// 这个方法会在玩家右键点击道具时被游戏自动调用
        /// </summary>
        /// <param name="selPawn">选中的角色（即要使用道具的角色）</param>
        /// <returns>右键菜单选项列表（每个选项都是一个FloatMenuOption对象）</returns>
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            // 第一步：先返回基础的菜单选项
            // 这确保了我们不会覆盖掉原有的菜单选项（比如"使用"选项）
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(selPawn))
            {
                yield return option; // yield return 表示逐个返回选项，而不是一次性返回所有选项
            }

            // 第二步：检查是否配置了批量使用功能
            // 如果XML中没有配置batchUseCount，或者配置的列表为空，就不显示批量使用菜单
            if (Props.batchUseCount == null || Props.batchUseCount.Count == 0)
            {
                yield break; // yield break 表示提前结束，不再返回更多选项
            }

            // 第三步：检查角色是否可以使用这个道具
            // 这里会检查种族限制、属性值限制等条件
            AcceptanceReport canUse = CanBeUsedBy(selPawn);
            if (!canUse.Accepted) // 如果不能使用
            {
                yield break; // 直接结束，不显示批量使用选项
            }

            // 第四步：为每个配置的批量数量创建菜单选项
            foreach (int batchCount in Props.batchUseCount)
            {
                // 检查道具数量是否足够批量使用
                if (parent.stackCount < batchCount) // parent.stackCount 是当前道具的堆叠数量
                {
                    // 数量不足时跳过这个数量，继续处理下一个
                    continue;
                }

                // 数量足够时创建可点击的批量使用选项
                yield return new FloatMenuOption(
                    $"批量使用 x{batchCount}", // 显示的文本，例如"批量使用 x5"
                    delegate() // 这是一个匿名委托（lambda表达式的另一种写法）
                    {
                        CreateBatchUseJob(selPawn, batchCount);
                    },
                    MenuOptionPriority.Default, // 菜单选项的优先级
                    null, // mouseoverGuiAction：鼠标悬停时的GUI动作
                    null, // revalidateClickTarget：重新验证点击目标
                    0f,   // extraPartWidth：额外部分的宽度
                    null, // extraPartOnGUI：额外部分的GUI绘制
                    null  // revalidateWorldClickTarget：重新验证世界点击目标
                );
            }
        }

        

    

        /// <summary>
        /// 道具使用时的效果
        /// 当玩家使用道具时，游戏会调用这个方法
        /// 支持批量使用：检查当前任务的count值来决定使用数量
        /// </summary>
        /// <param name="usedBy">使用道具的角色</param>
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            
            // 获取使用者身上的HumanIntPropertyComp组件
            HumanIntPropertyComp humanComp = usedBy.GetComp<HumanIntPropertyComp>();
            if (humanComp == null)
            {
                return; // 如果没有组件就不执行效果
            }
            
            // 检查是否是批量使用（通过检查当前任务的count值）
            int useCount = 1; // 默认使用1个
            if (usedBy.CurJob != null && usedBy.CurJob.count > 1)
            {
                useCount = usedBy.CurJob.count;
            }
            
            // 确保不超过实际拥有的道具数量
            useCount = Math.Min(useCount, parent.stackCount);
            
            // 计算批量使用的总效果
            int originalValue = humanComp.CustomIntValue;
            int totalEffect = Props.updateValue * useCount;
            int newValue = originalValue + totalEffect;
            
            // 应用数值限制检查
            if (Props.checkMaxLimit && humanComp.Props != null && newValue > humanComp.Props.maxValue)
            {
                newValue = humanComp.Props.maxValue;
            }
            
            if (Props.checkMinLimit && humanComp.Props != null && newValue < humanComp.Props.minValue)
            {
                newValue = humanComp.Props.minValue;
            }
            
            // 计算实际生效的数量
            int actualEffect = newValue - originalValue;
            int actualCount = actualEffect == 0 ? 0 : 
                (Props.updateValue > 0 ? 
                    (actualEffect + Props.updateValue - 1) / Props.updateValue : 
                    (actualEffect - Props.updateValue + 1) / Props.updateValue);
            
            // 确保实际使用数量不超过请求数量和拥有数量
            actualCount = Math.Min(actualCount, useCount);
            actualCount = Math.Min(actualCount, parent.stackCount);
            actualCount = Math.Max(actualCount, 0);
            
            if (actualCount > 0)
            {
                // 设置新值
                humanComp.SetValue(originalValue + Props.updateValue * actualCount);
                
                // 显示效果消息给玩家
                if (actualCount == 1)
                {
                    Messages.Message($"{usedBy.LabelShort} 使用了 {parent.LabelShort}", 
                        usedBy, MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    Messages.Message($"{usedBy.LabelShort} 批量使用了 {actualCount} 个 {parent.LabelShort}", 
                        usedBy, MessageTypeDefOf.NeutralEvent);
                }
                
                // 销毁使用的道具
                if (parent.stackCount > actualCount)
                {
                    parent.stackCount -= actualCount;
                }
                else
                {
                    parent.Destroy();
                }
            }
            else
            {
                // 如果属性值已经达到限制，显示提示消息
                if (Props.updateValue > 0)
                {
                    Messages.Message($"{usedBy.LabelShort} 的属性值已达到最大值，无法使用", 
                        usedBy, MessageTypeDefOf.RejectInput);
                }
                else
                {
                    Messages.Message($"{usedBy.LabelShort} 的属性值已达到最小值，无法使用", 
                        usedBy, MessageTypeDefOf.RejectInput);
                }
            }
        }

        /// <summary>
        /// 检查是否可以使用道具
        /// 在玩家尝试使用道具时，游戏会调用这个方法来检查是否允许使用
        /// </summary>
        /// <param name="p">要使用道具的角色</param>
        /// <returns>是否可以使用道具</returns>
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            // 检查基础使用条件
            AcceptanceReport baseResult = base.CanBeUsedBy(p);
            if (!baseResult.Accepted)
            {
                return baseResult;
            }
            
            // 检查种族限制
            if (!string.IsNullOrEmpty(Props.Race))
            {
                string currentRace = GetRaceDefName(p);
                if (currentRace == "ERR")
                {
                    return new AcceptanceReport("无法识别种族，无法使用道具");
                }
                if (currentRace != Props.Race)
                {
                    return new AcceptanceReport($"此道具只能由 {Props.Race} 种族使用，当前为 {currentRace}");
                }
            }

            // 检查使用者是否有HumanIntPropertyComp组件
            HumanIntPropertyComp humanComp = p.GetComp<HumanIntPropertyComp>();
            if (humanComp == null)
            {
                return new AcceptanceReport("无法使用");
            }

            // 检查是否可以应用效果
            int currentValue = humanComp.CustomIntValue;
            int newValue = currentValue + Props.updateValue;
            
            // 检查最大值限制
            if (Props.checkMaxLimit && humanComp.Props != null && newValue > humanComp.Props.maxValue)
            {
                if (Props.updateValue > 0)
                {
                    return new AcceptanceReport($"属性值已达到最大值 ({humanComp.Props.maxValue})");
                }
            }
            
            // 检查最小值限制
            if (Props.checkMinLimit && humanComp.Props != null && newValue < humanComp.Props.minValue)
            {
                if (Props.updateValue < 0)
                {
                    return new AcceptanceReport($"属性值已达到最小值 ({humanComp.Props.minValue})");
                }
            }

            return AcceptanceReport.WasAccepted;
        }

        /// <summary>
        /// 创建批量使用任务
        /// 让角色走到道具处进行批量使用（更真实的模拟）
        /// </summary>
        /// <param name="usedBy">使用者</param>
        /// <param name="count">使用数量</param>
        private void CreateBatchUseJob(Pawn usedBy, int count)
        {
            try
            {
                // 检查基本条件
                if (usedBy == null || parent == null)
                {
                    return;
                }

                // 检查角色是否能到达道具
                if (!usedBy.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                {
                    return;
                }

                // 检查道具是否可以被预约
                if (!usedBy.CanReserve(parent))
                {
                    return;
                }

                // 检查角色是否可以使用这个道具
                AcceptanceReport canUse = CanBeUsedBy(usedBy);
                if (!canUse.Accepted)
                {
                    return;
                }

                // 检查是否有新的使用任务，如果有则取消当前任务
                if (HasNewUseJob(usedBy))
                {
                    CancelCurrentUseJob(usedBy);
                    return;
                }

                // 通过DefDatabase查找UseItem任务定义
                JobDef useItemJob = DefDatabase<JobDef>.GetNamed("UseItem");
                if (useItemJob == null)
                {
                    return;
                }

                // 创建使用道具的工作任务
                Job useJob = JobMaker.MakeJob(useItemJob, parent);
                useJob.count = Math.Min(count, parent.stackCount); // 确保不超过实际数量
                
                // 预约道具，防止其他角色同时使用
                if (usedBy.Reserve(parent, useJob))
                {
                    // 给角色分配这个任务，优先级设为立即执行
                    usedBy.jobs.TryTakeOrderedJob(useJob, JobTag.Misc);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[批量使用] CreateBatchUseJob 错误: {ex}");
            }
        }

        /// <summary>
        /// 检查是否有新的使用任务
        /// </summary>
        /// <param name="pawn">角色</param>
        /// <returns>是否有新的使用任务</returns>
        private bool HasNewUseJob(Pawn pawn)
        {
            try
            {
                // 检查角色的当前任务队列
                if (pawn.jobs == null || pawn.jobs.jobQueue == null)
                    return false;

                // 检查是否有新的UseItem任务在队列中
                foreach (var queuedJob in pawn.jobs.jobQueue)
                {
                    if (queuedJob.job != null && queuedJob.job.def != null)
                    {
                        // 如果发现新的UseItem任务且目标不是当前道具，说明有新任务
                        if (queuedJob.job.def.defName == "UseItem" && queuedJob.job.targetA.Thing != parent)
                        {
                            return true;
                        }
                    }
                }

                // 检查角色是否收到了新的使用命令
                if (pawn.jobs.curJob != null && pawn.jobs.curJob.def != null)
                {
                    // 如果当前任务是UseItem但目标不是当前道具，说明有新任务
                    if (pawn.jobs.curJob.def.defName == "UseItem" && pawn.jobs.curJob.targetA.Thing != parent)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[批量使用] HasNewUseJob 错误: {ex}");
                return false;
            }
        }

        /// <summary>
        /// 取消当前的使用任务
        /// </summary>
        /// <param name="pawn">角色</param>
        private void CancelCurrentUseJob(Pawn pawn)
        {
            try
            {
                // 取消当前任务
                if (pawn.jobs.curJob != null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }

                // 清空所有任务队列（最安全的方法）
                if (pawn.jobs.jobQueue != null)
                {
                    pawn.jobs.jobQueue.Clear(pawn, false);
                }

                // 释放对当前道具的预约
                if (pawn.Map != null && pawn.Map.reservationManager != null)
                {
                    pawn.Map.reservationManager.Release(parent, pawn, pawn.jobs.curJob);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[批量使用] CancelCurrentUseJob 错误: {ex}");
            }
        }
    }
}
