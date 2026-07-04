namespace BANWlLib.BattleMovement
{
    // 战斗击退配置，负责描述位移技能命中目标后的推开距离和阵营过滤。
    public class BattleMovementKnockbackConfig
    {
        public bool enabled = false;
        public int distance = 0;
        public float speed = 0.35f;
        public bool affectHostile = true;
        public bool affectFriendly = false;
    }
}
