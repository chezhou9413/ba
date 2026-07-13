using System.Collections.Generic;
using Verse;

namespace SandWormLib.SandWormBehavior
{
    public sealed class SandWormStateMachine
    {
        private readonly SandWormBehaviorContext context;
        private readonly Dictionary<string, SandWormBehaviorBase> behaviors = new Dictionary<string, SandWormBehaviorBase>();

        private SandWormBehaviorBase currentBehavior;

        public SandWormStateMachine(SandWormBehaviorContext context)
        {
            this.context = context;
        }

        public SandWormBehaviorBase CurrentBehavior => currentBehavior;

        public string CurrentBehaviorId => currentBehavior?.Id;

        public void Register(SandWormBehaviorBase behavior)
        {
            if (behavior == null)
            {
                return;
            }

            behaviors[behavior.Id] = behavior;
        }

        public SandWormBehaviorDef CurrentBehaviorDef => currentBehavior?.Def;

        public void SetBehavior(string behaviorId)
        {
            if (string.IsNullOrEmpty(behaviorId) || CurrentBehaviorId == behaviorId)
            {
                return;
            }

            if (!behaviors.TryGetValue(behaviorId, out SandWormBehaviorBase nextBehavior))
            {
                Log.Error("[SandWorm] Missing behavior: " + behaviorId);
                return;
            }

            currentBehavior?.Exit(context);
            currentBehavior = nextBehavior;
            context.Blackboard.LastBehaviorChangeTick = context.CurrentTick;
            context.SurfacePressureEnabled = currentBehavior.Def.EnableSurfacePressure;
            currentBehavior.Enter(context);
        }

        public void Tick()
        {
            if (currentBehavior == null)
            {
                return;
            }

            currentBehavior.Tick(context);
            SandWormBehaviorTransition transition = currentBehavior.EvaluateTransition(context);
            if (transition.HasTransition)
            {
                SetBehavior(transition.NextBehaviorId);
            }
        }

        public void Shutdown()
        {
            currentBehavior?.Exit(context);
            currentBehavior = null;
        }
    }
}
