using System.Collections.Generic;
using Verse;

namespace SandWormLib
{
    // SandWormChallengeRiskEffect 负责描述一个 XML 词条对挑战运行参数产生的单项效果。
    public sealed class SandWormChallengeRiskEffect
    {
        public string effectType;
        public int count;
        public float factor = 1f;

        // ConfigErrors 负责校验词条效果的基础字段，方便后续扩展时发现 XML 配置错误。
        public IEnumerable<string> ConfigErrors(string ownerDefName)
        {
            if (effectType.NullOrEmpty())
            {
                yield return ownerDefName + " has a risk effect without effectType.";
            }

            if (count < 0)
            {
                yield return ownerDefName + " has a risk effect with negative count.";
            }

            if (factor < 0f)
            {
                yield return ownerDefName + " has a risk effect with negative factor.";
            }
        }
    }
}
