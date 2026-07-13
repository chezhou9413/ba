using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SandWormLib
{
    // SandWormSyndicateChallengeSite 负责承载辛迪加直启挑战的隐藏临时地图，并在失败后清理地图与世界对象。
    public sealed class SandWormSyndicateChallengeSite : MapParent
    {
        private const string ChallengeMapGeneratorDefName = "SandWorm_SyndicateChallengeArena";
        public static readonly IntVec3 ChallengeMapSize = new IntVec3(250, 1, 250);

        private bool removeNow;

        public override MapGeneratorDef MapGeneratorDef => DefDatabase<MapGeneratorDef>.GetNamedSilentFail(ChallengeMapGeneratorDefName) ?? MapGeneratorDefOf.Encounter;

        protected override bool UseGenericEnterMapFloatMenuOption => false;

        public override string Label => "SandWorm_SyndicateChallenge_SiteLabel".Translate();

        // MarkForRemoval 负责让 MapParent 在下一次检查时销毁挑战地图和隐藏世界对象。
        public void MarkForRemoval()
        {
            removeNow = true;
            forceRemoveWorldObjectWhenMapRemoved = true;
            CheckRemoveMapNow();
        }

        // ShouldRemoveMapNow 负责把挑战结束状态转换成原版地图清理请求。
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = true;
            return removeNow;
        }

        // ExposeData 负责保存临时地图是否已经进入清理阶段。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref removeNow, "removeNow", defaultValue: false);
        }
    }
}
