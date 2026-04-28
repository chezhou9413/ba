using BANWlLib.Tool;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.Gaka.MonoComp
{
    public class MonoComp_ResultListShow : MonoBehaviour
    {
        public gacaData gacaData;
        public Image Image;
        public GameObject CharacteShow;
        public GameObject ItemShow;
        public Dictionary<ThingDef, int> goodThings = new Dictionary<ThingDef, int>();
        void Start()
        {
            goodThings.AddRange(gacaData.BaStudentRaceDef.baStudentData.GakaStudentThingData);
            Image = this.transform.Find("back").GetComponent<Image>();
            CharacteShow = this.transform.Find("back/CharacteShow").gameObject;
            ItemShow = this.transform.Find("back/ItemShow").gameObject;
            if (gacaData != null)
            {
                switch (gacaData.starNum)
                {
                    case 1:
                        Image.sprite = GakaMapData.gtwenhaoback;
                        break;
                    case 2:
                        Image.sprite = GakaMapData.srwenhaoback;
                        break;
                    case 3:
                        Image.sprite = GakaMapData.srrwenhaoback;
                        break;
                }
            }
            StartCoroutine(showStudnt());
            if (!gacaData.isNew)
            {
                StartCoroutine(IterateForever());
            }
        }
        IEnumerator IterateForever()
        {
            yield return new WaitForSeconds(2f);

            while (true)
            {
                if (goodThings.Count == 0)
                {
                    yield return null;
                    continue;
                }
                ItemShow.SetActive(true);
                CharacteShow.SetActive(false);

                foreach (KeyValuePair<ThingDef, int> entry in goodThings)
                {
                    ThingDef thing = entry.Key;
                    int count = entry.Value;

                    ItemShow.transform.localScale = Vector3.zero;
                    ItemShow.transform.Find("ItemIcon").GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetSpriteFromThingDef(thing);
                    ItemShow.transform.Find("ItemCont").GetComponent<UnityEngine.UI.Text>().text = "X" + count;

                    // 动画播放
                    ItemShow.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                    // 等待一段时间让玩家看清
                    yield return new WaitForSeconds(1f);
                }
                ItemShow.SetActive(false);
                CharacteShow.SetActive(true);

                CharacteShow.transform.localScale = Vector3.zero;
                CharacteShow.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                // 人物停留时间
                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator showStudnt()
        {
            yield return new WaitForSeconds(0.1f);
            Sequence cardSeq = DOTween.Sequence();
            cardSeq.Append(Image.transform.DOLocalRotate(new Vector3(0, 90, 0), 0.2f).SetEase(Ease.Linear));
            cardSeq.AppendCallback(() =>
            {
                Image.sprite = GakaMapData.gtback;
                Image Imageback = CharacteShow.GetComponent<Image>();
                Image.transform.localRotation = Quaternion.Euler(0, -90, 0);
                CharacteShow.SetActive(true);
                Image star = CharacteShow.transform.Find("StarShow").GetComponent<Image>();
                CharacteShow.transform.Find("Mask/Avatar").GetComponent<Image>().sprite = gacaData.gakaAvt;
                switch (gacaData.starNum)
                {
                    case 1:
                        Imageback.sprite = GakaMapData.gtback;
                        star.sprite = GakaMapData.gtstar;
                        break;
                    case 2:
                        Imageback.sprite = GakaMapData.srback;
                        star.sprite = GakaMapData.srstar;
                        break;
                    case 3:
                        Imageback.sprite = GakaMapData.srrback;
                        star.sprite = GakaMapData.srrstar;
                        this.transform.Find("kapai_idle_cai_chai").gameObject.SetActive(true);
                        break;
                }
            });
            cardSeq.Append(Image.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.Linear));
            cardSeq.AppendCallback(() =>
            {
                if (gacaData.isNew)
                {
                    Transform newTagTransform = this.transform.Find("back/new");
                    newTagTransform.gameObject.SetActive(true);
                    Image image = newTagTransform.GetComponent<Image>();
                    newTagTransform.localScale = Vector3.one;
                    newTagTransform.DOScale(1.1f, 0.8f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            });
        }
    }
}
