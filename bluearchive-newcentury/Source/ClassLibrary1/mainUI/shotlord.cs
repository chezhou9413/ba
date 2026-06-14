using BANWlLib.mainUI.pojo;
using newpro;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace BANWlLib
{
    /// <summary>
    /// 商店商品加载器负责读取商品 JSON、创建商品 UI 条目，并按商店分页显示商品。
    /// </summary>
    public static class shotlord
    {
        private static List<shot> ordinary = new List<shot>();
        private static List<shot> Fragment1 = new List<shot>();
        private static List<shot> Fragment2 = new List<shot>();

        /// <summary>
        /// 初始化商店商品数据和商品预制体。
        /// </summary>
        public static bool Initializeshotlord()
        {
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
            UiMapData.ordinaryOBJ.Clear();
            UiMapData.Fragment1OBJ.Clear();
            UiMapData.Fragment2OBJ.Clear();
        }

        /// <summary>
        /// 从商店 JSON 目录读取所有分页商品配置。
        /// </summary>
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

        /// <summary>
        /// 根据已加载的商品配置创建商品预制体实例。
        /// </summary>
        private static void ceateShotPrefab()
        {
            if (UiMapData.shotpet == null)
            {
                Log.Error("[shotlord] 商品列表父节点未找到，请检查 UiMapData.shotpet 是否正确设置。");
                return;
            }
            if (UiMapData.shop == null)
            {
                Log.Error("[shotlord] 商品预制体未加载，无法创建商品列表。");
                return;
            }
            ClearAllChildren(UiMapData.shotpet.transform);

            CreateShotPage(ordinary, "ordinary", UiMapData.ordinaryOBJ);
            CreateShotPage(Fragment1, "Fragment1", UiMapData.Fragment1OBJ);
            CreateShotPage(Fragment2, "Fragment2", UiMapData.Fragment2OBJ);
            ShopEvents.RaiseRefresh();
        }

        /// <summary>
        /// 为单个商店分页创建商品条目，单个商品失败时跳过该商品并保留其他商品。
        /// </summary>
        private static void CreateShotPage(List<shot> shotList, string shopType, List<GameObject> outputObjects)
        {
            foreach (shot s in shotList)
            {
                GameObject childInstance = null;
                try
                {
                    childInstance = GameObject.Instantiate(UiMapData.shop);
                    childInstance.SetActive(false);
                    childInstance.transform.SetParent(UiMapData.shotpet.transform, false);

                    shotData shotData = childInstance.GetComponent<shotData>() ?? childInstance.AddComponent<shotData>();
                    shotData.shot = s;
                    shotData.shoptype = shopType;

                    childInstance.SetActive(true);
                    outputObjects.Add(childInstance);
                }
                catch (Exception ex)
                {
                    string name = s != null ? s.ProductName : "空商品数据";
                    Log.Error($"[shotlord] 创建商品条目失败，分页={shopType}，商品={name}，错误={ex}");
                    if (childInstance != null)
                    {
                        UnityEngine.Object.Destroy(childInstance);
                    }
                }
            }
        }

        /// <summary>
        /// 清理商品列表父节点下的旧商品对象。
        /// </summary>
        public static void ClearAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                shotData shot = child.GetComponent<shotData>();
                if (shot != null)
                {
                    shot.delect();
                }
            }
        }

        /// <summary>
        /// 按分页类型显示对应商品，并调整滚动内容高度。
        /// </summary>
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
            if (UiMapData.dsptext != null)
            {
                UiMapData.dsptext.text = "";
            }
        }

    }
}
