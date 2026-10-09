using Verse;
using Verse.AI;

namespace BANWlLib.Skills
{
    //引导特效实例，负责让绑定工作的Mote保持可见并在工作失效时结束。
    public class Mote_ChannelEffect : MoteAttached
    {
        public Pawn caster;
        public Job channelJob;

        //判断施法者是否仍在原地图执行本次引导工作。
        private bool ChannelActive => caster != null && caster.Spawned && !caster.Dead &&
            caster.Map == Map && caster.CurJob == channelJob;

        //绑定引导时使用工作生命周期，普通触发时沿用Mote定义的播放时间。
        protected override bool EndOfLife => channelJob != null ? !ChannelActive : base.EndOfLife;

        //引导期间保持可见，普通触发时保留原版淡入淡出配置。
        public override float Alpha => channelJob != null && ChannelActive ? 1f : base.Alpha;

        //在Mote自身更新之前完成维护，避免工作与地图实体的更新顺序导致提前消失。
        protected override void TimeInterval(float deltaTime)
        {
            if (channelJob != null && ChannelActive) Maintain();
            base.TimeInterval(deltaTime);
        }
    }
}
