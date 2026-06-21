using newpro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.comp
{
    public class DropPosCompProperties : CompProperties
    {
        public DropPosCompProperties()
        {
            // 修复：必须指向对应的 ThingComp 实现类，不能指向 CompProperties
            this.compClass = typeof(DropPosComp);
        }
    }

    public class DropPosComp : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Map map = parent.Map;
            if (map == null || !parent.Spawned) return;

            var comp = map.GetComponent<MapComponent_EveryFrame>();
            if (comp != null)
            {
                comp.dropCell = parent.Position;
            }
        }
    }
}
