namespace BANWlLib.Auras
{
    //本轮光环快照负责固定一次结算中的半径与强度，避免为每个目标重复计算属性。
    public sealed class FriendlyAuraSource
    {
        public readonly HediffComp_FriendlyAura aura;
        public readonly float radius;
        public readonly float strength;

        //从有效发射组件读取本轮参数。
        public FriendlyAuraSource(HediffComp_FriendlyAura aura)
        {
            this.aura = aura;
            radius = aura.Radius;
            strength = aura.Strength;
        }
    }
}
