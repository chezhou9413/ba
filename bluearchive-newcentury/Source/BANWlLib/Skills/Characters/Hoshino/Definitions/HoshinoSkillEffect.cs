using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //星野单个动作的声音与自身特效，延迟子特效由地图维护至播放完成。
    public class HoshinoSkillEffect
    {
        public EffecterDef effecter;
        public SoundDef sound;
        public int maintainTicks = 60;

        //保留角色引用，使附着特效能跟随角色移动。
        public void Play(Pawn pawn)
        {
            sound?.PlayOneShot(new TargetInfo(pawn));
            if (effecter == null) return;
            Effecter instance = effecter.Spawn();
            instance.Trigger(pawn, pawn);
            pawn.Map.effecterMaintainer.AddEffecterToMaintain(instance, pawn, pawn,
                Mathf.Max(maintainTicks, SpecialEffects.DurationTicks(effecter)));
        }
    }
}
