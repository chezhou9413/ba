using BANWlLib.BattleSystem;
using System.Collections.Generic;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能攻击段，负责配置弹丸、范围、连射和基础战斗参数。
    public class SpecialAttackConfig : BattleActionConfig
    {
        public ThingDef projectileDef;
        public float radius;
        public int shots = 1;
        public int shotIntervalTicks = 6;
        public SoundDef shotSound;
        public List<int> shotTicks;

        //读取本发相对技能生效的tick，未指定逐发表时使用统一间隔。
        public int ShotDelay(int index) => shotTicks == null ? 1 + index * shotIntervalTicks : shotTicks[index];

        //检查逐发表与发数一致，时间从生效后第1 tick起且不能逆序。
        public IEnumerable<string> TimingErrors()
        {
            if (shotTicks == null) yield break;
            if (shotTicks.Count != shots) yield return "shotTicks条目数必须与shots一致";
            for (int i = 0; i < shotTicks.Count; i++)
                if (shotTicks[i] < 1 || (i > 0 && shotTicks[i] < shotTicks[i - 1]))
                    yield return "shotTicks必须不小于1，并按发射时间顺序排列";
        }

        //保存脱手攻击段的完整配置，供存档后的弹丸和延迟攻击继续使用。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref shots, "shots", 1);
            Scribe_Values.Look(ref shotIntervalTicks, "shotIntervalTicks", 6);
            Scribe_Defs.Look(ref shotSound, "shotSound");
            Scribe_Collections.Look(ref shotTicks, "shotTicks", LookMode.Value);
        }
    }
}
