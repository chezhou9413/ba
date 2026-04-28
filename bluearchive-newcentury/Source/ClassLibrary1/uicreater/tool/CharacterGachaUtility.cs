using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace newpro
{
    public static class CharacterGachaUtility
    {
        private const float ONE_STAR_PROBABILITY = 78.5f;
        private const float TWO_STAR_PROBABILITY = 18.5f;
        private const float THREE_STAR_PROBABILITY = 3.0f;

        // 修改：UP 三星占三分之一的总三星概率
        private const float UP_THREE_STAR_RATIO = 0.3f; // 即占 1.5%

        private static System.Random random = new System.Random();

        private static Dictionary<string, string> characterLabelCache = new Dictionary<string, string>();

        public static string SinglePull(UIbody uiBody)
        {
            if (uiBody == null)
                return "无效的角色池";

            if (IsCharacterPoolEmpty(uiBody))
                return "角色池为空";

            int rarity = GetRandomRarity();

            switch (rarity)
            {
                case 1:
                    return GetRandomCharacter(uiBody.OneStar);
                case 2:
                    return GetRandomCharacter(uiBody.twoStar);
                case 3:
                    return GetThreeStarCharacter(uiBody);
                default:
                    return "抽卡错误";
            }
        }

        public static List<string> TenPull(UIbody uiBody)
        {
            List<string> results = new List<string>();

            if (uiBody == null || IsCharacterPoolEmpty(uiBody))
            {
                results.Add("无效的角色池或角色池为空");
                return results;
            }

            for (int i = 0; i < 10; i++)
            {
                results.Add(SinglePull(uiBody));
            }

            return results;
        }

        public static List<string> MultiPull(UIbody uiBody, int count)
        {
            List<string> results = new List<string>();

            if (uiBody == null || IsCharacterPoolEmpty(uiBody) || count <= 0)
            {
                results.Add("无效的参数");
                return results;
            }

            for (int i = 0; i < count; i++)
            {
                results.Add(SinglePull(uiBody));
            }

            return results;
        }

        private static bool IsCharacterPoolEmpty(UIbody uiBody)
        {
            bool hasOneStar = uiBody.OneStar != null && uiBody.OneStar.Count > 0;
            bool hasTwoStar = uiBody.twoStar != null && uiBody.twoStar.Count > 0;
            bool hasThreeStar = uiBody.ThreeStar != null && uiBody.ThreeStar.Count > 0;

            return !(hasOneStar || hasTwoStar || hasThreeStar);
        }

        private static int GetRandomRarity()
        {
            float roll = (float)random.NextDouble() * 100;

            if (roll < ONE_STAR_PROBABILITY)
                return 1;
            else if (roll < ONE_STAR_PROBABILITY + TWO_STAR_PROBABILITY)
                return 2;
            else
                return 3;
        }

        private static string GetRandomCharacter(List<string> characterPool)
        {
            if (characterPool == null || characterPool.Count == 0)
                return "角色池为空";

            int index = random.Next(characterPool.Count);
            return characterPool[index];
        }

        private static string GetThreeStarCharacter(UIbody uiBody)
        {
            List<string> pool = new List<string>();
            List<float> weights = new List<float>();

            float upWeightTotal = THREE_STAR_PROBABILITY * UP_THREE_STAR_RATIO;           // 1.5%
            float normalWeightTotal = THREE_STAR_PROBABILITY * (1f - UP_THREE_STAR_RATIO); // 1.5%

            if (uiBody.UPThreeStar != null && uiBody.UPThreeStar.Count > 0)
            {
                int count = uiBody.UPThreeStar.Count;
                foreach (var up in uiBody.UPThreeStar)
                {
                    pool.Add(up);
                    weights.Add(upWeightTotal / count);
                }
            }

            if (uiBody.ThreeStar != null)
            {
                var normalList = uiBody.ThreeStar
                    .Where(def => uiBody.UPThreeStar == null || !uiBody.UPThreeStar.Contains(def))
                    .ToList();

                int count = normalList.Count;
                foreach (var normal in normalList)
                {
                    pool.Add(normal);
                    weights.Add(count > 0 ? (normalWeightTotal / count) : 0);
                }
            }

            if (pool.Count == 0)
                return "3星角色池为空";

            return WeightedRandom(pool, weights);
        }

        private static string WeightedRandom(List<string> options, List<float> weights)
        {
            float totalWeight = weights.Sum();
            float roll = (float)(random.NextDouble() * totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < options.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return options[i];
            }

            return options.Last();
        }

        private static string GetCharacterLabel(string defName)
        {
            if (characterLabelCache.ContainsKey(defName))
                return characterLabelCache[defName];

            ThingDef thingDef = DefDatabase<ThingDef>.GetNamed(defName, false);
            if (thingDef != null && !string.IsNullOrEmpty(thingDef.label))
            {
                characterLabelCache[defName] = thingDef.label;
                return thingDef.label;
            }

            if (defName.StartsWith("BANW_") && defName.EndsWith("_race"))
            {
                string characterName = defName.Substring(5, defName.Length - 10);
                characterLabelCache[defName] = characterName;
                return characterName;
            }

            int lastUnderscoreIndex = defName.LastIndexOf('_');
            if (lastUnderscoreIndex > 0)
            {
                string extracted = defName.Substring(0, lastUnderscoreIndex);
                int firstUnderscoreIndex = extracted.IndexOf('_');
                if (firstUnderscoreIndex > 0)
                {
                    extracted = extracted.Substring(firstUnderscoreIndex + 1);
                }

                if (!string.IsNullOrEmpty(extracted))
                {
                    characterLabelCache[defName] = extracted;
                    return extracted;
                }
            }

            characterLabelCache[defName] = defName;
            return defName;
        }

        public static string GetProbabilityInfo()
        {
            return $"抽卡概率：\n" +
                   $"1星：{ONE_STAR_PROBABILITY}%\n" +
                   $"2星：{TWO_STAR_PROBABILITY}%\n" +
                   $"3星：{THREE_STAR_PROBABILITY}%（其中 UP 三星占比 {UP_THREE_STAR_RATIO * 100}%）";
        }

        public static string GenerateBlueArchiveStylePoolText(UIbody uiBody)
        {
            if (uiBody == null || IsCharacterPoolEmpty(uiBody))
                return "【无效的角色池】";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("━━━━【招募详情】━━━━");
            sb.AppendLine();

            if (uiBody.UPThreeStar != null && uiBody.UPThreeStar.Count > 0)
            {
                sb.AppendLine("★★★ 【UP！概率提升！】");
                foreach (var character in uiBody.UPThreeStar)
                {
                    sb.AppendLine($"▲ {GetCharacterLabel(character)} ▲");
                }
                sb.AppendLine($"UP角色在3星中占比: {UP_THREE_STAR_RATIO * 100}%");
                sb.AppendLine();
            }

            if (uiBody.ThreeStar != null && uiBody.ThreeStar.Count > 0)
            {
                sb.AppendLine("★★★ 【3星角色】");
                int count = 0;
                foreach (var character in uiBody.ThreeStar)
                {
                    if (count > 0 && count % 3 == 0)
                        sb.AppendLine();
                    sb.Append($"{GetCharacterLabel(character)} | ");
                    count++;
                }
                sb.AppendLine();
                sb.AppendLine($"3星总概率: {THREE_STAR_PROBABILITY}%");
                sb.AppendLine();
            }

            if (uiBody.twoStar != null && uiBody.twoStar.Count > 0)
            {
                sb.AppendLine("★★ 【2星角色】");
                int count = 0;
                foreach (var character in uiBody.twoStar)
                {
                    if (count > 0 && count % 4 == 0)
                        sb.AppendLine();
                    sb.Append($"{GetCharacterLabel(character)} | ");
                    count++;
                }
                sb.AppendLine();
                sb.AppendLine($"2星角色出现概率: {TWO_STAR_PROBABILITY}%");
                sb.AppendLine();
            }

            if (uiBody.OneStar != null && uiBody.OneStar.Count > 0)
            {
                sb.AppendLine("★ 【1星角色】");
                int count = 0;
                foreach (var character in uiBody.OneStar)
                {
                    if (count > 0 && count % 5 == 0)
                        sb.AppendLine();
                    sb.Append($"{GetCharacterLabel(character)} | ");
                    count++;
                }
                sb.AppendLine();
                sb.AppendLine($"1星角色出现概率: {ONE_STAR_PROBABILITY}%");
            }

            sb.AppendLine();
            sb.AppendLine("━━━━【抽卡说明】━━━━");
            sb.AppendLine("1. 单抽消耗120枚青辉石");
            sb.AppendLine("2. 十连抽消耗1200枚青辉石");
            sb.AppendLine("3. 抽卡获得的角色不会重复");
            sb.AppendLine("4. 池子将于活动结束后关闭");

            return sb.ToString();
        }
    }
}
