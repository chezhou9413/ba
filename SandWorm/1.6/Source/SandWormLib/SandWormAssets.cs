using ChezhouLib.ALLmap;
using UnityEngine;

namespace SandWormLib
{
    public static class SandWormAssets
    {
        public const string RuchongPrefabKey = "SandWorm.Ruchong";
        public const string ShaChenBaoPrefabKey = "Sshachenbao";

        public static GameObject RuchongPrefab
        {
            get
            {
                abDatabase.prefabDataBase.TryGetValue(RuchongPrefabKey, out GameObject prefab);
                return prefab;
            }
        }

        public static GameObject ShaChenBaoPrefab
        {
            get
            {
                abDatabase.prefabDataBase.TryGetValue(ShaChenBaoPrefabKey, out GameObject prefab);
                return prefab;
            }
        }
    }
}
