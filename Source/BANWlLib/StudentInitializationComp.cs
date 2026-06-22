using RimWorld;
using BANWlLib.Tool;
using System.Collections.Generic;
using System.IO; // Added for Path and File operations
using System.Linq;
using Verse;
using Verse.Sound;

namespace BANWlLib
{
    // 学生初始化调试工具，负责让测试生成的 Pawn 可以保留手动设置的经验值。
    public static class StudentInitializationDebugUtility
    {
        // 标记 Pawn 已完成测试初始化，负责跳过默认初始经验和同名存档经验覆盖。
        public static void MarkDebugInitialized(Pawn pawn)
        {
            StudentInitializationComp comp = pawn?.GetComp<StudentInitializationComp>();
            comp?.MarkInitializedFromDebug();
        }
    }

    public class StudentInitializationCompProperties : CompProperties
    {
        public int initialExperience = 0;
        public string starUpEffect = "";
        public List<string> starUpSounds = new List<string>();

        public StudentInitializationCompProperties()
        {
            this.compClass = typeof(StudentInitializationComp);
        }
    }

    public class StudentInitializationComp : ThingComp
    {
        private bool hasInitialized = false;
        private int initializationTimer = 0;
        private int lastExperienceValue = 0;
        private int currentRankLevel = 0;
        private Effecter currentEffecter = null;
        private int effecterTimer = 0;
        private int lastCheckedExperience = -1;
        private int lastExperienceCheckTick = 0;
        public StudentInitializationCompProperties Props => (StudentInitializationCompProperties)this.props;

        public HumanIntPropertyComp humanComp = null;
        public int GetInitialExperience()
        {
            return Props.initialExperience;
        }

        // 标记组件已完成测试初始化，负责防止延迟初始化覆盖 Debug 设置的阶级经验。
        public void MarkInitializedFromDebug()
        {
            hasInitialized = true;
            initializationTimer = 0;
            lastExperienceValue = humanComp?.CustomIntValue ?? 0;
            lastCheckedExperience = lastExperienceValue;
            currentRankLevel = GetCurrentRankLevel();
        }

        public string GetStarUpEffect()
        {
            return Props.starUpEffect;
        }

        public string GetRandomStarUpSound()
        {
            if (Props.starUpSounds == null || Props.starUpSounds.Count == 0)
            {
                return "";
            }
            return Props.starUpSounds.RandomElement();
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            humanComp = parent.GetComp<HumanIntPropertyComp>();

            if (!hasInitialized)
            {
                initializationTimer = 6;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!StudentIdentityUtility.IsConfiguredStudentKind(parent as Pawn))
            {
                return;
            }

            if (!hasInitialized && initializationTimer > 0)
            {
                initializationTimer--;
                if (initializationTimer <= 0)
                {
                    PerformInitialization();
                    hasInitialized = true;
                }
            }

            if (hasInitialized)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (currentTick - lastExperienceCheckTick >= 300)
                {
                    CheckExperienceChange();
                    lastExperienceCheckTick = currentTick;
                }
            }

            if (currentEffecter != null && effecterTimer > 0)
            {
                effecterTimer--;

                if (parent is Pawn pawn)
                {
                    TargetInfo targetInfo = new TargetInfo(pawn);
                    currentEffecter.EffectTick(targetInfo, targetInfo);
                }

                if (effecterTimer <= 0)
                {
                    currentEffecter.Cleanup();
                    currentEffecter = null;
                }
            }
        }

        private void PerformInitialization()
        {
            if (!StudentIdentityUtility.IsConfiguredStudentKind(parent as Pawn))
            {
                return;
            }

            if (humanComp != null)
            {
                var tracker = Current.Game.GetComponent<BANWlLib.mainUI.StudentManual.ManualDataGameComp>();
                if (tracker != null)
                {
                    string studentId = StudentIdentityUtility.GetStudentId(parent as Pawn);
                    var studentSave = tracker.studentSaves.FirstOrDefault(s => s != null && s.DefName == studentId);
                    if (studentSave != null)
                    {
                        humanComp.SetValue(studentSave.StudentExtra);
                        lastExperienceValue = studentSave.StudentExtra;
                        currentRankLevel = GetCurrentRankLevel();
                        return;
                    }
                }

                int initialExperience = GetInitialExperience(parent as Pawn);
                if (initialExperience > 0)
                {
                    humanComp.SetValue(initialExperience);
                    lastExperienceValue = initialExperience;
                    currentRankLevel = GetCurrentRankLevel();
                }
            }
        }

        private int GetInitialExperience(Pawn pawn)
        {
            PawnProgressBarKindExtension kindConfig = pawn?.kindDef?.GetModExtension<PawnProgressBarKindExtension>();
            if (kindConfig != null && kindConfig.initialExperience >= 0)
            {
                return kindConfig.initialExperience;
            }

            return Props.initialExperience;
        }

        private void CheckExperienceChange()
        {
            if (!StudentIdentityUtility.IsConfiguredStudentKind(parent as Pawn))
            {
                return;
            }

            if (humanComp == null) return;

            int currentExperience = humanComp.CustomIntValue;

            if (currentExperience == lastCheckedExperience)
            {
                return;
            }

            if (currentExperience != lastExperienceValue)
            {
                int newRankLevel = GetCurrentRankLevel();

                if (newRankLevel > currentRankLevel)
                {
                    PlayStarUpEffect();

                    Messages.Message($"{parent.LabelShort} 达到了 {newRankLevel} 阶级！",
                        parent, MessageTypeDefOf.PositiveEvent);
                }

                currentRankLevel = newRankLevel;
                lastExperienceValue = currentExperience;
            }

            lastCheckedExperience = currentExperience;
        }

        private int GetCurrentRankLevel()
        {
            if (!StudentIdentityUtility.IsConfiguredStudentKind(parent as Pawn))
            {
                return 0;
            }

            if (humanComp == null) return 0;

            DamageReductionComp damageComp = parent.GetComp<DamageReductionComp>();
            if (damageComp == null) return 0;

            int currentExperience = humanComp.CustomIntValue;

            for (int i = damageComp.Props.customValueThresholds.Count - 1; i >= 0; i--)
            {
                if (currentExperience >= damageComp.Props.customValueThresholds[i])
                {
                    return i + 1; // 阶级从1开始
                }
            }

            return 1;
        }

        public void PlayStarUpEffect()
        {
            if (!StudentIdentityUtility.IsConfiguredStudentKind(parent as Pawn))
            {
                return;
            }

            if (parent is Pawn pawn)
            {

                string starUpEffect = GetStarUpEffect(pawn);
                if (!string.IsNullOrEmpty(starUpEffect))
                {
                    TryPlayEffecter(starUpEffect, pawn);
                }

                List<string> starUpSounds = GetStarUpSounds(pawn);
                if (starUpSounds != null && starUpSounds.Count > 0)
                {
                    string selectedSound = starUpSounds.RandomElement();
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

        private string GetStarUpEffect(Pawn pawn)
        {
            PawnProgressBarKindExtension kindConfig = pawn.kindDef?.GetModExtension<PawnProgressBarKindExtension>();
            if (kindConfig?.starUpEffects != null && kindConfig.starUpEffects.Count > 0)
            {
                return kindConfig.starUpEffects.RandomElement();
            }

            if (kindConfig != null && !string.IsNullOrEmpty(kindConfig.starUpEffect))
            {
                return kindConfig.starUpEffect;
            }

            return Props.starUpEffect;
        }

        private List<string> GetStarUpSounds(Pawn pawn)
        {
            PawnProgressBarKindExtension kindConfig = pawn.kindDef?.GetModExtension<PawnProgressBarKindExtension>();
            if (kindConfig?.starUpSounds != null && kindConfig.starUpSounds.Count > 0)
            {
                return kindConfig.starUpSounds;
            }

            return Props.starUpSounds;
        }

        private bool TryPlayEffecter(string effecterDefName, Pawn pawn)
        {
            try
            {
                if (currentEffecter != null)
                {
                    currentEffecter.Cleanup();
                    currentEffecter = null;
                }

                EffecterDef effecterDef = DefDatabase<EffecterDef>.GetNamed(effecterDefName, false);
                if (effecterDef != null)
                {
                    currentEffecter = effecterDef.Spawn();
                    TargetInfo targetInfo = new TargetInfo(pawn);
                    currentEffecter.Trigger(targetInfo, targetInfo);
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

        private bool TryPlaySound(string soundDefName, Pawn pawn)
        {
            try
            {
                SoundDef soundDef = DefDatabase<SoundDef>.GetNamed(soundDefName, false);
                if (soundDef != null)
                {
                    if (soundDef.subSounds == null || soundDef.subSounds.Count == 0)
                    {
                        Log.Error($"[升星音效] 音效定义 {soundDefName} 没有有效的subSounds");
                        return false;
                    }

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

                    SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn));
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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hasInitialized, "hasInitialized", false);
            Scribe_Values.Look(ref initializationTimer, "initializationTimer", 0);
            Scribe_Values.Look(ref lastExperienceValue, "lastExperienceValue", 0);
            Scribe_Values.Look(ref currentRankLevel, "currentRankLevel", 0);
            Scribe_Values.Look(ref effecterTimer, "effecterTimer", 0);
        }
    }
}
