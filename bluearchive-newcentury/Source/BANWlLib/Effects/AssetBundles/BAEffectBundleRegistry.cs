using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace BANWlLib.Effects.AssetBundles
{
    // 特效 AB 注册表，负责按 texPath 加载贴图、创建材质并按空闲时间释放资源。
    public static class BAEffectBundleRegistry
    {
        private const int CleanupIntervalTicks = 300;
        private const int IdleReleaseTicks = 600;
        private static readonly bool EnableRuntimeIdleUnload = true;

        private static readonly Dictionary<string, BAEffectBundleRecord> records = new Dictionary<string, BAEffectBundleRecord>(StringComparer.OrdinalIgnoreCase);
        private static readonly FieldInfo shaderParameterNameField = typeof(ShaderParameter).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo shaderParameterTypeField = typeof(ShaderParameter).GetField("type", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo shaderParameterValueField = typeof(ShaderParameter).GetField("value", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo shaderParameterValueTexField = typeof(ShaderParameter).GetField("valueTex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static int lastCleanupTick = int.MinValue / 2;

        // 获取材质，负责从 AB 加载贴图并按 shader 参数创建材质。
        public static Material AcquireMaterial(GraphicRequest request, string owner)
        {
            if (!BAEffectPathUtility.IsEffectTexPath(request.path))
            {
                Log.Error("[BA特效AB] 非 Effect 路径不能加载 AB：" + request.path);
                return BaseContent.BadMat;
            }

            BAEffectBundleRecord record = GetOrLoadRecord(request.path);
            if (record == null || record.texture == null)
            {
                return BaseContent.BadMat;
            }

            Touch(record);

            string materialKey = BuildMaterialKey(request);
            if (!record.materials.TryGetValue(materialKey, out Material material) || material == null)
            {
                material = new Material(request.shader)
                {
                    name = "BAEffect_" + request.path,
                    mainTexture = record.texture,
                    color = request.color
                };

                if (request.renderQueue > 0)
                {
                    material.renderQueue = request.renderQueue;
                }

                ApplyShaderParameters(material, request.shaderParameters);
                record.materials[materialKey] = material;
            }

            return material;
        }

        // 标记贴图正在使用，负责让清理器识别近期绘制过的特效。
        public static void TouchTexPath(string texPath)
        {
            if (string.IsNullOrWhiteSpace(texPath))
            {
                return;
            }

            if (!records.TryGetValue(texPath, out BAEffectBundleRecord record))
            {
                return;
            }

            Touch(record);
        }

        // 定期清理，负责释放空闲超过阈值的 AB、贴图和材质。
        public static void CleanupIfNeeded()
        {
            if (!EnableRuntimeIdleUnload)
            {
                return;
            }

            if (Find.TickManager == null)
            {
                return;
            }

            int now = Find.TickManager.TicksGame;
            if (now - lastCleanupTick < CleanupIntervalTicks)
            {
                return;
            }

            lastCleanupTick = now;
            List<string> removeKeys = new List<string>();
            foreach (KeyValuePair<string, BAEffectBundleRecord> pair in records)
            {
                BAEffectBundleRecord record = pair.Value;
                if (now - record.lastTouchTick < IdleReleaseTicks)
                {
                    continue;
                }

                DestroyRecord(record);
                removeKeys.Add(pair.Key);
            }

            foreach (string key in removeKeys)
            {
                records.Remove(key);
            }
        }

        // 重置全部缓存，负责在静态构造或必要清理时彻底释放资源。
        public static void ResetAll()
        {
            foreach (BAEffectBundleRecord record in records.Values)
            {
                DestroyRecord(record);
            }

            records.Clear();
            lastCleanupTick = int.MinValue / 2;
        }

        // 加载或复用记录，负责创建 AB、读取包内贴图并记录路径。
        private static BAEffectBundleRecord GetOrLoadRecord(string texPath)
        {
            CleanupIfNeeded();
            if (records.TryGetValue(texPath, out BAEffectBundleRecord existing))
            {
                Touch(existing);
                return existing;
            }

            string bundlePath = BAEffectPathUtility.GetBundlePath(texPath);
            if (!File.Exists(bundlePath))
            {
                Log.Error("[BA特效AB] 缺少 AB 文件：" + bundlePath);
                return null;
            }

            string assetPath = BAEffectPathUtility.GetAssetPathFromManifest(bundlePath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Log.Error("[BA特效AB] AssetBundle.LoadFromFile 失败：" + bundlePath);
                return null;
            }

            Texture2D texture = bundle.LoadAsset<Texture2D>(assetPath);
            if (texture == null)
            {
                bundle.Unload(true);
                Log.Error("[BA特效AB] 包内找不到贴图资源：" + assetPath + " in " + bundlePath);
                return null;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            BAEffectBundleRecord record = new BAEffectBundleRecord
            {
                texPath = texPath,
                bundlePath = bundlePath,
                assetPath = assetPath,
                bundle = bundle,
                texture = texture
            };
            Touch(record);
            records[texPath] = record;
            return record;
        }

        // 材质键生成，负责区分同一贴图在不同 shader 参数下的材质实例。
        private static string BuildMaterialKey(GraphicRequest request)
        {
            string key = request.path + "|" + (request.shader != null ? request.shader.name : "null") + "|" + request.color + "|" + request.colorTwo + "|" + request.renderQueue;
            if (request.shaderParameters == null)
            {
                return key;
            }

            foreach (ShaderParameter parameter in request.shaderParameters)
            {
                key += "|" + BuildShaderParameterKey(parameter);
            }

            return key;
        }

        // Shader 参数键生成，负责把 RimWorld 的非公开参数字段稳定写入材质缓存键。
        private static string BuildShaderParameterKey(ShaderParameter parameter)
        {
            if (parameter == null)
            {
                return "null";
            }

            object name = shaderParameterNameField?.GetValue(parameter);
            object type = shaderParameterTypeField?.GetValue(parameter);
            object value = shaderParameterValueField?.GetValue(parameter);
            object valueTex = shaderParameterValueTexField?.GetValue(parameter);
            string valueTexName = valueTex is Texture2D texture ? texture.name : string.Empty;
            return name + ":" + type + ":" + value + ":" + valueTexName;
        }

        // Shader 参数应用，负责把 XML 中的 shaderParameters 写入 AB 材质。
        private static void ApplyShaderParameters(Material material, List<ShaderParameter> parameters)
        {
            if (material == null || parameters == null)
            {
                return;
            }

            foreach (ShaderParameter parameter in parameters)
            {
                parameter.Apply(material);
            }
        }

        // 触碰记录，负责更新时间戳。
        private static void Touch(BAEffectBundleRecord record)
        {
            if (record == null)
            {
                return;
            }

            record.lastTouchTick = GetCurrentTickSafe();
        }

        // 当前时间读取，负责在 Def 图标解析和长事件阶段避开未初始化的 TickManager。
        private static int GetCurrentTickSafe()
        {
            try
            {
                if (Current.Game?.tickManager != null)
                {
                    return Current.Game.tickManager.TicksGame;
                }
            }
            catch (NullReferenceException)
            {
                return 0;
            }

            return 0;
        }

        // 销毁记录，负责彻底释放 Unity 对象和 AB。
        private static void DestroyRecord(BAEffectBundleRecord record)
        {
            if (record == null)
            {
                return;
            }

            foreach (Material material in record.materials.Values)
            {
                if (material != null)
                {
                    UnityEngine.Object.Destroy(material);
                }
            }

            record.materials.Clear();
            if (record.bundle != null)
            {
                record.bundle.Unload(true);
            }

            record.bundle = null;
            record.texture = null;
        }
    }
}
