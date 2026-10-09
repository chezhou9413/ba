using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //喷射参数职责：复制原版表现配置并叠加命中方向，不修改共享的子特效定义。
    public static class SpecialImpactSprayerConfig
    {
        //复制职责：保留粒子数量、颜色、尺寸、运动和延迟，禁用会再次覆盖弹道角度的目标朝向选项。
        public static SubEffecterDef Create(SubEffecterDef source, float angle, float rotationOffset)
        {
            return new SubEffecterDef
            {
                subEffecterClass = source.subEffecterClass,
                burstCount = source.burstCount, ticksBetweenMotes = source.ticksBetweenMotes,
                maxMoteCount = source.maxMoteCount, initialDelayTicks = source.initialDelayTicks,
                lifespanMaxTicks = source.lifespanMaxTicks, chancePerTick = source.chancePerTick,
                chancePeriodTicks = source.chancePeriodTicks, spawnLocType = source.spawnLocType,
                positionLerpFactor = source.positionLerpFactor,
                positionOffset = source.positionOffset.RotatedBy(angle),
                positionRadius = source.positionRadius, positionRadiusMin = source.positionRadiusMin,
                positionDimensions = source.positionDimensions, avoidLastPositionRadius = source.avoidLastPositionRadius,
                moteDef = source.moteDef, fleckDef = source.fleckDef, color = source.color,
                angle = new FloatRange(source.angle.min + angle, source.angle.max + angle),
                absoluteAngle = true, fleckUsesAngleForVelocity = source.fleckUsesAngleForVelocity,
                speed = source.speed,
                rotation = new FloatRange(source.rotation.min + angle + rotationOffset,
                    source.rotation.max + angle + rotationOffset),
                rotationRate = source.rotationRate, scale = source.scale, airTime = source.airTime,
                soundDef = source.soundDef, intermittentSoundInterval = source.intermittentSoundInterval,
                ticksBeforeSustainerStart = source.ticksBeforeSustainerStart,
                orbitOrigin = source.orbitOrigin, orbitSpeed = source.orbitSpeed,
                orbitSnapStrength = source.orbitSnapStrength, makeMoteOnSubtrigger = source.makeMoteOnSubtrigger,
                destroyMoteOnCleanup = source.destroyMoteOnCleanup, cameraShake = source.cameraShake,
                distanceAttenuationScale = source.distanceAttenuationScale,
                distanceAttenuationMax = source.distanceAttenuationMax,
                randomWeight = source.randomWeight, subTriggerOnSpawn = source.subTriggerOnSpawn,
                children = source.children
            };
        }
    }
}
