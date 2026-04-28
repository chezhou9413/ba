using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.DamageFontSystem
{
   public class FontDef:Def
    {
        public List<DamageDef> DisableCritical = new List<DamageDef>();
        public List<DamageDef> EnsureCritical = new List<DamageDef>();
        public List<DamageDef> DisableIncomingDamageFactorCritical = new List<DamageDef>();
    }
}
