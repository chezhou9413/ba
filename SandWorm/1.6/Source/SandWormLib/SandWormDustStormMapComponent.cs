using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    public sealed class SandWormDustStormMapComponent : MapComponent
    {
        private const string AbnormalSandstormDefName = "SandWorm_AbnormalSandstorm";
        private const float PitchRotation = 90f;
        private const float ScaleMultiplier = 8.5f;
        private const float HeightOffset = 24f;
        private const int WeatherCheckIntervalTicks = 30;
        private const float WeatherParticleSortingFudge = -8f;
        private const int FadeoutDestroyDelayTicks = 360;

        private GameObject instance;
        private WeatherDef abnormalSandstormDef;
        private bool lastWeatherActive;
        private bool isStopping;
        private int destroyAtTick = -1;

        public SandWormDustStormMapComponent(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            abnormalSandstormDef = DefDatabase<WeatherDef>.GetNamedSilentFail(AbnormalSandstormDefName);
            RefreshState(forceRefresh: true);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (Find.TickManager.TicksGame % WeatherCheckIntervalTicks == 0)
            {
                RefreshState(forceRefresh: false);
            }
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            DestroyInstanceImmediate();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                abnormalSandstormDef = DefDatabase<WeatherDef>.GetNamedSilentFail(AbnormalSandstormDefName);
                RefreshState(forceRefresh: true);
            }
        }

        private void RefreshState(bool forceRefresh)
        {
            WeatherDef currentWeather = map.weatherManager?.curWeather;
            bool isActive = abnormalSandstormDef != null && currentWeather == abnormalSandstormDef;

            if (!forceRefresh && isActive == lastWeatherActive)
            {
                if (isActive)
                {
                    UpdateTransform();
                }
                else
                {
                    TryFinalizeStoppedInstance();
                }
                return;
            }

            lastWeatherActive = isActive;
            if (isActive)
            {
                EnsureInstance();
                UpdateTransform();
            }
            else
            {
                BeginStopInstance();
            }
        }

        private void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            DestroyInstanceImmediate();

            GameObject prefab = SandWormAssets.ShaChenBaoPrefab;
            if (prefab == null)
            {
                Log.Error("[SandWorm] Missing prefab: " + SandWormAssets.ShaChenBaoPrefabKey);
                return;
            }

            instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = "SandWormDustStormInstance";
            instance.transform.eulerAngles = new Vector3(PitchRotation, 0f, 0f);
            instance.transform.localScale *= ScaleMultiplier;
            ConfigureRenderers(instance);
            SandWormVisualStateManager.Register(instance, map);
            isStopping = false;
            destroyAtTick = -1;
        }

        private void UpdateTransform()
        {
            if (instance == null)
            {
                return;
            }

            Vector3 center = new Vector3(map.Size.x * 0.5f, HeightOffset, map.Size.z * 0.5f);
            instance.transform.position = center;
        }

        private static void ConfigureRenderers(GameObject root)
        {
            ParticleSystemRenderer[] particleRenderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                particleRenderers[i].sortingFudge = WeatherParticleSortingFudge;
            }
        }

        private void BeginStopInstance()
        {
            if (instance == null || isStopping)
            {
                return;
            }

            isStopping = true;
            destroyAtTick = Find.TickManager.TicksGame + FadeoutDestroyDelayTicks;

            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                var emission = particleSystem.emission;
                emission.enabled = false;
                particleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void TryFinalizeStoppedInstance()
        {
            if (instance == null || !isStopping)
            {
                return;
            }

            if (destroyAtTick >= 0 && Find.TickManager.TicksGame < destroyAtTick)
            {
                return;
            }

            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                if (particleSystem != null && particleSystem.IsAlive(withChildren: true))
                {
                    return;
                }
            }

            DestroyInstanceImmediate();
        }

        // DestroyInstanceImmediate 立即清理沙尘暴 Unity 视觉实例，用于地图移除和沙虫击杀后的天气恢复。
        internal void DestroyInstanceImmediate()
        {
            if (instance != null)
            {
                SandWormVisualStateManager.Unregister(instance);
                Object.Destroy(instance);
                instance = null;
            }

            isStopping = false;
            destroyAtTick = -1;
        }
    }
}
