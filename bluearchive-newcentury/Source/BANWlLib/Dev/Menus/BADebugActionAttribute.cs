using System;
using LudeonTK;

namespace BANWlLib.Dev.Menus
{
    //声明 BA 子页面内的调试操作，避免每个操作分别注册到原版首页。
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class BADebugActionAttribute : Attribute
    {
        internal readonly string Page;
        internal readonly string Label;
        public AllowedGameStates allowedGameStates = AllowedGameStates.Playing;
        public DebugActionType actionType = DebugActionType.Action;

        //保存操作所属页面及显示名称，执行条件由标注处提供。
        public BADebugActionAttribute(string page, string label)
        {
            Page = page;
            Label = label;
        }
    }
}
