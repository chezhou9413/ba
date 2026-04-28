using HarmonyLib;
using MyCoolMusicMod;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Verse;
using Verse.AI;
namespace newpro
{
    public class gameobjcreater
    {
        public static void carddataspwn(GameObject petui,string imagepath)
        {
            GameObject one = UiMapData.shilianjiesuan.transform.Find("one").gameObject;       
            GameObject ten = UiMapData.shilianjiesuan.transform.Find("ten").gameObject;         
            GameObject colseall = UiMapData.shilianjiesuan.transform.Find("fiks").gameObject;
            GameObject list = ten.transform.Find("Viewport").transform.Find("Content").gameObject;
            GameObject showUI = petui.transform.Find("showUI").gameObject;
            GameObject closeUI = petui.transform.Find("closeUI").gameObject;
            GameObject UIcore = petui.transform.Find("UIcore").gameObject;
            GameObject shiping = petui.transform.Find("shiping").gameObject;
            RectTransform rectTransformshipin = shiping.GetComponent<RectTransform>();
            rectTransformshipin.sizeDelta = new Vector2(0, 0);
            UiMapData.shilianjiesuan.SetActive(true);
            UIcore.SetActive(false);
            showUI.SetActive(false);
            closeUI.SetActive(false);
            if (UiMapData.carddata.Count == 1)
            {
                ten.SetActive(false);
                one.SetActive(true);
                if (UiMapData.ImagraceMap.TryGetValue(UiMapData.carddata[0], out string raceimage) && !string.IsNullOrEmpty(raceimage))
                {
                    Sprite sprite = imgcvT2d.LoadSpriteFromFile(raceimage);
                    if (sprite != null)
                    {
                        Image image = one.GetComponentInChildren<Image>();
                        if (image != null)
                        {
                            image.sprite = sprite;
                        }
                    }
                }
            }
            else if (UiMapData.carddata.Count == 10)
            {
                one.SetActive(false);
                ten.SetActive(true);

                foreach (string item in UiMapData.carddata)
                {
                    GameObject childInstance = GameObject.Instantiate(UiMapData.card);
                    childInstance.transform.SetParent(list.transform, false);
                    if (UiMapData.ImagraceMap.TryGetValue(item, out string raceimage) && !string.IsNullOrEmpty(raceimage))
                    {
                        Sprite sprite = imgcvT2d.LoadSpriteFromFile(raceimage);
                        if (sprite != null)
                        {
                            Image image = childInstance.GetComponentInChildren<Image>();
                            if (image != null)
                            {
                                image.sprite = sprite;
                            }
                        }
                    }
                }
            }
            var closeBtn = colseall.GetComponent<Button>();
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() =>
            {
                UiMapData.uiclose = true;
                UiMapData.shilianjiesuan.SetActive(false);
                for (int i = list.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = list.transform.GetChild(i);
                    GameObject.Destroy(child.gameObject);
                }
                PawnDropHelper.DropPawnsByDefNames(UiMapData.carddata);
                UiMapData.carddata.Clear();
            });

        }

        public static void UIBodychek(string defname)
        {
            GameObject BauiUI = GameObject.Find("BauiUI");
            if (BauiUI == null)
            {
                return;
            }

            GameObject UIcore = BauiUI.transform.Find("UIcore")?.gameObject;
            if (UIcore == null)
            {
                return;
            }

            GameObject pet = UIcore.transform.Find("choukamianban")?.gameObject;
            if (pet == null)
            {
                return;
            }

            GameObject obj1 = pet.transform.Find("Text_datashow")?.gameObject;
            GameObject obj2 = pet.transform.Find("Text_titleshou")?.gameObject;
            GameObject obj3 = pet.transform.Find("Text_upshow")?.gameObject;
            GameObject obj4 = pet.transform.Find("Text_dexshow")?.gameObject;

            if (obj1 == null || obj2 == null || obj3 == null || obj4 == null)
            {
                return;
            }

            UnityEngine.UI.Text Text_datashow = obj1.GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text Text_titleshow = obj2.GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text Text_upshow = obj3.GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text Text_dexshow = obj4.GetComponent<UnityEngine.UI.Text>();

            if (Text_datashow == null || Text_titleshow == null || Text_upshow == null || Text_dexshow == null)
            {
                return;
            }

            if (!UiMapData.BodyMap.ContainsKey(defname))
            {
                return;
            }

            UIbody uIbody = UiMapData.BodyMap[defname];
            if (uIbody == null)
            {
                return;
            }

            // 实际赋值
            Text_datashow.text = uIbody.datashow;
            Text_titleshow.text = uIbody.titleshow;
            Text_upshow.text = uIbody.upshow;
            Text_dexshow.text = uIbody.dexshow;
        }

        public static void listPoitshot(string petpath, string poitjson, GameObject poitcont)
        {
       
            List<poitlist> poitlists = jsoncvpojo.LoadPoitlistFromJson(poitjson);
            if (poitlists == null || poitlists.Count == 0)
            {
                Log.Error("未成功找到json");
                return;
            }
            if (poitcont == null)
            {
                Log.Error("未成功找到点数商店列表父节点");
                return;
            }
            foreach (poitlist poit in poitlists)
            {
                GameObject childInstance = GameObject.Instantiate(UiMapData.poitlist);
                childInstance.transform.SetParent(poitcont.transform);
                UnityEngine.UI.Text text = childInstance.transform.Find("avtname").GetComponent<UnityEngine.UI.Text>();
                text.text = poit.characterName;
                Sprite texture2D = imgcvT2d.LoadSpriteFromFile(petpath + "\\" + poit.characterImage + ".png");
                Image image = childInstance.transform.Find("avt").GetComponent<Image>();
                image.sprite = texture2D;
                Button button = childInstance.transform.Find("goumai").GetComponent<Button>();
                if (button == null)
                {
                    Log.Error("未成功找到点数商店按钮");
                    return;
                }
                ButtomEffect buttom = button.gameObject.AddComponent<ButtomEffect>();
                buttom.ShangdianButt = button;
                button.onClick.AddListener(() =>
                {
                    if (Current.Game.GetComponent<newpro.PoitSaveComponent>().poit < 200)
                    {
                        return;
                    }
                    Current.Game.GetComponent<PoitSaveComponent>().poit -= 200;
                    UiMapData.carddata.Clear();
                    UiMapData.carddata.Add(poit.characterRace);
                    PawnDropHelper.DropPawnsByDefNames(UiMapData.carddata);
                });
                int hsiz = poitlists.Count / 2 + 1;
                RectTransform rt = poitcont.GetComponent<RectTransform>();
                Vector2 size = rt.sizeDelta;
                size.y = 250f * hsiz;
                rt.sizeDelta = size;
            }
        }

        public static void InitBody(List<string> bodyjson)
        {
            List<UIbody> UIheads = new List<UIbody>();
            for (int i = 0; i < bodyjson.Count; i++)
            {
                UIheads.Add(jsoncvpojo.LoadUIbodyFromJson(bodyjson[i]));
            }
            foreach (UIbody body in UIheads)
            {
                UiMapData.BodyMap[body.defName] = body;
            }
        }
    }
}
