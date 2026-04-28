using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.comp
{
    public class HediffCompProps_Regeneration : HediffCompProperties
    {
        // 每次治疗的量
        public float healAmount = 0.1f;

        // 治疗的间隔 (in Ticks)
        public int healIntervalTicks = 60;

        public bool isHeatScar = true;
        public HediffCompProps_Regeneration()
        {
            this.compClass = typeof(Hediff_Regeneration);
        }
    }

    public class Hediff_Regeneration : HediffComp
    {
        // 缓存我们的配置属性，避免每次Tick都去获取，提高性能
        private HediffCompProps_Regeneration _props;

        public HediffCompProps_Regeneration Props => (HediffCompProps_Regeneration)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // 使用从Props中读取的配置值，而不是硬编码的常量
            if (Pawn.IsHashIntervalTick(Props.healIntervalTicks))
            {
                if (Props.isHeatScar)
                {
                    Hediff_Injury permanent = GetPermanentWound();
                    if (permanent != null)
                    {
                        // 使用与普通治疗一致的幅度逐步修复
                        // 注意：某些版本中永久性伤口可能不受自然愈合影响，但 Heal 仍可降低严重度
                        permanent.Heal(Props.healAmount);
                        return;
                    }
                }
                Hediff_Injury woundToHeal = GetTreatableWound();

                if (woundToHeal != null)
                {
                    // 使用从Props中读取的配置值
                    woundToHeal.Heal(Props.healAmount);
                }
            }
        }
        private Hediff_Injury GetPermanentWound()
        {
            List<Hediff> allHediffs = Pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff in allHediffs)
            {
                if (hediff is Hediff_Injury injury)
                {
                    // 仅治疗“疤痕类”永久性伤口：要求永久，且具备 HediffComp_GetsPermanent（表明属于会形成疤痕的旧伤类型）
                    // 并且部位未缺失。这样可排除断肢及非疤痕的永久性损伤（如脑损伤等特殊定义）。
                    if (injury.IsPermanent() && injury.TryGetComp<HediffComp_GetsPermanent>() != null)
                    {
                        if (injury.Part == null || !Pawn.health.hediffSet.PartIsMissing(injury.Part))
                        {
                            return injury;
                        }
                    }
                }
            }
            return null;
        }

        private Hediff_Injury GetTreatableWound()
        {
            // 1. 不再使用 GetHediffs<T>()，而是获取最基础的 hediffs 列表。
            //    这是一个包含了所有健康状况（不仅仅是伤口）的列表。
            List<Hediff> allHediffs = Pawn.health.hediffSet.hediffs;

            // 2. 遍历这个泛用的列表
            foreach (Hediff hediff in allHediffs)
            {
                // 3. 首先，检查当前的 hediff 是不是一个伤口 (Hediff_Injury)
                //    我们只对伤口感兴趣
                if (hediff is Hediff_Injury injury)
                {
                    // 4. 检查伤口是否不是永久性的 (即不是疤痕)
                    //    IsPermanent() 是一个很早就存在的方法，应该可用
                    if (!injury.IsPermanent())
                    {
                        // 5. 这是旧版本中判断是否需要治疗的核心逻辑：
                        //    a. 尝试获取伤口上的“可照料”组件 (TendDuration)
                        HediffComp_TendDuration tendComp = injury.TryGetComp<HediffComp_TendDuration>();

                        //    b. 如果这个组件存在，并且伤口当前“未被照料”(IsTended 是 false)
                        //       那么它就是一个可以治疗的伤口。
                        if (tendComp != null && !tendComp.IsTended)
                        {
                            // 找到了一个！立即返回它。
                            return injury;
                        }
                    }
                }
            }

            // 6. 如果遍历完所有 hediffs 都没有找到符合条件的，返回 null
            return null;
        }
    }
}
