using BANWlLib.BaDef;
using newpro;
using System.Collections.Generic;
using UnityEngine;

namespace BANWlLib.mainUI.StudentManual
{
    /// <summary>
    /// 学生图鉴运行时缓存，负责保存图鉴界面对象和按学生数据 Def 读取的列表。
    /// </summary>
    public static class ManualMapData
    {
        public static bool isOpenManual = false;

        public static GameObject StudentManual;

        public static List<BaStudentDef> StudentList = new List<BaStudentDef>();
        public static List<BaStudentUI> studentUIList = new List<BaStudentUI>();

        public static GameObject StudentListOBJ;
        public static GameObject ManualScrollView;

        public static int selectStudentIndex = 0;

        public static GameObject StudentDetailOBJ;

        public static GameObject StarOBJ;

        public static GameObject messageUI;
        public static GameObject messageUIQuek;


        public static bool isOpenDetail = false;
        public static void Reset()
        {
            messageUIQuek = null;
            messageUI = null;
            StarOBJ = null;
            isOpenDetail = false;
            StudentDetailOBJ = null;
            selectStudentIndex = 0;
            ManualScrollView = null;
            StudentListOBJ = null;
            studentUIList = new List<BaStudentUI>();
            StudentList = new List<BaStudentDef>();   
            StudentManual = null;
            isOpenManual = false;
        }
    }


}
