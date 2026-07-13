using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormChallengeRiskDef 负责让辛迪加挑战词条通过 XML 配置名称、等级、位置、前置关系和效果。
    public sealed class SandWormChallengeRiskDef : Def
    {
        public int level = 1;
        public string groupKey;
        public string groupLabel;
        public Vector2 gridPosition;
        public string prerequisite;
        public string iconPath = "Things/SandWorm/SandHammer";
        public List<SandWormChallengeRiskEffect> effects;

        // GroupLabel 负责兼容旧配置里的分组显示文本，新词条界面不再依赖大类。
        public string GroupLabel
        {
            get
            {
                return !groupLabel.NullOrEmpty() ? groupLabel : groupKey.Translate().ToString();
            }
        }

        // ConfigErrors 负责校验词条基础配置，避免 UI 和挑战运行时读取到无效等级或效果。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (level <= 0)
            {
                yield return defName + " level must be greater than 0.";
            }

            if (effects != null)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    foreach (string error in effects[i].ConfigErrors(defName))
                    {
                        yield return error;
                    }
                }
            }
        }
    }
}
