using BANWlLib.BaDef;
using newpro;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BANWlLib.BANWGamecomp
{
    public class Gamecomp_gakaAction : GameComponent
    {
        // 当前最终展示给玩家的卡池 (包含随机的4个 + 固定的x个)
        public List<Gacha> CurrentDisplayPool = new List<Gacha>();

        // 记录上一期随机生成的卡 (用于去重，不包含固定卡)
        public List<Gacha> LastGeneratedRandomPool = new List<Gacha>();

        public GachaSetting gachaSetting;
        public int RotationTickCounter = 0;

        // 新增：记录卡池更新了多少次，用于触发特殊队列
        public int TotalRefreshCount = 0;
        private const float LimitedPoolChance = 0.15f;

        public int gacaPoit = 0;
        public Gamecomp_gakaAction(Game game)
        {
        }

        /// <summary>
        /// 获取当期的非固定卡池（即随机生成的卡）
        /// </summary>
        public List<Gacha> GetCurrentRandomPool()
        {
            return LastGeneratedRandomPool?.ToList() ?? new List<Gacha>();
        }
        public bool updataGacaPoit(int value) {
            if (value >= 0)
            {
                gacaPoit += value;
                return true;
            }
            int cost = -value; // 转为正数
            if (cost > gacaPoit)
            {
                return false; // 积分不足
            }
            gacaPoit -= cost;
            return true;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref gacaPoit, "gacaPoit", 0);
            // 保存当前展示的池子
            Scribe_Collections.Look(ref CurrentDisplayPool, "CurrentDisplayPool", LookMode.Def);
            // 保存上一期的池子用于去重
            Scribe_Collections.Look(ref LastGeneratedRandomPool, "LastGeneratedRandomPool", LookMode.Def);

            Scribe_Defs.Look(ref gachaSetting, "gachaSetting");
            Scribe_Values.Look(ref RotationTickCounter, "RotationTickCounter", 0);
            Scribe_Values.Look(ref TotalRefreshCount, "TotalRefreshCount", 0);

            // 初始化加载
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (gachaSetting == null)
                    gachaSetting = DefDatabase<GachaSetting>.AllDefs.FirstOrDefault();

                // 如果池子是空的，立即初始化一次
                if (CurrentDisplayPool == null || CurrentDisplayPool.Count == 0)
                {
                    UpdateRotationPool();
                }
            }
        }

        public override void GameComponentTick()
        {
            // 确保 Setting 存在
            if (gachaSetting == null)
            {
                gachaSetting = DefDatabase<GachaSetting>.AllDefs.FirstOrDefault();
                if (gachaSetting == null) return; // 还没有Def，跳过
            }

            if (!UiMapData.uiclose) // 假设这是你的UI判断逻辑
            {
                RotationTickCounter--;
            }

            // 倒计时结束，更新卡池
            if (RotationTickCounter <= 0)
            {
                UpdateRotationPool();
                RotationTickCounter = gachaSetting.RotationTick;
            }

            base.GameComponentTick();
        }

        /// <summary>
        /// 核心方法：更新卡池
        /// </summary>
        public void UpdateRotationPool()
        {
            if (gachaSetting == null) return;

            // 1. 增加刷新次数计数 (第1次，第2次...)
            TotalRefreshCount++;

            List<Gacha> newRandomItems = new List<Gacha>();

            // === 逻辑 A: 检查是否命中特殊队列 ===
            SpecialQueueConfig specialConfig = gachaSetting.SpecialQueues?
                .FirstOrDefault(x => x.triggerIndex == TotalRefreshCount);

            if (specialConfig != null && !specialConfig.forcedPool.NullOrEmpty())
            {
                // 命中特殊队列！直接强制使用配置的卡池
                // 注意：这里假设特殊队列配置的是那4个随机位的内容
                newRandomItems.AddRange(specialConfig.forcedPool);

                // 如果配置数量不足4个或过多，按需处理，这里直接全部采纳
            }
            else
            {
                // === 逻辑 B: 走常规随机算法 ===
                newRandomItems = GenerateRandomPoolItems();
            }

            // === 逻辑 C: 组装最终卡池 ===

            // 1. 记录这次生成的随机部分，供下一次去重使用
            LastGeneratedRandomPool.Clear();
            LastGeneratedRandomPool.AddRange(newRandomItems);

            // 2. 清空当前展示池
            CurrentDisplayPool.Clear();

            // 3. 加入本次生成的随机卡 (4个)
            CurrentDisplayPool.AddRange(newRandomItems);

            // 4. 加入永久固定的卡 (x个)
            if (!gachaSetting.FixedPool.NullOrEmpty())
            {
                // 这里使用 Distinct 防止如果随机池里偶然抽到了固定池的卡导致重复显示
                // 如果你的逻辑允许重复，可以直接 AddRange
                foreach (var fixedItem in gachaSetting.FixedPool)
                {
                    if (!CurrentDisplayPool.Contains(fixedItem))
                    {
                        CurrentDisplayPool.Add(fixedItem);
                    }
                }
            }

            SendRotationLetter();
        }

        /// <summary>
        /// 随机算法实现
        /// </summary>
        private List<Gacha> GenerateRandomPoolItems()
        {
            List<Gacha> result = new List<Gacha>();
            int slotsToFill = gachaSetting.SlotsCount; // 目标抽4个

            for (int i = 0; i < slotsToFill; i++)
            {
                Gacha selectedItem = null;

                // 15% 概率抽限定池，85% 概率抽常驻池
                bool tryLimited = Verse.Rand.Value < LimitedPoolChance;

                // 尝试抽取
                selectedItem = TryPickItem(tryLimited, result);

                // 如果因为池子空了或者去重太严格导致没抽到，尝试从另一个池子补救
                if (selectedItem == null)
                {
                    selectedItem = TryPickItem(!tryLimited, result);
                }

                if (selectedItem != null)
                {
                    result.Add(selectedItem);
                }
            }
            return result;
        }

        /// <summary>
        /// 尝试从指定类型的池子中抽取一个不重复的物品
        /// </summary>
        private Gacha TryPickItem(bool fromLimited, List<Gacha> currentBatch)
        {
            List<Gacha> sourcePool = fromLimited ? gachaSetting.LimitedPool : gachaSetting.StandardPool;

            if (sourcePool.NullOrEmpty()) return null;
            var candidates = sourcePool
                .Where(x => !LastGeneratedRandomPool.Contains(x) && !currentBatch.Contains(x))
                .ToList();
            if (candidates.Count == 0)
            {
                candidates = sourcePool
                    .Where(x => !currentBatch.Contains(x))
                    .ToList();
            }

            if (candidates.Count == 0) return null;
            return candidates.RandomElement();
        }
        public string GetRemainingTimeString(int currentCounter)
        {
            if (currentCounter <= 0) return "即将轮换";
            int days = currentCounter / 60000;
            int hours = (currentCounter % 60000) / 2500;

            if (days > 0) return $"距离轮换还剩: {days}天 {hours}小时";
            else if (hours > 0) return $"距离轮换还剩: {hours}小时";
            else
            {
                int minutes = currentCounter / 42;
                return $"距离轮换还剩: {minutes}分钟";
            }
        }

        public void Debug_ForceNextPool()
        {
            RotationTickCounter = 0;
            UpdateRotationPool();
            RotationTickCounter = gachaSetting?.RotationTick ?? 0;
            Messages.Message($"[Debug] 已强制刷新卡池，当前第 {TotalRefreshCount} 期", MessageTypeDefOf.PositiveEvent, false);
        }

        private void SendRotationLetter()
        {
            if (CurrentDisplayPool.NullOrEmpty())
            {
                return;
            }

            string poolNames = string.Join("、", CurrentDisplayPool.Select(x => x.gachaTitle).Where(x => !x.NullOrEmpty()));
            string letterText = poolNames.NullOrEmpty()
                ? "老师，新的招募卡池已经刷新，请前往什亭之匣查看。"
                : $"老师，新的招募卡池已经刷新。\n当前卡池：{poolNames}";

            Find.LetterStack.ReceiveLetter(
                "招募卡池已刷新",
                letterText,
                LetterDefOf.NeutralEvent);
        }
    }
}
