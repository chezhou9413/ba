using BANWlLib.mainUI.pojo;
using newpro;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace BANWlLib
{
    public static class shotlord
    {
        private static List<shot> ordinary = new List<shot>();
        private static List<shot> Fragment1 = new List<shot>();
        private static List<shot> Fragment2 = new List<shot>();
        public static bool Initializeshotlord()
        {
            // 清理静态商品列表，防止读档时重复创建商品
            ClearStaticShotLists();
            jsonlordshot();
            ceateShotPrefab();
            showpageUI("ordinary");
            return true;
        }

        /// <summary>
        /// 清理静态商品列表，防止读档时重复创建商品
        /// </summary>
        private static void ClearStaticShotLists()
        {
            ordinary.Clear();
            Fragment1.Clear();
            Fragment2.Clear();
        }

        private static void jsonlordshot()
        {
            List<string> ordinaryjson = new List<string>(Directory.GetFiles(UiMapData.modRootPath + "/1.6/UI/shot/ordinary", "*.json"));
            List<string> Fragment1json = new List<string>(Directory.GetFiles(UiMapData.modRootPath + "/1.6/UI/shot/Fragment1", "*.json"));
            List<string> Fragment2json = new List<string>(Directory.GetFiles(UiMapData.modRootPath + "/1.6/UI/shot/Fragment2", "*.json"));
            foreach (string path in ordinaryjson)
            {
                List<shot> loadedShots = jsoncvpojo.LoadShotFromJson(path);
                if (loadedShots != null)
                {
                    ordinary.AddRange(loadedShots);
                }
            }
            foreach (string path in Fragment1json)
            {
                List<shot> loadedShots = jsoncvpojo.LoadShotFromJson(path);
                if (loadedShots != null)
                {
                    Fragment1.AddRange(loadedShots);
                }
            }
            foreach (string path in Fragment2json)
            {
                List<shot> loadedShots = jsoncvpojo.LoadShotFromJson(path);
                if (loadedShots != null)
                {
                    Fragment2.AddRange(loadedShots);
                }
            }
        }
        //创建商品预制体
        private static void ceateShotPrefab()
        {
            if(UiMapData.shotpet == null)
            {
                Log.Error("[shotlord] 商品列表父节点未找到，请检查 UiMapData.shotpet 是否正确设置。");
            }
            ClearAllChildren(UiMapData.shotpet.transform);
            // 这里可以实现创建商品预制体的逻辑
            // 例如：根据 ordinary, Fragment1, Fragment2 列表生成对应的 UI 元素
            foreach (shot s in ordinary)
            {
                GameObject childInstance = GameObject.Instantiate(UiMapData.shop);
                shotData shot = childInstance.AddComponent<shotData>();
                shot.shot = s;
                shot.shoptype = "ordinary";
                childInstance.transform.SetParent(UiMapData.shotpet.transform, false);
                childInstance.SetActive(true);
                UiMapData.ordinaryOBJ.Add(childInstance);
            }
            foreach (shot s in Fragment1)
            {
                GameObject childInstance = GameObject.Instantiate(UiMapData.shop);
                shotData shot = childInstance.AddComponent<shotData>();
                shot.shot = s;
                shot.shoptype = "Fragment1";
                childInstance.transform.SetParent(UiMapData.shotpet.transform, false);
                childInstance.SetActive(true);
                UiMapData.Fragment1OBJ.Add(childInstance);
            }
            foreach (shot s in Fragment2)
            {
                GameObject childInstance = GameObject.Instantiate(UiMapData.shop);
                shotData shot = childInstance.AddComponent<shotData>();
                shot.shot = s;
                shot.shoptype = "Fragment2";
                childInstance.transform.SetParent(UiMapData.shotpet.transform, false);
                childInstance.SetActive(true);
                UiMapData.Fragment2OBJ.Add(childInstance);
            }
            ShopEvents.RaiseRefresh();
        }

        public static void ClearAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // 销毁子对象
                shotData shot = child.GetComponent<shotData>();
                if (shot != null)
                {
                    shot.delect();
                }
            }
        }
        public static void showpageUI(string pagetype)
        {
            foreach (GameObject obj in UiMapData.ordinaryOBJ)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in UiMapData.Fragment1OBJ)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in UiMapData.Fragment2OBJ)
            {
                obj.SetActive(false);
            }
            if (pagetype == "ordinary")
            {
                foreach (GameObject obj in UiMapData.ordinaryOBJ)
                {
                    obj.SetActive(true);
                }
                int hsiz = UiMapData.ordinaryOBJ.Count / 4 + 1;
                RectTransform rt = UiMapData.shotpet.GetComponent<RectTransform>();
                Vector2 size = rt.sizeDelta;
                size.y = 350f * hsiz;
                rt.sizeDelta = size;
            }
            else if (pagetype == "Fragment1")
            {
                foreach (GameObject obj in UiMapData.Fragment1OBJ)
                {
                    obj.SetActive(true);
                }
                int hsiz = UiMapData.Fragment1OBJ.Count / 4 + 1;
                RectTransform rt = UiMapData.shotpet.GetComponent<RectTransform>();
                Vector2 size = rt.sizeDelta;
                size.y = 350f * hsiz;
                rt.sizeDelta = size;
            }
            else if (pagetype == "Fragment2")
            {
                foreach (GameObject obj in UiMapData.Fragment2OBJ)
                {
                    obj.SetActive(true);
                }
                int hsiz = UiMapData.Fragment2OBJ.Count / 4 + 1;
                RectTransform rt = UiMapData.shotpet.GetComponent<RectTransform>();
                Vector2 size = rt.sizeDelta;
                size.y = 350f * hsiz;
                rt.sizeDelta = size;
            }
            UiMapData.dsptext.text = "";
        }

    }
}
