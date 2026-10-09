using UnityEngine;
using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //引导旋转子特效，负责延迟生成、实时瞄准、位置偏移与工作结束后的资源释放。
    public class SubEffecter_ChannelRotatedMote : SubEffecter
    {
        private Pawn caster;
        private Job channelJob;
        private TargetInfo focus;
        private int dueTick;
        private bool triggered;
        private float rotation;
        private Mote_ChannelEffect mote;

        //保存子特效定义和所属控制器，生成实例只使用本次运行数据。
        public SubEffecter_ChannelRotatedMote(SubEffecterDef def, Effecter parent) : base(def, parent) { }

        //记录本次来源与延迟时刻，妮露二段绑定引导工作，其他入口保持普通定时播放。
        public override void SubTrigger(TargetInfo source, TargetInfo target, int overrideSpawnTick = -1, bool force = false)
        {
            caster = source.Thing as Pawn;
            if (caster == null)
            {
                Log.Error("[BANW] 引导旋转特效的来源必须是角色");
                return;
            }
            var driver = caster.jobs.curDriver as JobDriver_NeroExChannel;
            channelJob = driver?.job;
            focus = target;
            dueTick = (driver?.ChannelStartTick ?? Find.TickManager.TicksGame) + def.initialDelayTicks;
            rotation = def.rotation.RandomInRange;
            triggered = true;
            SubEffectTick(source, target);
        }

        //执行到期生成并更新正在引导的实例，已经结束的工作不能继续生成延迟特效。
        public override void SubEffectTick(TargetInfo source, TargetInfo target)
        {
            if (!triggered) return;
            if (!caster.Spawned || caster.Dead || (channelJob != null && caster.CurJob != channelJob))
            {
                SubCleanup();
                return;
            }
            if (mote == null && Find.TickManager.TicksGame >= dueTick) SpawnMote();
            if (mote == null || mote.Destroyed) return;
            if (channelJob != null) UpdateAim();
            mote.Maintain();
        }

        //生成独立引导Mote，几何与颜色来自当前子节点，生命周期由绑定工作控制。
        private void SpawnMote()
        {
            mote = ThingMaker.MakeThing(def.moteDef) as Mote_ChannelEffect;
            if (mote == null)
            {
                triggered = false;
                Log.Error("[BANW] 引导旋转特效的moteDef必须使用Mote_ChannelEffect：" + def.moteDef.defName);
                return;
            }
            mote.caster = caster;
            mote.channelJob = channelJob;
            mote.Scale = def.scale.RandomInRange * parent.scale;
            mote.instanceColor = EffectiveColor;
            mote.yOffset = EffectiveOffset.y;
            UpdateAim();
            GenSpawn.Spawn(mote, caster.Position, caster.Map);
            //生成流程会应用Mote自身偏移，随后统一按实时瞄准几何定位。
            UpdateAim();
            mote.Maintain();
        }

        //使用工作目标计算连续角度，旋转贴图和位置偏移而不依赖角色四向朝向。
        private void UpdateAim()
        {
            Vector3 target = channelJob != null ? channelJob.targetA.CenterVector3 : focus.CenterVector3;
            float angle = def.absoluteAngle ? 0f : (target - caster.DrawPos).Yto0().AngleFlat();
            mote.exactPosition = caster.DrawPos + EffectiveOffset.RotatedBy(angle) + def.moteDef.mote.attachedDrawOffset;
            mote.exactRotation = rotation + angle;
        }

        //停止未生成节点，引导模式销毁本节点Mote，普通播放保留其自身结束时间。
        public override void SubCleanup()
        {
            triggered = false;
            if (channelJob != null && mote != null && !mote.Destroyed) mote.Destroy();
            mote = null;
        }
    }
}
