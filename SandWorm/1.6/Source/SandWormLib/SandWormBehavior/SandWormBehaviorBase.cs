namespace SandWormLib.SandWormBehavior
{
    public abstract class SandWormBehaviorBase : ISandWormBehavior
    {
        protected SandWormBehaviorBase(SandWormBehaviorDef def)
        {
            Def = def;
        }

        public string Id => Def.Id;

        public SandWormBehaviorDef Def { get; }

        public virtual void Enter(SandWormBehaviorContext context)
        {
        }

        public abstract void Tick(SandWormBehaviorContext context);

        public virtual void Exit(SandWormBehaviorContext context)
        {
        }

        public virtual SandWormBehaviorTransition EvaluateTransition(SandWormBehaviorContext context)
        {
            return SandWormBehaviorTransition.None;
        }
    }
}
