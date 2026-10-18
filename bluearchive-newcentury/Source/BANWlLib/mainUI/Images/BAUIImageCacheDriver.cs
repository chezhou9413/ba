#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif

namespace BANWlLib.mainUI.Images
{
    //按真实时间维护闲置缓存，不依赖地图 Tick 或游戏速度。
    public sealed class BAUIImageCacheDriver : MonoBehaviour
    {
        private static BAUIImageCacheDriver instance;
        private float nextTrim;

        //在首次绑定图片时创建唯一的缓存维护对象。
        internal static void Ensure()
        {
            if (instance != null) return;
            GameObject driver = new GameObject("BA UI图片内存管理");
#if !RW_1_7
            Object.DontDestroyOnLoad(driver);
#endif
            instance = driver.AddComponent<BAUIImageCacheDriver>();
        }

        //每秒回收过期缓存，避免每帧排序与内存统计。
        private void Update()
        {
            if (Time.realtimeSinceStartup < nextTrim) return;
            nextTrim = Time.realtimeSinceStartup + 1f;
            BAUIImageCache.Trim(false);
        }
    }
}
