using System;
using System.Collections.Generic;
using System.Reflection;
using LudeonTK;
using Verse;

namespace BANWlLib.Dev.Menus
{
    //把 BA 调试声明转换为原版菜单节点，保留执行方式和游戏状态限制。
    internal static class BADebugNodeFactory
    {
        //绑定静态操作或下级菜单委托；签名不匹配时直接抛出注册错误。
        internal static DebugActionNode Create(MethodInfo method, BADebugActionAttribute attribute)
        {
            var node = new DebugActionNode(attribute.Label, attribute.actionType)
            {
                category = attribute.Page,
                sourceAttribute = new DebugActionAttribute(attribute.Page, attribute.Label)
                {
                    allowedGameStates = attribute.allowedGameStates,
                    actionType = attribute.actionType
                }
            };
            if (method.ReturnType == typeof(List<DebugActionNode>))
            {
                node.childGetter = (Func<List<DebugActionNode>>)Delegate.CreateDelegate(typeof(Func<List<DebugActionNode>>), method);
                node.label += "...";
            }
            else if (method.ReturnType == typeof(void))
            {
                if (attribute.actionType == DebugActionType.ToolMapForPawns)
                    node.pawnAction = (Action<Pawn>)Delegate.CreateDelegate(typeof(Action<Pawn>), method);
                else
                    node.action = (Action)Delegate.CreateDelegate(typeof(Action), method);
            }
            else
            {
                throw new InvalidOperationException("BA 调试操作必须返回 void 或 List<DebugActionNode>：" + method.DeclaringType.FullName + "." + method.Name);
            }
            if (attribute.actionType != DebugActionType.Action) node.label = "T: " + node.label;
            return node;
        }
    }
}
