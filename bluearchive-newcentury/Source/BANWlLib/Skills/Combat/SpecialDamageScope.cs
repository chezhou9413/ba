using System;
using BANWlLib.BattleSystem;

namespace BANWlLib.Skills
{
    //同步伤害上下文，负责标记已结算伤害、普攻来源和蓄积资格，阻止旧补丁重复乘算。
    public sealed class SpecialDamageScope : IDisposable
    {
        [ThreadStatic] private static BattleDamageRequest current;
        private readonly BattleDamageRequest previous;
        public static BattleDamageRequest Current => current;

        //保存上一层伤害上下文，允许嵌套伤害在返回后恢复正确来源。
        private SpecialDamageScope(BattleDamageRequest request)
        {
            previous = current;
            current = request;
        }

        //建立一次同步伤害处理的范围。
        public static SpecialDamageScope Enter(BattleDamageRequest request) => new SpecialDamageScope(request);

        //结束当前范围并恢复上层请求。
        public void Dispose() { current = previous; }
    }
}
