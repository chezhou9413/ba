using BANWlLib.BaDef;
using newpro;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BANWlLib.BANWGamecomp
{
    //招募卡池轮换组件，负责记录刷新次数、生成展示卡池并在轮换时通知玩家。
    public class Gamecomp_gakaAction : GameComponent
    {
        //当前最终展示给玩家的卡池，包含随机卡池和固定卡池。
        public List<Gacha> CurrentDisplayPool = new List<Gacha>();

        //记录上一期随机生成的卡池，用于常规轮换时去重。
        public List<Gacha> LastGeneratedRandomPool = new List<Gacha>();

        public GachaSetting gachaSetting;
        public int RotationTickCounter = 0;

        //记录卡池更新了多少次，用于触发特殊队列。
        public int TotalRefreshCount = 0;
        private const float LimitedPoolChance = 0.15f;

        public int gacaPoit = 0;

        //创建招募卡池轮换组件，负责让 RimWorld GameComponent 系统实例化组件。
        public Gamecomp_gakaAction(Game game)
        {
        }

        //获取当期的非固定卡池，负责给界面和逻辑读取本期随机部分。
        public List<Gacha> GetCurrentRandomPool()
        {
            return LastGeneratedRandomPool?.ToList() ?? new List<Gacha>();
        }

        //更新招募积分，负责增加积分或在积分足够时扣除消耗。
        public bool updataGacaPoit(int value) {
            if (value >= 0)
            {
                gacaPoit += value;
                return true;
            }
            int cost = -value; //转为正数。
            if (cost > gacaPoit)
            {
                return false; //积分不足。
            }
            gacaPoit -= cost;
            return true;
        }

        //保存和读取招募系统数据，负责持久化积分、展示池、计时器和刷新次数。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref gacaPoit, "gacaPoit", 0);
            //保存当前展示的池子。
            Scribe_Collections.Look(ref CurrentDisplayPool, "CurrentDisplayPool", LookMode.Def);
            //保存上一期的池子用于去重。
            Scribe_Collections.Look(ref LastGeneratedRandomPool, "LastGeneratedRandomPool", LookMode.Def);

            Scribe_Defs.Look(ref gachaSetting, "gachaSetting");
            Scribe_Values.Look(ref RotationTickCounter, "RotationTickCounter", 0);
            Scribe_Values.Look(ref TotalRefreshCount, "TotalRefreshCount", 0);

            //读档后补齐配置和展示池，防止旧档没有初始化卡池。
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (gachaSetting == null)
                    gachaSetting = DefDatabase<GachaSetting>.AllDefs.FirstOrDefault();

                //如果池子是空的，立即初始化一次。
                if (CurrentDisplayPool == null || CurrentDisplayPool.Count == 0)
                {
                    UpdateRotationPool();
                }
            }
        }

        //游戏逐 tick 更新，负责在界面开启时推进轮换倒计时并触发卡池刷新。
        public override void GameComponentTick()
        {
            //确保配置存在。
            if (gachaSetting == null)
            {
                gachaSetting = DefDatabase<GachaSetting>.AllDefs.FirstOrDefault();
                if (gachaSetting == null) return;
            }

            if (!UiMapData.uiclose)
            {
                //界面开启时才推进倒计时，避免玩家看不到刷新结果。
                RotationTickCounter--;
            }

            //倒计时结束，更新卡池。
            if (RotationTickCounter <= 0)
            {
                UpdateRotationPool();
                RotationTickCounter = gachaSetting.RotationTick;
            }

            base.GameComponentTick();
        }

        //更新卡池，负责按刷新次数选择特殊队列或常规随机池并组装最终展示列表。
        public void UpdateRotationPool()
        {
            if (gachaSetting == null) return;

            //先增加刷新次数，特殊队列的周期判断从第 1 次刷新开始计算。
            TotalRefreshCount++;

            List<Gacha> newRandomItems = new List<Gacha>();

            //检查是否命中特殊队列，triggerIndex 表示每隔多少次刷新触发一次。
            SpecialQueueConfig specialConfig = gachaSetting.SpecialQueues?
                .FirstOrDefault(IsSpecialQueueTriggered);

            if (specialConfig != null && !specialConfig.forcedPool.NullOrEmpty())
            {
                //命中特殊队列时从配置候选池里按槽位数量随机抽取。
                newRandomItems = GenerateSpecialPoolItems(specialConfig.forcedPool);
            }
            else
            {
                newRandomItems = GenerateRandomPoolItems();
            }

            //记录这次生成的随机部分，供下一次去重使用。
            LastGeneratedRandomPool.Clear();
            LastGeneratedRandomPool.AddRange(newRandomItems);

            //清空当前展示池。
            CurrentDisplayPool.Clear();

            //加入本次生成的随机卡池。
            CurrentDisplayPool.AddRange(newRandomItems);

            //加入永久固定的卡池。
            if (!gachaSetting.FixedPool.NullOrEmpty())
            {
                //避免固定池和随机池重复显示同一个卡池。
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

        //判断特殊队列是否在本次刷新触发，负责把 triggerIndex 作为重复周期使用。
        private bool IsSpecialQueueTriggered(SpecialQueueConfig specialConfig)
        {
            return specialConfig != null &&
                specialConfig.triggerIndex > 0 &&
                TotalRefreshCount % specialConfig.triggerIndex == 0;
        }

        //生成特殊队列卡池，负责从强制候选池中按槽位数量随机抽取并尽量避免连续重复。
        private List<Gacha> GenerateSpecialPoolItems(List<Gacha> forcedPool)
        {
            List<Gacha> result = new List<Gacha>();
            int slotsToFill = gachaSetting.SlotsCount;

            for (int i = 0; i < slotsToFill; i++)
            {
                Gacha selectedItem = TryPickForcedPoolItem(forcedPool, result);
                if (selectedItem == null)
                {
                    break;
                }

                result.Add(selectedItem);
            }

            return result;
        }

        //尝试从特殊候选池中抽取一个卡池，负责在候选不足时逐步放宽重复限制。
        private Gacha TryPickForcedPoolItem(List<Gacha> forcedPool, List<Gacha> currentBatch)
        {
            if (forcedPool.NullOrEmpty()) return null;

            List<Gacha> candidates = forcedPool
                .Where(x => !LastGeneratedRandomPool.Contains(x) && !currentBatch.Contains(x))
                .ToList();
            if (candidates.Count > 0)
            {
                return candidates.RandomElement();
            }

            //上一期去重导致候选不足时，允许和上一期重复，但仍保持本期内部不重复。
            candidates = forcedPool
                .Where(x => !currentBatch.Contains(x))
                .ToList();
            if (candidates.Count > 0)
            {
                return candidates.RandomElement();
            }

            //候选池数量本身小于槽位数量时，允许本期内部重复来补满展示槽位。
            return forcedPool.RandomElement();
        }

        //生成常规随机卡池，负责按常驻和限定概率填充本期随机槽位。
        private List<Gacha> GenerateRandomPoolItems()
        {
            List<Gacha> result = new List<Gacha>();
            int slotsToFill = gachaSetting.SlotsCount;

            for (int i = 0; i < slotsToFill; i++)
            {
                Gacha selectedItem = null;

                //按配置概率优先尝试限定池，否则尝试常驻池。
                bool tryLimited = Verse.Rand.Value < LimitedPoolChance;

                //先按本次概率结果抽取。
                selectedItem = TryPickItem(tryLimited, result);

                //如果池子为空或去重后没有候选，就从另一个池子补位。
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

        //尝试从指定类型的池子中抽取一个卡池，负责避开上一期和本期已选内容。
        private Gacha TryPickItem(bool fromLimited, List<Gacha> currentBatch)
        {
            List<Gacha> sourcePool = fromLimited ? gachaSetting.LimitedPool : gachaSetting.StandardPool;

            if (sourcePool.NullOrEmpty()) return null;
            var candidates = sourcePool
                .Where(x => !LastGeneratedRandomPool.Contains(x) && !currentBatch.Contains(x))
                .ToList();
            if (candidates.Count == 0)
            {
                //上一期去重过严时放宽限制，只保证本期内部不重复。
                candidates = sourcePool
                    .Where(x => !currentBatch.Contains(x))
                    .ToList();
            }

            if (candidates.Count == 0) return null;
            return candidates.RandomElement();
        }

        //获取轮换剩余时间文本，负责把 tick 倒计时转换成玩家可读的天、小时或分钟。
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

        //调试强制刷新卡池，负责立即推进一次轮换并重置轮换倒计时。
        public void Debug_ForceNextPool()
        {
            RotationTickCounter = 0;
            UpdateRotationPool();
            RotationTickCounter = gachaSetting?.RotationTick ?? 0;
            Messages.Message($"[Debug] 已强制刷新卡池，当前第 {TotalRefreshCount} 期", MessageTypeDefOf.PositiveEvent, false);
        }

        //发送卡池轮换信件，负责把当前展示卡池名称通知玩家。
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
