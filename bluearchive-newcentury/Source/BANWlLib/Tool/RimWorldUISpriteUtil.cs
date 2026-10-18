#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
using Verse;

namespace BANWlLib.Tool
{
    //把 Pawn 和物品渲染为界面使用的 Sprite。
    public static class RimWorldUISpriteUtil
    {
        private static readonly Dictionary<string, Sprite> GeneratedSpriteCache = new Dictionary<string, Sprite>();
#if RW_1_7
        [System.ThreadStatic] internal static bool GeneratingPreview;
#endif

        //按 PawnKindDef 生成全身预览，展示任务队列中未实例化学生的默认外观。
        public static Sprite GetSpriteFromKind(PawnKindDef kindDef, int size = 128)
        {
            if (kindDef == null) return null;

            string cacheKey = BuildKey("full-kind", kindDef.defName, size);
            if (TryGetCachedSprite(cacheKey, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Pawn pawn = null;
            try
            {
                pawn = GenerateTempPawn(kindDef);
                Sprite sprite = CaptureAndProcess(pawn, size, zoom: 0.6f, offset: new Vector3(0f, 0f, 0.15f));
                CacheSprite(cacheKey, sprite);
                return sprite;
            }
            finally
            {
                CleanupTempPawn(pawn);
            }
        }

        //按真实 Pawn 生成全身预览，保留当前发型、衣服和装备外观。
        public static Sprite GetSpriteFromPawn(Pawn pawn, int size = 128)
        {
            if (pawn == null) return null;

            string cacheKey = BuildPawnKey("full-pawn", pawn, size);
            if (TryGetCachedSprite(cacheKey, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = CaptureAndProcess(pawn, size, zoom: 0.6f, offset: new Vector3(0f, 0f, 0.15f));
            CacheSprite(cacheKey, sprite);
            return sprite;
        }

        //按 PawnKindDef 生成头像预览，展示未生成学生的默认头像。
        public static Sprite GetHeadShotSpriteFromKind(PawnKindDef kindDef, int size = 128)
        {
            if (kindDef == null) return null;

            string cacheKey = BuildKey("head-kind", kindDef.defName, size);
            if (TryGetCachedSprite(cacheKey, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Pawn pawn = null;
            try
            {
                pawn = GenerateTempPawn(kindDef);
                Sprite sprite = CaptureAndProcess(pawn, size, zoom: 2.5f, offset: new Vector3(0f, 0f, 0.4f));
                CacheSprite(cacheKey, sprite);
                return sprite;
            }
            finally
            {
                CleanupTempPawn(pawn);
            }
        }

        //按真实 Pawn 生成头像预览，保留当前穿戴状态。
        public static Sprite GetHeadShotSpriteFromPawn(Pawn pawn, int size = 128)
        {
            if (pawn == null) return null;

            string cacheKey = BuildPawnKey("head-pawn", pawn, size);
            if (TryGetCachedSprite(cacheKey, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = CaptureAndProcess(pawn, size, zoom: 2.5f, offset: new Vector3(0f, 0f, 0.4f));
            CacheSprite(cacheKey, sprite);
            return sprite;
        }

        public static Sprite GetSpriteFromThingDef(ThingDef def)
        {
            if (def == null) return null;
#if RW_1_7
            Texture2D rimTexture = Texture2D.FromNative((def.uiIcon ?? BaseContent.BadTex).CoreTexture);
#else
            Texture2D rimTexture = def.uiIcon;
            if (rimTexture == null)
            {
                rimTexture = BaseContent.BadTex;
            }
#endif
            return Sprite.Create(
                rimTexture,
                new Rect(0, 0, rimTexture.width, rimTexture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        private static Sprite CaptureAndProcess(Pawn pawn, int targetSize, float zoom, Vector3 offset)
        {
            Texture2D rawTexture = null;
            Texture2D finalTexture = null;
            int requestSize = targetSize * 4;

            try
            {
                PortraitsCache.SetDirty(pawn);
#if RW_1_7
                var renderTexture = PortraitsCache.Get(pawn, new global::UnityEngine.Vector2(requestSize, requestSize), Rot4.South,
                    new global::UnityEngine.Vector3(offset.x, offset.y, offset.z), zoom);
                var capture = new global::UnityEngine.Texture2D(renderTexture.width, renderTexture.height, global::UnityEngine.TextureFormat.RGBA32, false);
                var oldActive = global::UnityEngine.RenderTexture.active;
                try
                {
                    global::UnityEngine.RenderTexture.active = renderTexture;
                    capture.ReadPixels(new global::UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    capture.Apply();
                    rawTexture = new Texture2D(renderTexture.width, renderTexture.height);
                    rawTexture.SetPixels32(System.Array.ConvertAll(capture.GetPixels32(), p => new Color32(p.r, p.g, p.b, p.a)));
                    rawTexture.Apply();
                }
                finally { global::UnityEngine.RenderTexture.active = oldActive; global::UnityEngine.Object.Destroy(capture); }
#else
                RenderTexture renderTexture = PortraitsCache.Get(pawn, new Vector2(requestSize, requestSize), Rot4.South, offset, zoom);
                int actualWidth = renderTexture.width;
                int actualHeight = renderTexture.height;

                rawTexture = new Texture2D(actualWidth, actualHeight, TextureFormat.RGBA32, false);

                RenderTexture oldActive = RenderTexture.active;
                RenderTexture.active = renderTexture;

                try
                {
                    rawTexture.ReadPixels(new Rect(0, 0, actualWidth, actualHeight), 0, 0);
                    rawTexture.Apply();
                }
                finally
                {
                    RenderTexture.active = oldActive;
                }
#endif

                finalTexture = ProcessTextureFitHeightPriority(rawTexture, targetSize, padding: 0);

                if (finalTexture != rawTexture)
                {
                    Engine.Object.Destroy(rawTexture);
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"[RimWorldUISpriteUtil] Error: {e.Message}");
                if (rawTexture != null)
                {
                    Engine.Object.Destroy(rawTexture);
                }
                return null;
            }

            if (finalTexture != null)
            {
                return Sprite.Create(finalTexture, new Rect(0, 0, targetSize, targetSize), new Vector2(0.5f, 0.5f), 100f);
            }
            return null;
        }

        private static Texture2D ProcessTextureFitHeightPriority(Texture2D source, int targetSize, int padding)
        {
            Color32[] srcPixels = source.GetPixels32();
            int srcW = source.width;
            int srcH = source.height;

            int minX = srcW;
            int maxX = 0;
            int minY = srcH;
            int maxY = 0;
            bool hasContent = false;

            for (int i = 0; i < srcPixels.Length; i++)
            {
                if (srcPixels[i].a > 10)
                {
                    int x = i % srcW;
                    int y = i / srcW;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    hasContent = true;
                }
            }

            if (!hasContent) return new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);

            int contentW = maxX - minX + 1;
            int contentH = maxY - minY + 1;

            float availableHeight = targetSize - (padding * 2);
            float scale = availableHeight / contentH;

            int drawW = Mathf.RoundToInt(contentW * scale);
            int drawH = Mathf.RoundToInt(contentH * scale);
            int startX = (targetSize - drawW) / 2;
            int startY = (targetSize - drawH) / 2;

            Color32[] finalPixels = new Color32[targetSize * targetSize];

            for (int y = 0; y < drawH; y++)
            {
                int destY = startY + y;
                if (destY < 0 || destY >= targetSize) continue;

                for (int x = 0; x < drawW; x++)
                {
                    int destX = startX + x;
                    if (destX < 0 || destX >= targetSize) continue;

                    float srcX = minX + (x / scale);
                    float srcY = minY + (y / scale);
                    finalPixels[destY * targetSize + destX] = SampleBilinear(srcPixels, srcW, srcH, srcX, srcY);
                }
            }

            Texture2D result = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
            result.SetPixels32(finalPixels);
            result.Apply();
            return result;
        }

        //双线性采样源图像，在缩放截图时减少锯齿和模糊边缘。
        private static Color32 SampleBilinear(Color32[] pixels, int width, int height, float x, float y)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt(x), 0, width - 1);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(y), 0, height - 1);
            int x1 = Mathf.Clamp(x0 + 1, 0, width - 1);
            int y1 = Mathf.Clamp(y0 + 1, 0, height - 1);
            float tx = Mathf.Clamp01(x - x0);
            float ty = Mathf.Clamp01(y - y0);

            Color c00 = pixels[y0 * width + x0];
            Color c10 = pixels[y0 * width + x1];
            Color c01 = pixels[y1 * width + x0];
            Color c11 = pixels[y1 * width + x1];
            Color cx0 = Color.Lerp(c00, c10, tx);
            Color cx1 = Color.Lerp(c01, c11, tx);
            return Color.Lerp(cx0, cx1, ty);
        }

        public static Texture2D AutoCrop(Texture2D original, int padding = 2)
        {
            if (original == null) return null;
            return original;
        }

        public static void ClearGeneratedSpriteCache()
        {
            foreach (Sprite sprite in GeneratedSpriteCache.Values)
            {
                if (sprite == null)
                {
                    continue;
                }

                Texture2D texture = sprite.texture;
                Engine.Object.Destroy(sprite);
                if (texture != null)
                {
                    Engine.Object.Destroy(texture);
                }
            }

            GeneratedSpriteCache.Clear();
        }

        private static string BuildKey(string category, string id, int size)
        {
            return category + ":" + id + ":" + size;
        }

        //在衣服和装备变化后重新生成头像。
        private static string BuildPawnKey(string category, Pawn pawn, int size)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(category).Append(":").Append(pawn.ThingID).Append(":").Append(size);

            if (pawn.apparel?.WornApparel != null)
            {
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    builder.Append(":a=").Append(apparel.def?.defName).Append("#").Append(apparel.ThingID);
                }
            }

            if (pawn.equipment?.Primary != null)
            {
                builder.Append(":e=").Append(pawn.equipment.Primary.def?.defName).Append("#").Append(pawn.equipment.Primary.ThingID);
            }

            builder.Append(":d=").Append(pawn.Drafted);
            return builder.ToString();
        }

        private static bool TryGetCachedSprite(string key, out Sprite sprite)
        {
            if (GeneratedSpriteCache.TryGetValue(key, out sprite))
            {
                if (sprite != null)
                {
                    return true;
                }

                GeneratedSpriteCache.Remove(key);
            }

            sprite = null;
            return false;
        }

        private static void CacheSprite(string key, Sprite sprite)
        {
            if (sprite != null)
            {
                GeneratedSpriteCache[key] = sprite;
            }
        }

        //生成任务 UI 使用的临时 Pawn，给未出击学生提供默认预览图。
        private static Pawn GenerateTempPawn(PawnKindDef kindDef)
        {
#if RW_1_7
            bool previous = GeneratingPreview;
            GeneratingPreview = true;
            try { return GenerateTempPawnCore(kindDef); }
            finally { GeneratingPreview = previous; }
#else
            return GenerateTempPawnCore(kindDef);
#endif
        }

        //保留角色外观生成规则，临时实例由截图调用者回收。
        private static Pawn GenerateTempPawnCore(PawnKindDef kindDef)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                kindDef,
                faction: Find.FactionManager.OfPlayer,
                context: PawnGenerationContext.NonPlayer,
                tile: -1,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f,
                fixedBiologicalAge: null,
                fixedChronologicalAge: null,
                fixedGender: null,
                forcedXenogenes: null,
                fixedBirthName: null,
                fixedTitle: null,
                fixedIdeo: null,
                forceNoIdeo: true,
                forceNoBackstory: true,
                forceBaselinerChance: 0f,
                forbidAnyTitle: true,
                dontGiveWeapon: true,
                forceNoGear: false
            ));

            if (pawn.equipment != null)
            {
                pawn.equipment.DestroyAllEquipment();
            }
            return pawn;
        }

        private static void CleanupTempPawn(Pawn pawn)
        {
            if (pawn != null)
            {
                if (Find.WorldPawns.Contains(pawn))
                {
                    Find.WorldPawns.RemoveAndDiscardPawnViaGC(pawn);
                }
                else
                {
                    if (pawn.Spawned)
                    {
                        pawn.DeSpawn();
                    }

                    if (!pawn.Destroyed)
                    {
                        pawn.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
    }
}
