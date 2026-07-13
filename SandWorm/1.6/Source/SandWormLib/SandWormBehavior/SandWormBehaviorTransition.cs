namespace SandWormLib.SandWormBehavior
{
    public readonly struct SandWormBehaviorTransition
    {
        public static readonly SandWormBehaviorTransition None = new SandWormBehaviorTransition(null);

        public SandWormBehaviorTransition(string nextBehaviorId)
        {
            NextBehaviorId = nextBehaviorId;
        }

        public string NextBehaviorId { get; }

        public bool HasTransition => !string.IsNullOrEmpty(NextBehaviorId);
    }
}
