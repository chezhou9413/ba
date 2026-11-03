using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //每个实际生成的EX场地保存独立的记录与释放机会；前摇锁定索引，发射才消耗。
    public class KeiRuntimeState : IExposable
    {
        public List<KeiCharge> charges = new List<KeiCharge>();
        public int castingIndex = -1;
        public int ReadyCount => charges.Count(charge => charge.ready);
        public KeiCharge Preview => castingIndex >= 0 && castingIndex < charges.Count
            ? charges[castingIndex] : charges.FirstOrDefault(charge => charge.ready) ?? charges.FirstOrDefault();

        //旧档原有的一次机会只迁移一次；已发射的旧状态不会重新获得机会。
        public static KeiRuntimeState Get(Hediff_SpecialSkillState state)
        {
            if (state.kei != null) return state.kei;
            var runtime = state.kei = new KeiRuntimeState();
            if (state.endTick > 0 || state.releaseReady)
            {
                runtime.charges.Add(new KeiCharge
                {
                    field = state.field, endTick = state.endTick, ready = state.releaseReady,
                    recorded = state.recorded, recordCap = state.recordCap
                });
                if (state.releaseReady && state.keiNormalJob != null) runtime.castingIndex = 0;
            }
            return runtime;
        }

        public void Add(Thing field, int endTick, float recordCap)
        {
            //同一场地重复通知不是另一次成功EX。
            if (field == null || charges.Any(charge => charge.field == field)) return;
            charges.Add(new KeiCharge { field = field, endTick = endTick, recordCap = recordCap });
        }

        //绝对游戏Tick沿用原来的离图计时语义，场地销毁本身不提前释放。
        public int Advance(int now)
        {
            int completed = 0;
            foreach (var charge in charges)
            {
                if (charge.ready || charge.endTick > now) continue;
                charge.ready = true;
                charge.field = null;
                completed++;
            }
            return completed;
        }

        public bool ReserveNext()
        {
            castingIndex = charges.FindIndex(charge => charge.ready);
            return castingIndex >= 0;
        }

        public bool TryConsume(out float amount)
        {
            amount = 0;
            if (castingIndex < 0 || castingIndex >= charges.Count || !charges[castingIndex].ready) return false;
            var charge = charges[castingIndex];
            amount = Mathf.Min(charge.recorded, charge.recordCap);
            charges.RemoveAt(castingIndex);
            castingIndex = -1;
            return true;
        }

        //兼容公共生命周期和旧存档字段；真正的数据来源是独立机会列表。
        public void Sync(Hediff_SpecialSkillState state)
        {
            var preview = Preview;
            state.releaseReady = ReadyCount > 0;
            state.endTick = -1;
            foreach (var charge in charges)
                if (!charge.ready) state.endTick = System.Math.Max(state.endTick, charge.endTick);
            state.field = preview?.field;
            state.recorded = preview?.recorded ?? 0f;
            state.recordCap = preview?.recordCap ?? 0f;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref charges, "charges", LookMode.Deep);
            Scribe_Values.Look(ref castingIndex, "castingIndex", -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                charges ??= new List<KeiCharge>();
                if (castingIndex < -1 || castingIndex >= charges.Count) castingIndex = -1;
            }
        }
    }

    public class KeiCharge : IExposable
    {
        public Thing field;
        public int endTick = -1;
        public bool ready;
        public float recorded;
        public float recordCap;

        public void ExposeData()
        {
            Scribe_References.Look(ref field, "field");
            Scribe_Values.Look(ref endTick, "endTick", -1);
            Scribe_Values.Look(ref ready, "ready");
            Scribe_Values.Look(ref recorded, "recorded");
            Scribe_Values.Look(ref recordCap, "recordCap");
        }
    }
}
