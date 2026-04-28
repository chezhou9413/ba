using RimWorld;
using System.Collections.Generic;
using System.IO; // Added for Path and File operations
using Verse;
using Verse.Sound;

namespace BANWlLib
{
    /// <summary>
    /// 学生初始化组件配置类
    /// 用于配置学生角色的初始经验值和升星特效
    /// </summary>
    public class StudentInitializationCompProperties : CompProperties
    {
        // 初始经验值 - 角色生成时的经验值
        public int initialExperience = 0;

        // 升星特效 - 升星时播放的特效ID或路径
        public string starUpEffect = "";

        // 升星音效列表 - 升星时随机播放其中一个音效
        public List<string> starUpSounds = new List<string>();

        /// <summary>
        /// 构造函数
        /// 告诉游戏这个配置对应哪个组件类
        /// </summary>
        public StudentInitializationCompProperties()
        {
            this.compClass = typeof(StudentInitializationComp);
        }
    }

    /// <summary>
    /// 学生初始化组件实现类
    /// 处理学生角色的初始化和升星特效
    /// </summary>
    public class StudentInitializationComp : ThingComp
    {
        // 标记是否已经执行过初始化
        private bool hasInitialized = false;

        // 延迟初始化的计时器
        private int initializationTimer = 0;

        // 上次检查的经验值（用于检测经验值变化）
        private int lastExperienceValue = 0;

        // 当前星星等级（用于检测升星）
        private int currentStarLevel = 0;

        // 当前播放的特效实例（用于管理特效生命周期）
        private Effecter currentEffecter = null;

        // 特效播放计时器（用于控制特效播放时长）
        private int effecterTimer = 0;

        // 性能优化：缓存上次检查的经验值，避免重复计算
        private int lastCheckedExperience = -1;
        private int lastExperienceCheckTick = 0;

        /// <summary>
        /// 获取组件配置属性
        /// </summary>
        public StudentInitializationCompProperties Props => (StudentInitializationCompProperties)this.props;

        public HumanIntPropertyComp humanComp = null;

        /// <summary>
        /// 获取初始经验值
        /// </summary>
        /// <returns>配置的初始经验值</returns>
        public int GetInitialExperience()
        {
            return Props.initialExperience;
        }

        /// <summary>
        /// 获取升星特效ID
        /// </summary>
        /// <returns>升星特效的ID或路径</returns>
        public string GetStarUpEffect()
        {
            return Props.starUpEffect;
        }

        /// <summary>
        /// 获取随机的升星音效ID
        /// </summary>
        /// <returns>从音效列表中随机选择的一个音效ID，如果列表为空则返回空字符串</returns>
        public string GetRandomStarUpSound()
        {
            if (Props.starUpSounds == null || Props.starUpSounds.Count == 0)
            {
                return "";
            }
            return Props.starUpSounds.RandomElement();
        }

        /// <summary>
        /// 组件初始化时调用
        /// 启动延迟初始化计时器
        /// </summary>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            humanComp = parent.GetComp<HumanIntPropertyComp>();

            // 启动延迟初始化计时器（6个tick = 0.1秒）
            if (!hasInitialized)
            {
                initializationTimer = 6;
            }
        }

        /// <summary>
        /// 每帧更新
        /// 处理延迟初始化和经验值变化监听
        /// 性能优化：减少不必要的检查频率，优化特效管理
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();

            // 处理延迟初始化
            if (!hasInitialized && initializationTimer > 0)
            {
                initializationTimer--;
                if (initializationTimer <= 0)
                {
                    PerformInitialization();
                    hasInitialized = true;
                }
            }

            // 性能优化：每5秒检查一次经验值变化，而不是每秒
            if (hasInitialized)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (currentTick - lastExperienceCheckTick >= 300) // 5秒 = 300 ticks
                {
                    CheckExperienceChange();
                    lastExperienceCheckTick = currentTick;
                }
            }

            // 管理特效播放 - 持续调用EffectTick来保持特效活跃
            if (currentEffecter != null && effecterTimer > 0)
            {
                effecterTimer--;

                // 每帧调用EffectTick来保持特效活跃，确保延迟子特效能正确播放
                if (parent is Pawn pawn)
                {
                    TargetInfo targetInfo = new TargetInfo(pawn);
                    currentEffecter.EffectTick(targetInfo, targetInfo);
                }

                if (effecterTimer <= 0)
                {
                    // 特效播放时间结束，清理特效
                    currentEffecter.Cleanup();
                    currentEffecter = null;
                }
            }
        }

        /// <summary>
        /// 执行实际的初始化逻辑
        /// 设置初始经验值
        /// </summary>
        private void PerformInitialization()
        {
            if (humanComp != null)
            {
                // 检查是否有保存的学生数据
                var tracker = Current.Game.GetComponent<BANWlLib.mainUI.StudentManual.ManualDataGameComp>();
                if (tracker != null)
                {
                    var studentSave = tracker.studentSaves.FirstOrDefault(s => s.DefName == parent.def.defName);
                    if (studentSave != null)
                    {
                        // 如果有保存的数据，使用保存的经验值
                        humanComp.SetValue(studentSave.StudentExtra);
                        lastExperienceValue = studentSave.StudentExtra;
                        currentStarLevel = GetCurrentStarLevel();
                        return;
                    }
                }

                // 如果没有保存的数据，使用默认初始经验值
                if (Props.initialExperience > 0)
                {
                    humanComp.SetValue(Props.initialExperience);
                    lastExperienceValue = Props.initialExperience;
                    currentStarLevel = GetCurrentStarLevel();
                }
            }
        }

        /// <summary>
        /// 检查经验值变化
        /// 当经验值增加时，检查是否达到新的星星阈值
        /// 性能优化：添加缓存检查，避免重复计算
        /// </summary>
        private void CheckExperienceChange()
        {
            if (humanComp == null) return;

            int currentExperience = humanComp.CustomIntValue;

            // 性能优化：如果经验值没有变化，直接返回
            if (currentExperience == lastCheckedExperience)
            {
                return;
            }

            // 检查经验值是否有变化
            if (currentExperience != lastExperienceValue)
            {
                // 计算新的星星等级
                int newStarLevel = GetCurrentStarLevel();

                // 如果星星等级提升了，播放升星特效
                if (newStarLevel > currentStarLevel)
                {
                    PlayStarUpEffect();

                    // 显示升星消息
                    Messages.Message($"{parent.LabelShort} 达到了 {newStarLevel} 星级！",
                        parent, MessageTypeDefOf.PositiveEvent);

                    currentStarLevel = newStarLevel;
                }

                lastExperienceValue = currentExperience;
            }

            // 更新缓存
            lastCheckedExperience = currentExperience;
        }

        /// <summary>
        /// 获取当前星星等级
        /// 根据经验值和减伤组件的阈值计算当前星级
        /// </summary>
        /// <returns>当前星星等级</returns>
        private int GetCurrentStarLevel()
        {
            if (humanComp == null) return 0;

            // 获取减伤组件来获取星星阈值
            DamageReductionComp damageComp = parent.GetComp<DamageReductionComp>();
            if (damageComp == null) return 0;

            int currentExperience = humanComp.CustomIntValue;

            // 从最高等级开始检查，找到符合条件的最高等级
            for (int i = damageComp.Props.customValueThresholds.Count - 1; i >= 0; i--)
            {
                if (currentExperience >= damageComp.Props.customValueThresholds[i])
                {
                    return i + 1; // 等级从1开始
                }
            }

            return 0; // 如果没有达到任何阈值，返回0级
        }

        /// <summary>
        /// 播放升星特效
        /// 检查特效是否存在并播放相应的特效和音效
        /// </summary>
        public void PlayStarUpEffect()
        {
            if (parent is Pawn pawn)
            {

                // 播放升星特效
                if (!string.IsNullOrEmpty(Props.starUpEffect))
                {
                    TryPlayEffecter(Props.starUpEffect, pawn);
                }

                // 检查音效列表
                if (Props.starUpSounds != null && Props.starUpSounds.Count > 0)
                {

                    // 随机选择并播放一个升星音效
                    string selectedSound = GetRandomStarUpSound();
                    if (!string.IsNullOrEmpty(selectedSound))
                    {
                        TryPlaySound(selectedSound, pawn);
                    }
                    else
                    {
                        Log.Error("[升星音效] 无法获取随机音效");
                    }
                }
                else
                {
                    Log.Error("[升星音效] 音效列表为空或未配置");
                }
            }
            else
            {
                Log.Error("[升星特效] parent不是Pawn类型");
            }
        }

        /// <summary>
        /// 尝试播放指定的特效
        /// </summary>
        /// <param name="effecterDefName">特效定义名</param>
        /// <param name="pawn">目标角色</param>
        /// <returns>是否成功播放</returns>
        private bool TryPlayEffecter(string effecterDefName, Pawn pawn)
        {
            try
            {
                // 如果已经有特效在播放，先清理旧的
                if (currentEffecter != null)
                {
                    currentEffecter.Cleanup();
                    currentEffecter = null;
                }

                // 查找特效定义
                EffecterDef effecterDef = DefDatabase<EffecterDef>.GetNamed(effecterDefName, false);
                if (effecterDef != null)
                {
                    // 创建特效实例
                    currentEffecter = effecterDef.Spawn();

                    // 创建目标信息
                    TargetInfo targetInfo = new TargetInfo(pawn);

                    // 初始触发特效
                    currentEffecter.Trigger(targetInfo, targetInfo);

                    // 设置特效播放时长（5秒 = 300 ticks，确保所有延迟特效都能播放完成）
                    effecterTimer = 300;

                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[升星特效] 播放特效时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 尝试播放指定的音效
        /// </summary>
        /// <param name="soundDefName">音效定义名</param>
        /// <param name="pawn">目标角色</param>
        /// <returns>是否成功播放</returns>
        private bool TryPlaySound(string soundDefName, Pawn pawn)
        {
            try
            {

                // 查找音效定义
                SoundDef soundDef = DefDatabase<SoundDef>.GetNamed(soundDefName, false);
                if (soundDef != null)
                {

                    // 检查音效是否有有效的subSounds
                    if (soundDef.subSounds == null || soundDef.subSounds.Count == 0)
                    {
                        Log.Error($"[升星音效] 音效定义 {soundDefName} 没有有效的subSounds");
                        return false;
                    }

                    // 检查音效文件路径
                    foreach (var subSound in soundDef.subSounds)
                    {
                        foreach (var grain in subSound.grains)
                        {
                            if (grain is AudioGrain_Clip audioGrain)
                            {
                                string fullPath = $"Sounds/{audioGrain.clipPath}";

                                // 尝试获取音效文件的完整路径
                                string modPath = ModLister.GetActiveModWithIdentifier("Archive.NewWorld")?.RootDir.FullName ?? "";
                                string absolutePath = Path.Combine(modPath, fullPath);

                            }
                        }
                    }

                    // 创建音效信息
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn));

                    // 播放音效
                    soundDef.PlayOneShot(info);
                    return true;
                }
                else
                {
                    Log.Error($"[升星音效] 找不到音效定义: {soundDefName}");
                }

                return false;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[升星音效] 播放音效时发生错误: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 数据保存和加载
        /// 保存初始化状态和经验值记录
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hasInitialized, "hasInitialized", false);
            Scribe_Values.Look(ref initializationTimer, "initializationTimer", 0);
            Scribe_Values.Look(ref lastExperienceValue, "lastExperienceValue", 0);
            Scribe_Values.Look(ref currentStarLevel, "currentStarLevel", 0);
            Scribe_Values.Look(ref effecterTimer, "effecterTimer", 0);
        }
    }
}