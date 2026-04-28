using BANWlLib.BaDef;
using newpro;
using System.Collections.Generic;
using UnityEngine;

namespace BANWlLib.mainUI.StudentManual
{
    public static class ManualMapData
    {
        public static bool isOpenManual = false;

        public static GameObject StudentManual;

        public static List<BaStudentRaceDef> StudentList = new List<BaStudentRaceDef>();
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
            StudentList = new List<BaStudentRaceDef>();   
            StudentManual = null;
            isOpenManual = false;
        }
    }


}
