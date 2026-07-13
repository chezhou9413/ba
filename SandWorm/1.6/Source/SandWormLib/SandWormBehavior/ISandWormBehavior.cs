namespace SandWormLib.SandWormBehavior
{
    public interface ISandWormBehavior
    {
        string Id { get; }

        void Enter(SandWormBehaviorContext context);

        void Tick(SandWormBehaviorContext context);

        void Exit(SandWormBehaviorContext context);
    }
}
