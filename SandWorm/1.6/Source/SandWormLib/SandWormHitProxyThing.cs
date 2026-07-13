using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace SandWormLib
{
    public class SandWormHitProxyThing : ThingWithComps, IAttackTarget
    {
        private SandWormThing owner;
        private int boneIndex = -1;
        private Vector3 exactPos;
        private Quaternion worldRotation = Quaternion.identity;
        private Vector2 debugSize = new Vector2(1f, 1f);

        public override Vector3 DrawPos => exactPos;

        Thing IAttackTarget.Thing => this;

        public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

        public float TargetPriorityFactor => 1f;

        public SandWormThing Owner => owner;
        public int BoneIndex => boneIndex;
        public Quaternion WorldRotation => worldRotation;
        public Vector2 DebugSize => debugSize;

        public void AttachToOwner(SandWormThing newOwner, int newBoneIndex)
        {
            owner = newOwner;
            boneIndex = newBoneIndex;
        }

        public void UpdateTransform(Vector3 newExactPos, Quaternion newWorldRotation, Vector2 newDebugSize)
        {
            exactPos = newExactPos;
            worldRotation = newWorldRotation;
            debugSize = newDebugSize;

            if (Spawned && Map != null)
            {
                IntVec3 newCell = ClampCellToMap(exactPos.ToIntVec3());
                if (newCell != Position)
                {
                    Position = newCell;
                }
            }
        }

        protected override void Tick()
        {
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (exactPos == default(Vector3))
            {
                exactPos = base.DrawPos;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (SandWormMod.Settings == null || !SandWormMod.Settings.showDeveloperSettings || !SandWormMod.Settings.showHitProxyDebugRects)
            {
                return;
            }

            DrawDebugRect();
        }

        public void DrawDebugRect()
        {
            if (!Spawned || Map == null || Find.CurrentMap != Map)
            {
                return;
            }

            float halfWidth = Mathf.Max(0.5f, debugSize.x * 0.5f);
            float halfLength = Mathf.Max(0.5f, debugSize.y * 0.5f);
            Vector3 right = worldRotation * Vector3.right;
            Vector3 forward = worldRotation * Vector3.forward;
            right.y = 0f;
            forward.y = 0f;
            if (right.sqrMagnitude < 0.001f || forward.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
                forward = Vector3.forward;
            }

            right.Normalize();
            forward.Normalize();
            Vector3 center = exactPos + Vector3.up * 0.45f;
            Vector3 cornerA = center + right * halfWidth + forward * halfLength;
            Vector3 cornerB = center - right * halfWidth + forward * halfLength;
            Vector3 cornerC = center - right * halfWidth - forward * halfLength;
            Vector3 cornerD = center + right * halfWidth - forward * halfLength;
            SimpleColor color = boneIndex == 0 ? SimpleColor.Red : SimpleColor.Green;
            GenDraw.DrawLineBetween(cornerA, cornerB, color, 0.18f);
            GenDraw.DrawLineBetween(cornerB, cornerC, color, 0.18f);
            GenDraw.DrawLineBetween(cornerC, cornerD, color, 0.18f);
            GenDraw.DrawLineBetween(cornerD, cornerA, color, 0.18f);
            GenDraw.DrawLineBetween(center - right * 0.6f, center + right * 0.6f, SimpleColor.White, 0.08f);
            GenDraw.DrawLineBetween(center - forward * 0.6f, center + forward * 0.6f, SimpleColor.White, 0.08f);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (owner != null && !owner.Destroyed)
            {
                absorbed = true;
                owner.Notify_HitProxyDamaged(this, dinfo);
                return;
            }

            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        public override string GetInspectString()
        {
            return string.Empty;
        }

        public bool HostileTo(Thing t)
        {
            if (owner == null || owner.Destroyed)
            {
                return false;
            }

            if (t == null || t.Faction == null)
            {
                return false;
            }

            return t.Faction == Faction.OfPlayer || !t.Faction.HostileTo(Faction.OfPlayer);
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return owner == null || owner.Destroyed || !Spawned;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref owner, "owner");
            Scribe_Values.Look(ref boneIndex, "boneIndex", -1);
            Scribe_Values.Look(ref exactPos, "exactPos");
            Scribe_Values.Look(ref worldRotation, "worldRotation", Quaternion.identity);
            Scribe_Values.Look(ref debugSize, "debugSize", new Vector2(1f, 1f));
        }

        private IntVec3 ClampCellToMap(IntVec3 cell)
        {
            if (Map == null)
            {
                return cell;
            }

            IntVec2 size = def?.size ?? new IntVec2(1, 1);
            int halfWidth = size.x / 2;
            int halfHeight = size.z / 2;

            int minX = halfWidth;
            int maxX = Mathf.Max(minX, Map.Size.x - (size.x - halfWidth));
            int minZ = halfHeight;
            int maxZ = Mathf.Max(minZ, Map.Size.z - (size.z - halfHeight));

            int x = Mathf.Clamp(cell.x, minX, maxX);
            int z = Mathf.Clamp(cell.z, minZ, maxZ);
            return new IntVec3(x, 0, z);
        }
    }
}
