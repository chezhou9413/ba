using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LudeonTK;

namespace BANWlLib.Dev.Menus
{
    //提供 BA 调试的唯一首页入口，并按用途生成可继续进入的子页面。
    internal static class BADebugMenu
    {
        private static readonly string[] PageOrder = { "抽卡与招募", "学生档案", "任务进度", "战斗测试", "特殊技能", "UI内存" };

        //首次打开时收集本程序集的 BA 调试声明，后续由原版菜单缓存节点。
        [DebugAction("BA", "BA调试", allowedGameStates = AllowedGameStates.Playing)]
        private static List<DebugActionNode> Open()
        {
            var pages = new Dictionary<string, DebugActionNode>(StringComparer.Ordinal);
            foreach (Type type in typeof(BADebugMenu).Assembly.GetTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).OrderBy(m => m.MetadataToken))
                {
                    BADebugActionAttribute attribute = method.GetCustomAttribute<BADebugActionAttribute>(false);
                    if (attribute == null) continue;
                    if (!pages.TryGetValue(attribute.Page, out DebugActionNode page))
                    {
                        page = CreatePage(attribute.Page);
                        pages.Add(attribute.Page, page);
                    }
                    page.AddChild(BADebugNodeFactory.Create(method, attribute));
                }
            }
            return pages.Values.OrderByDescending(page => page.displayPriority).ThenBy(page => page.label, StringComparer.Ordinal).ToList();
        }

        //创建分组页面，并在当前状态没有可用操作时隐藏该页面。
        private static DebugActionNode CreatePage(string name)
        {
            int order = Array.IndexOf(PageOrder, name);
            var page = new DebugActionNode(name + "...")
            {
                category = "BA",
                displayPriority = order < 0 ? 0 : PageOrder.Length - order
            };
            page.visibilityGetter = () => page.children.Any(child => child.VisibleNow);
            return page;
        }
    }
}
