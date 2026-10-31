using System.Collections.Generic;
using BANWlLib.BaClass;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //爱丽丝EX沿原有时间表播放特效，让准备表现与光束使用同一瞄准方向。
    internal sealed class ArisuExEffects
    {
        private readonly Pawn caster;
        private readonly int startTick;
        private readonly Effecter effect;
        private readonly bool[] triggered;
        private readonly List<(SubEffecterDef def, Mote mote, float rotation)> motes =
            new List<(SubEffecterDef, Mote, float)>();

        //独立创建本次特效状态，不修改共享EffecterDef或Mote定义。
        internal ArisuExEffects(EffecterDef def, Pawn caster, int startTick)
        {
            this.caster = caster;
            this.startTick = startTick;
            effect = def?.Spawn();
            triggered = new bool[effect?.children.Count ?? 0];
        }

        //延迟节点到点才生成，已经生成的旋转节点随当前瞄准角更新。
        internal void Tick(LocalTargetInfo target)
        {
            if (effect == null) return;
            int now = Find.TickManager.TicksGame;
            float angle = SpecialDirectionalEffects.AimAngle(caster, target);
            for (int i = 0; i < effect.children.Count; i++)
            {
                SubEffecter child = effect.children[i];
                SubEffecterDef def = child.def;
                if (child is SubEffecter_SprayerTriggeredRotatedOffset)
                {
                    if (!triggered[i] && now >= startTick + def.initialDelayTicks)
                    {
                        triggered[i] = true;
                        var mote = (Mote)ThingMaker.MakeThing(def.moteDef);
                        float rotation = def.rotation.RandomInRange;
                        mote.Scale = def.scale.RandomInRange;
                        Position(mote, def, rotation, angle);
                        GenSpawn.Spawn(mote, caster.Position, caster.Map);
                        mote.ForceSpawnTick(startTick + def.initialDelayTicks);
                        motes.Add((def, mote, rotation));
                    }
                }
                else
                {
                    if (!triggered[i])
                    {
                        triggered[i] = true;
                        //读档重建只恢复画面，不重播已经播放过的音效。
                        if (now == startTick || !(child is SubEffecter_SoundTriggered))
                            child.SubTrigger(caster, target.ToTargetInfo(caster.Map));
                    }
                    child.SubEffectTick(caster, target.ToTargetInfo(caster.Map));
                }
            }
            foreach (var entry in motes)
                if (!entry.mote.Destroyed) Position(entry.mote, entry.def, entry.rotation, angle);
        }

        //位置偏移和贴图方向共用瞄准角，保持原有固定地图轴配置。
        private void Position(Mote mote, SubEffecterDef def, float rotation, float angle)
        {
            float facing = def.absoluteAngle ? 0f : angle;
            mote.exactPosition = caster.Position.ToVector3Shifted() + def.positionOffset.RotatedBy(facing);
            mote.exactRotation = rotation + facing;
        }

        //准备被打断时清掉表现，已发射光束保留Mote本身的播放尾段。
        internal void Cleanup(bool cancelMotes)
        {
            effect?.Cleanup();
            if (cancelMotes)
                foreach (var entry in motes)
                    if (!entry.mote.Destroyed) entry.mote.Destroy();
            motes.Clear();
        }
    }
}
