using BANWlLib.mainUI.Images;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace newpro
{
    //为界面提供统一图片绑定与可选图片目录查询入口。
    public static class imgcvT2d
    {
        //保留配置路径，由图片管理器解析 AB、散图及原版资源位置。
        public static string getRimWorldImgPath(string path)
        {
            return path;
        }

        //绑定图片控件与路径，显示期间持有引用，关闭或滚出视口后归还引用。
        public static void SetImage(Image image, string path)
        {
            BAUIImageCache.SetImage(image, path);
        }

        //枚举可选的旧式头像目录，不存在的目录返回空表。
        public static Dictionary<string, string> GetPngMap(string folderPath)
        {
            var result = new Dictionary<string, string>();
            if (!Directory.Exists(folderPath)) return result;
            foreach (string file in Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly))
                result[Path.GetFileNameWithoutExtension(file)] = file;
            return result;
        }
    }
}
