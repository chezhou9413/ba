using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BANWlLib.Tool
{
    public static class ListUtils
    {
        /// <summary>
        /// 从字符串列表中移除一个指定的字符串（全字匹配），并清除所有空、null或空白条目。
        /// </summary>
        /// <param name="stringList">要修改的字符串列表。</param>
        /// <param name="stringToRemove">需要被移除的、完全匹配的字符串。</param>
        /// <returns>被移除的元素总数。</returns>
        public static int RemoveStringAndClean(List<string> stringList, string stringToRemove)
        {
            if (stringList == null)
            {
                return 0;
            }

            // 使用 RemoveAll 一次性完成所有删除操作
            return stringList.RemoveAll(item =>
                // 如果当前项与要删除的字符串完全相等，或者当前项是空/空白，则删除它
                item == stringToRemove || string.IsNullOrWhiteSpace(item)
            );
        }
    }
}
