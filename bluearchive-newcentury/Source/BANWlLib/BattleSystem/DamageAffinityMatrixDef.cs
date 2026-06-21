using System.Collections.Generic;
using BANWlLib.BaDef;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class DamageAffinityEntry
    {
        public damageType defenseType;
        public float multiplier = 1f;
    }

    public class DamageAffinityRow
    {
        public damageType attackType;
        public List<DamageAffinityEntry> entries = new List<DamageAffinityEntry>();
    }

    public class DamageAffinityMatrixDef : Def
    {
        public float defaultMultiplier = 1f;
        public List<DamageAffinityRow> rows = new List<DamageAffinityRow>();
    }
}
