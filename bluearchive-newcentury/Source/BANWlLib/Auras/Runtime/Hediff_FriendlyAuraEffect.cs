using Verse;

namespace BANWlLib.Auras
{
    //光环受益状态由地图统一续期，离图或失去续期时自动移除。
    public sealed class Hediff_FriendlyAuraEffect : HediffWithComps
    {
        private int expiresAtTick;
        public override bool ShouldRemove => base.ShouldRemove || !pawn.Spawned || pawn.Dead
            || Find.TickManager.TicksGame > expiresAtTick;

        //设置本轮汇总强度并保留足够的续期间隔。
        public void Refresh(float strength)
        {
            Severity = strength;
            expiresAtTick = Find.TickManager.TicksGame + MapComponent_FriendlyAuras.RefreshIntervalTicks * 2;
        }

        //禁止外部同名状态合并，受益实例和强度只由光环管理器维护。
        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }

        //保存受益续期时间，读取后由所在地图重新汇总覆盖关系。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref expiresAtTick, "auraExpiresAtTick");
        }
    }
}
