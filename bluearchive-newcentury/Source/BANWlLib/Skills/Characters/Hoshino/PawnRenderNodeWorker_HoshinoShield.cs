using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //同一件盾牌在输出形态背负，在防御形态移至身前。
    public class PawnRenderNodeWorker_HoshinoShield : PawnRenderNodeWorker_Apparel_Body
    {
        //从角色实例读取形态，普通服装绘制不会进入本节点。
        private static bool IsTank(PawnDrawParms parms) =>
            Hediff_SpecialSkillState.Find(parms.pawn, SpecialSkillRole.Hoshino)?.stage == 1;

        //前持时沿当前朝向移至身前，继续跟随原版身体姿态矩阵。
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 offset = base.OffsetFor(node, parms, out pivot);
            if (IsTank(parms)) offset += new Vector3(0f, 0f, 0.35f).RotatedBy(parms.facing.AsAngle);
            return offset;
        }

        //朝北时由身体遮挡前持盾牌，其他朝向把盾牌放在服装前方。
        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms) =>
            IsTank(parms) ? (parms.facing == Rot4.North ? -3f : 93f) : base.LayerFor(node, parms);

        //从背负翻转为前持时，南北面交换盾牌的正反面。
        public override Material GetFinalizedMaterial(PawnRenderNode node, PawnDrawParms parms)
        {
            if (IsTank(parms) && (parms.facing == Rot4.North || parms.facing == Rot4.South))
                parms.facing = parms.facing.Opposite;
            return base.GetFinalizedMaterial(node, parms);
        }

        //盾牌从背包上翻至手前，保持现有贴图标志的正向。
        public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
        {
            Quaternion rotation = base.RotationFor(node, parms);
            if (IsTank(parms) && (parms.facing == Rot4.North || parms.facing == Rot4.South))
                rotation *= Quaternion.AngleAxis(180f, Vector3.up);
            return rotation;
        }
    }
}
