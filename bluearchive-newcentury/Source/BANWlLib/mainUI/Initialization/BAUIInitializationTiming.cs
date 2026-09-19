using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace BANWlLib.mainUI.Initialization
{
    //记录一次 UI 初始化的分段耗时，方便从游戏日志定位资源或界面构建瓶颈。
    internal sealed class BAUIInitializationTiming : IDisposable
    {
        private readonly string name;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly List<string> stages = new List<string>();
        private long last;

        //为当前初始化范围建立独立计时器。
        internal BAUIInitializationTiming(string name)
        {
            this.name = name;
        }

        //记录上一个阶段结束到当前阶段结束的毫秒数。
        internal void Mark(string stage)
        {
            long now = clock.ElapsedMilliseconds;
            stages.Add(stage + "=" + (now - last) + "ms");
            last = now;
        }

        //初始化范围结束时输出一条汇总，无需每个资源分别刷日志。
        public void Dispose()
        {
            Log.Message("[BA UI耗时] " + name + "：总计=" + clock.ElapsedMilliseconds + "ms；" + string.Join("，", stages));
        }
    }
}
