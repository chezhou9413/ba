using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BANWlLib.Tool
{
    public static class ListUtils
    {
        //移除完全匹配的字符串及空白条目，返回移除数量。
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
