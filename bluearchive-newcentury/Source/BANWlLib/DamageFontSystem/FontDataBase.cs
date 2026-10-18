#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif

namespace BANWlLib.DamageFontSystem
{
    public static class FontDataBase
    {
        public static GameObject Canvas;
        public static GameObject CriticalFont;
    }
}
