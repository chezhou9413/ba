using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.BaDef
{
    public class GachaPool
    {
        public float Weight = 0;
        public List<ThingDef> RaceList = new List<ThingDef>();
    }
    public class Gacha:Def
    {
        public string gachaTexPath;
        public string gachaVidPath;
        public string gachaTitle;
        public string gachaUp;
        public string gachaDesc;
        public bool isFes;

        public GachaPool oneStarPool;
        public GachaPool twoStarPool;
        public GachaPool threeStarPool;
        public GachaPool upthreeStarPool;
        public GachaPool FESupthreeStarPool;
        public GachaPool FESthreeStarPool;
    }
}
