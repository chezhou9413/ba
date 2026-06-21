using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib.mainUI.Gaka.MonoComp
{
    public class MonoComp_CardshowList : MonoBehaviour
    {
        public float TargetSize = 2f;
        public float initSize = 6;
        public Vector2 offset = new Vector2(0, 50);
        public gacaData gacaData;
        public Image Image;
        void Start()
        {
            Image = this.transform.Find("MainImage").GetComponent<Image>();
            if (gacaData != null)
            {
                switch (gacaData.starNum)
                {
                    case 1:
                        Image.sprite = GakaMapData.studentcard1star;
                        break;
                    case 2:
                        Image.sprite = GakaMapData.studentcard2star;
                        break;
                    case 3:
                        Image.sprite = GakaMapData.studentcard3star;
                        break;
                }
            }
            Image.GetComponent<RectTransform>().anchoredPosition += offset;
            Image.transform.localScale = Vector3.one * initSize;
            Sequence cardSeq = DOTween.Sequence();
            cardSeq.Append(Image.transform.DOScale(TargetSize, 0.35f).SetEase(Ease.Linear));
            cardSeq.OnComplete(() =>
            {
                // 卡片拍下的瞬间执行的代码
                OnCardSlammed();
            });

            void OnCardSlammed()
            {
                switch (gacaData.starNum)
                {
                    case 2:
                        this.transform.Find("Yellow_lizi").transform.localScale = this.transform.Find("Yellow_lizi").transform.localScale * (TargetSize - (TargetSize * 0.2f));
                        this.transform.Find("Yellow_lizi").gameObject.SetActive(true);
                        break;
                    case 3:
                        this.transform.Find("Zise_lizi").transform.localScale = this.transform.Find("Zise_lizi").transform.localScale * (TargetSize - (TargetSize * 0.2f));
                        this.transform.Find("Zise_lizi").gameObject.SetActive(true);
                        break;
                }
            }
        }
    }
}
