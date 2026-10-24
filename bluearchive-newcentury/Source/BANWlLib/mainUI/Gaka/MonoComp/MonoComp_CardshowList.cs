#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
#if RW_1_7
using RimUnity.Tweening;
#else
using DG.Tweening;
#endif
using System.Linq;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using RimUnity.UI;
#else
using UnityEngine.UI;
#endif

namespace BANWlLib.mainUI.Gaka.MonoComp
{
    //准备抽取卡面并播放入场和落下特效。
    public class MonoComp_CardshowList : MonoBehaviour
    {
        public float TargetSize = 2f;
        public float initSize = 6;
        public Vector2 offset = new Vector2(0, 50);
        public gacaData gacaData;
        public Image Image;

        //实例创建当帧先隐藏卡面，避免 Start 前显示默认贴图和缩放。
        private void Awake()
        {
#if RW_1_7
            BANWlLib.mainUI.Initialization.BAUiParticles.Configure(gameObject);
#endif
            Image = this.transform.Find("MainImage").GetComponent<Image>();
            Image.gameObject.SetActive(false);
        }

        //数据由创建方赋值后，先设置初始画面再开始可见动画。
        void Start()
        {
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
            Image.gameObject.SetActive(true);
            Sequence cardSeq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            cardSeq.Append(Image.transform.DOScale(TargetSize, 0.35f).SetEase(Ease.Linear));
            cardSeq.OnComplete(() =>
            {
                // 卡片拍下的瞬间执行的代码
                OnCardSlammed();
            });

            //卡片落下时启动对应稀有度的粒子。
            void OnCardSlammed()
            {
                if (!BANWlLib.DamageFontSystem.Setting.DamageFontMod.settings.enableBurstParticle) return;
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
