using BANWlLib.mainUI.Mission.MonoComp;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.MonoComp
{
    public class uiListObj
    {
        public GameObject backObj;
        public string bgm;
    }
    public class MonoComp_BackButton : MonoBehaviour
    {
        public static MonoComp_BackButton instance;
        public Button button;
        public GameObject backObj;
        public string currentBgm = "bgm2"; 
        // 历史记录列表
        public List<uiListObj> backList = new List<uiListObj>();

        void Awake()
        {
            instance = this;
            button = GetComponent<Button>();
        }

        void Start()
        {
            button.onClick.AddListener(() =>
            {
                onback();
            });
        }
        public void setNewObj(GameObject newGameObj, string bgmnamn)
        {
            if (backObj != null)
            {
                uiListObj uiListObj = new uiListObj();
                uiListObj.backObj = backObj;
                uiListObj.bgm = currentBgm;
                backList.Add(uiListObj);
            }
            backObj = newGameObj;
            currentBgm = bgmnamn;
        }

        public void ClearAll()
        {
            foreach (uiListObj listObj in backList)
            {
                if (listObj.backObj != null)
                {
                    listObj.backObj.SetActive(false);
                }
            }
            if (backObj != null)
            {
                backObj.SetActive(false);
            }
            backObj = null;
            backList.Clear();
            currentBgm = "bgm2";
        }
        public void onback()
        {
            if (UiMapData.isLocKBack) return;

            // 1. 关闭当前正在显示的页面
            if (backObj != null)
            {
                backObj.SetActive(false);
            }

            // 2. 检查是否有历史记录（上一级页面）
            if (backList.Count > 0)
            {
                int lastIndex = backList.Count - 1;
                uiListObj lastEntry = backList[lastIndex];

                GameObject prevObj = lastEntry.backObj;
                string prevBgm = lastEntry.bgm;

                // 关键：必须重新激活上一个页面！
                if (prevObj != null)
                {
                    prevObj.SetActive(true);
                    backObj = prevObj; // 更新当前指针
                }

                if (!string.IsNullOrEmpty(prevBgm))
                {
                    LoopBGMManager.switchUiBgm(prevBgm);
                    currentBgm = prevBgm;
                }

                backList.RemoveAt(lastIndex);
            }
            else
            {
                // 3. 已经回到主页面，再次点击则是“退出整个UI回到游戏”
                Log.Message("已回到最底层，执行退出逻辑");
                ExitWholeUI();
            }
        }

        private void ExitWholeUI()
        {
            backObj = null;
            backList.Clear();

            // 安全检查，防止报错
            if (UiMapData.uiCamera != null) UiMapData.uiCamera.gameObject.SetActive(false);
            if (UiMapData.mainUI != null) UiMapData.mainUI.SetActive(false);

            UiMapData.uiclose = false;

            if (UiMapData.showUI != null)
            {
                UiMapData.showUI.SetActive(true);
            }
        }
    }
}