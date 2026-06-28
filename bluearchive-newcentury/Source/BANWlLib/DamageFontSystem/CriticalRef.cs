using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BANWlLib.DamageFontSystem
{
    [DefOf]
    public class CriticalRef
    {
        public static StatDef BANW_CriticalChance;
        public static StatDef BANW_CriticalDamage;
        public static StatDef BANW_CriticalChanceResistance;
        public static StatDef BANW_CriticalDamageResistance;
        public static StatDef IncomingDamageFactor;
        static CriticalRef()
        {
        }
    }
}
