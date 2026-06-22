using BANWlLib.Pojo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.Pojo
{
   public class PendingDamageAction: IExposable
    {
        public int fireAtTick; // 在 Toil 的第几 tick 触发
        public DamageSetting setting;

        public void ExposeData()
        {
           Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
           Scribe_Deep.Look(ref setting, "setting");
        }
    }

    public class PendingHediffAction : IExposable
    {
        public int fireAtTick; // 在 Toil 的第几 tick 触发
        public SelfHediffSetting setting;

        public void ExposeData()
        {
            Scribe_Values.Look(ref fireAtTick, "fireAtTick", 0);
            Scribe_Deep.Look(ref setting, "setting");
        }
    }
}
