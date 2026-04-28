using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.Pojo
{
    public class TickDamage
    {
        public int tick;
        public float damageAmount;
        public DamageDef damageType;
        public EffecterDef effecterDef;
        public float penetration = 0f;
        public bool isAttackBuilding = false;
    }
}
