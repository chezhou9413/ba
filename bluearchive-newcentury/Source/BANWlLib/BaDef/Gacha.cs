using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.BaDef
{
    /// <summary>
    /// 抽卡池配置，负责保存权重和按学生数据 Def 维护的候选列表。
    /// </summary>
    public class GachaPool
    {
        public float Weight = 0;
        public List<BaStudentDef> StudentList = new List<BaStudentDef>();
    }
    /// <summary>
    /// 抽卡配置 Def，负责描述一个卡池的资源、文本和各星级池。
    /// </summary>
    public class Gacha:Def
    {
        public string gachaTexPath;
        public string gachaVidPath;
        public string gachaTitle;
        public string gachaUp;
        public string gachaDesc;
        public bool isFes;

        public GachaPool oneStarPool;
        public GachaPool twoStarPool;
        public GachaPool threeStarPool;
        public GachaPool upthreeStarPool;
        public GachaPool FESupthreeStarPool;
        public GachaPool FESthreeStarPool;
    }
}
