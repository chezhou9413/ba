using AlienRace;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BANWlLib.Tool
{
    public static class RimWorldUISpriteUtil
    {
        private static readonly Dictionary<string, Sprite> GeneratedSpriteCache = new Dictionary<string, Sprite>();

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

        public static Sprite GetSpriteFromPawn(Pawn pawn, int size = 128)
        {
            if (pawn == null) return null;

            string cacheKey = BuildKey("full-pawn", pawn.ThingID, size);
            if (TryGetCachedSprite(cacheKey, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = CaptureAndProcess(pawn, size, zoom: 0.6f, offset: new Vector3(0f, 0f, 0.15f));
            CacheSprite(cacheKey, sprite);
            return sprite;
        }

        public static Sprite GetHeadShotSpriteFromDef(ThingDef_AlienRace raceDef, int size = 128)
        {
            if (raceDef == null) return null;
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(k => k.race == raceDef);
            return GetHeadShotSpriteFromKind(kindDef, size);
        }

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

        public static Sprite GetHeadShotSpriteFromPawn(Pawn pawn, int size = 128)
        {
            if (pawn == null) return null;

            string cacheKey = BuildKey("head-pawn", pawn.ThingID, size);
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
            Texture2D rimTexture = def.uiIcon;
            if (rimTexture == null)
            {
                rimTexture = BaseContent.BadTex;
            }
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
            int requestSize = targetSize * 2;

            try
            {
                PortraitsCache.SetDirty(pawn);
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

                finalTexture = ProcessTextureFitHeightPriority(rawTexture, targetSize, padding: 0);

                if (finalTexture != rawTexture)
                {
                    UnityEngine.Object.Destroy(rawTexture);
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"[RimWorldUISpriteUtil] Error: {e.Message}");
                if (rawTexture != null)
                {
                    UnityEngine.Object.Destroy(rawTexture);
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
                    int sx = Mathf.Clamp(Mathf.RoundToInt(srcX), 0, srcW - 1);
                    int sy = Mathf.Clamp(Mathf.RoundToInt(srcY), 0, srcH - 1);

                    finalPixels[destY * targetSize + destX] = srcPixels[sy * srcW + sx];
                }
            }

            Texture2D result = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
            result.SetPixels32(finalPixels);
            result.Apply();
            return result;
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
                UnityEngine.Object.Destroy(sprite);
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }

            GeneratedSpriteCache.Clear();
        }

        private static string BuildKey(string category, string id, int size)
        {
            return category + ":" + id + ":" + size;
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

        private static Pawn GenerateTempPawn(PawnKindDef kindDef)
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
                forceBaselinerChance: 0f,
                forbidAnyTitle: true
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
                    pawn.Discard();
                }
            }
        }
    }
}
