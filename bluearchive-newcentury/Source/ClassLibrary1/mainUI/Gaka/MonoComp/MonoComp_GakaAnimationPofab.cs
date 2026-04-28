using BANWlLib.BANWGamecomp;
using BANWlLib.DamageFontSystem.Setting;
using BANWlLib.uicreater.tool;
using DG.Tweening;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Verse;

namespace BANWlLib.mainUI.Gaka.MonoComp
{
    public class MonoComp_GakaAnimationPofab : MonoBehaviour
    {
        public Image Flash2;
        public GameObject clip2;
        public Image Card;
        public Button showEnd;

        public GameObject Clip35;
        public GameObject Clip55;

        // 新增：视频相关
        public GameObject Clip1;
        public VideoPlayer videoPlayer;

        // 双击ESC跳过相关
        private float lastEscTime = -1f;
        private float doubleClickThreshold = 0.3f;
        private bool videoSkipped = false;
        private Tween delayedCallTween;

        // ========== 预缓存引用 ==========
        private GameObject redStart;
        private GameObject blueStart;
        private GameObject redLoop;
        private GameObject blueLoop;
        private GameObject redShan;
        private GameObject blueShan;
        private GameObject redShow;
        private GameObject blueShow;
        private GameObject cardTree;
        private GameObject cardOne;
        private Image clip45Image;
        private Transform danTransform;
        private Transform danTransform55;
        private Transform tenContentTransform;
        private Transform tenResContentTransform;

        void Start()
        {
            Gamecomp_gakaAction gamecomp = Current.Game.GetComponent<Gamecomp_gakaAction>();
            this.transform.Find("Clip55/poit/poit_cont").gameObject.GetComponent<UnityEngine.UI.Text>().text = gamecomp.gacaPoit.ToString();
            Flash2 = this.transform.Find("Clip25").GetComponent<Image>();
            clip2 = this.transform.Find("Clip2").gameObject;
            Card = this.transform.Find("Clip2/Card").GetComponent<Image>();
            showEnd = this.transform.Find("Clip2/showEnd").GetComponent<Button>();
            Clip35 = this.transform.Find("Clip35").gameObject;
            Clip55 = this.transform.Find("Clip55").gameObject;
            showEnd.interactable = false;
            // 新增：获取 Clip1 和 VideoPlayer
            Clip1 = this.transform.Find("Clip1").gameObject;
            videoPlayer = Clip1.GetComponent<VideoPlayer>();

            // ========== 预缓存所有子对象引用 ==========
            CacheChildReferences();

            // ========== 预热 GameObject ==========
            PrewarmGameObjects();

            // ========== 预加载卡面纹理 ==========
            PreloadSprites();

            Button tryAgainBtn = Clip55.transform.Find("TryAgainButtom").GetComponent<Button>();

            int cost = GakaMapData.tenGacha ? 1200 : 120;
            int current = ItemUtility.GetTotalItemCount("QinghuiStone");
            bool isGodMode = Prefs.DevMode && DebugSettings.godMode;
            bool canAfford = isGodMode || current >= cost;

            tryAgainBtn.interactable = canAfford;

            if (!canAfford)
            {
                var btnText = tryAgainBtn.GetComponentInChildren<UnityEngine.UI.Text>();
                if (btnText != null)
                {
                    btnText.text = $"青辉石不足 ({current}/{cost})";
                    btnText.color = Color.gray;
                }
            }

            tryAgainBtn.onClick.AddListener(() =>
            {
                LoopBGMManager.playEffAudio("鼠标点击音效");
                int currentCheck = ItemUtility.GetTotalItemCount("QinghuiStone");
                bool stillCanAfford = (Prefs.DevMode && DebugSettings.godMode) || currentCheck >= cost;

                if (!stillCanAfford)
                {
                    BamessageUI.ShowBaMessageUI("提示", $"青辉石不足！\n需要：{cost}\n当前：{currentCheck}", "确认");
                    return;
                }

                if (!(Prefs.DevMode && DebugSettings.godMode))
                {
                    if (!ItemUtility.TryRemoveItem("QinghuiStone", cost))
                    {
                        BamessageUI.ShowBaMessageUI("提示", "青辉石扣除失败！", "确认");
                        return;
                    }
                }

                if (!GakaMapData.tenGacha)
                {
                    Gakalord.danchou();
                }
                else
                {
                    Gakalord.shilianchou();
                }
                GameObject.Destroy(this.gameObject);
            });

            Clip55.transform.Find("BackButtom").GetComponent<Button>().onClick.AddListener(() =>
            {
                LoopBGMManager.playEffAudio("鼠标点击音效");
                UiMapData.isLocKBack = false;
                GameObject.Destroy(this.gameObject);
            });

            SetAlpha(Flash2, 0);

            delayedCallTween = DOVirtual.DelayedCall(6f, () =>
            {
                OnVideoEnd();
            });
        }

        private void CacheChildReferences()
        {
            Transform clip2T = clip2.transform;
            redStart = clip2T.Find("Red_Start")?.gameObject;
            blueStart = clip2T.Find("Blue_Start")?.gameObject;
            redLoop = clip2T.Find("Red_Loop")?.gameObject;
            blueLoop = clip2T.Find("Blue_Loop")?.gameObject;
            redShan = clip2T.Find("Red_Shan")?.gameObject;
            blueShan = clip2T.Find("Blue_Shan")?.gameObject;
            cardTree = clip2T.Find("Card/tree")?.gameObject;
            cardOne = clip2T.Find("Card/one")?.gameObject;

            if (Clip35 != null)
            {
                redShow = Clip35.transform.Find("Red_Show")?.gameObject;
                blueShow = Clip35.transform.Find("Blue_Shou")?.gameObject;
                clip45Image = Clip35.transform.Find("Clip45")?.GetComponent<Image>();
                danTransform = Clip35.transform.Find("dan");
                tenContentTransform = Clip35.transform.Find("ten/Content");
            }

            if (Clip55 != null)
            {
                danTransform55 = Clip55.transform.Find("dan");
                tenResContentTransform = Clip55.transform.Find("ten/Content");
            }
        }

        private void PrewarmGameObjects()
        {
            // 先预热 clip2 整体（最重的对象，包含 Canvas、所有子物体）
            // 无论当前 activeSelf 状态，都强制走一遍激活流程
            if (clip2 != null)
            {
                bool wasActive = clip2.activeSelf;
                clip2.SetActive(true);
                if (!wasActive) clip2.SetActive(false);
            }

            if (Clip35 != null)
            {
                bool wasActive = Clip35.activeSelf;
                Clip35.SetActive(true);
                if (!wasActive) Clip35.SetActive(false);
            }

            // 预热子对象（粒子、卡背标记等）
            GameObject[] toPrewarm = new GameObject[]
            {
                redStart, blueStart, redLoop, blueLoop,
                redShan, blueShan, cardTree, cardOne,
                redShow, blueShow
            };

            foreach (var go in toPrewarm)
            {
                if (go != null)
                {
                    bool wasActive = go.activeSelf;
                    go.SetActive(true);
                    if (!wasActive) go.SetActive(false);
                }
            }
        }

        private void PreloadSprites()
        {
            if (GakaMapData.cardZheng != null && GakaMapData.cardZheng.texture != null)
            {
                var _ = GakaMapData.cardZheng.texture.width;
            }
            if (GakaMapData.cardFan != null && GakaMapData.cardFan.texture != null)
            {
                var _ = GakaMapData.cardFan.texture.width;
            }
        }

        void Update()
        {
            if (!videoSkipped && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
            {
                float currentTime = Time.time;
                if (currentTime - lastEscTime <= doubleClickThreshold)
                {
                    SkipVideo();
                }
                lastEscTime = currentTime;
            }
        }

        private void SkipVideo()
        {
            if (videoSkipped) return;
            videoSkipped = true;

            if (delayedCallTween != null && delayedCallTween.IsActive())
            {
                delayedCallTween.Kill();
            }

            // 把视频跳转到末尾
            if (videoPlayer != null)
            {
                videoPlayer.time = 6.0;
            }

            OnVideoEnd();
        }

        private void OnVideoEnd()
        {
            videoSkipped = true;

            PlayFlashEffect(Flash2, 2f, 1f);
            DOVirtual.DelayedCall(2f, () =>
            {
                // 用协程分帧激活 clip2 及其子对象，避免同一帧堆积过多操作
                StartCoroutine(ActivateClip2AndSetup());
            });
        }

        /// <summary>
        /// 分帧激活 clip2，但保持与原始代码完全一致的 DelayedCall 时序
        /// </summary>
        private IEnumerator ActivateClip2AndSetup()
        {
            // 第1帧：激活 clip2
            Card.transform.localScale = Vector3.one * 1.5f;
            Card.transform.localRotation = Quaternion.Euler(0, 0, -10);
            clip2.SetActive(true);
            Card.gameObject.SetActive(false);
            showEnd.gameObject.SetActive(false);

            yield return null; // 等 clip2 的 Canvas rebuild 完成

            // 第2帧：激活循环粒子
            if (DamageFontMod.settings.enableBurstParticle)
            {
                if (GakaMapData.isP3)
                {
                    if (redLoop != null) redLoop.SetActive(true);
                }
                else
                {
                    if (blueLoop != null) blueLoop.SetActive(true);
                }
            }

            // 启动原始的 DelayedCall 时序（从此刻开始计时，和原始逻辑等效）
            DOVirtual.DelayedCall(0.3f, () =>
            {
                if (GakaMapData.isP3)
                {
                    if (DamageFontMod.settings.enableBurstParticle)
                    {
                        if (redStart != null) redStart.SetActive(true);
                    }
                    if (cardTree != null) cardTree.SetActive(true);
                }
                else
                {
                    if (DamageFontMod.settings.enableBurstParticle)
                    {
                        if (blueStart != null) blueStart.SetActive(true);
                    }
                    if (cardOne != null) cardOne.SetActive(true);
                }
            });
            DOVirtual.DelayedCall(0.7f, () =>
            {
                showEnd.interactable = true;
            });
            showEnd.onClick.AddListener(() =>
            {
                if (DamageFontMod.settings.enableBurstParticle)
                {
                    if (GakaMapData.isP3)
                    {
                        if (redShan != null) redShan.SetActive(true);
                    }
                    else
                    {
                        if (blueShan != null) blueShan.SetActive(true);
                    }
                }
                LoopBGMManager.playEffAudio("抽卡音频");
                if (cardTree != null) cardTree.SetActive(false);
                if (cardOne != null) cardOne.SetActive(false);
                showEnd.interactable = false;
                Card.sprite = GakaMapData.cardZheng;
                Vector3 scaleSmall = Vector3.one * 0.8f;
                Vector3 scaleBig = scaleSmall * 3f;
                float flipSpeed = 0.7f;
                Sequence cardSeq = DOTween.Sequence();
                cardSeq.Append(Card.transform.DOScale(scaleSmall, 0.4f).SetEase(Ease.OutQuad));
                cardSeq.Join(Card.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.4f).SetEase(Ease.OutQuad));
                cardSeq.Append(Card.transform.DOLocalRotate(new Vector3(0, 90, 0), flipSpeed).SetEase(Ease.Linear));
                cardSeq.AppendCallback(() =>
                {
                    PlayFlashEffect(Flash2, 0.5f, 0.3f);
                    DOVirtual.DelayedCall(0.5f, () => clip2.SetActive(false));
                    Card.sprite = GakaMapData.cardFan;
                    Card.transform.localRotation = Quaternion.Euler(0, -90, 0);
                });
                cardSeq.Append(Card.transform.DOLocalRotate(new Vector3(0, 0, 0), flipSpeed).SetEase(Ease.Linear));
                cardSeq.Join(Card.transform.DOScale(scaleBig, flipSpeed).SetEase(Ease.OutCubic));
                cardSeq.Join(Card.transform.DOLocalMoveY(-700f, flipSpeed).SetEase(Ease.OutCubic));
                cardSeq.AppendCallback(() =>
                {
                    Clip35.SetActive(true);
                    if (GakaMapData.isP3)
                    {
                        if (redShow != null) redShow.SetActive(true);
                    }
                    else
                    {
                        if (blueShow != null) blueShow.SetActive(true);
                    }
                    if (!GakaMapData.tenGacha)
                    {
                        GameObject gameObject = Instantiate(GakaMapData.showGaList);
                        gameObject.transform.SetParent(danTransform, false);
                        MonoComp_CardshowList monoComp_CardshowList = gameObject.AddComponent<MonoComp_CardshowList>();
                        monoComp_CardshowList.gacaData = GakaMapData.gacaDatas[0];
                        GameObject resObj = Instantiate(GakaMapData.ResultList);
                        resObj.transform.SetParent(danTransform55, false);
                        MonoComp_ResultListShow monoComp_ResultListShow = resObj.AddComponent<MonoComp_ResultListShow>();
                        monoComp_ResultListShow.gacaData = GakaMapData.gacaDatas[0];
                    }
                    else if (GakaMapData.tenGacha)
                    {
                        StartCoroutine(CreateCardsWithDelay());
                    }
                    PlayFlashEffect(Flash2, 1f, 0.5f);
                    StartCoroutine(showResult());
                });
            });
            DOVirtual.DelayedCall(0.5f, () =>
            {
                Card.gameObject.SetActive(true);
                showEnd.gameObject.SetActive(true);
                float animDuration = 0.5f;
                Sequence cardSeq = DOTween.Sequence();
                cardSeq.Append(Card.transform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack));
                cardSeq.Join(Card.transform.DOLocalRotate(new Vector3(0, 0, 0), animDuration).SetEase(Ease.OutQuad));
                cardSeq.Append(Card.transform.DOLocalRotate(new Vector3(30, 0, 0), animDuration).SetEase(Ease.OutQuad));
                if (GakaMapData.isP3)
                {
                    if (cardTree != null) cardTree.SetActive(true);
                }
                else
                {
                    if (cardOne != null) cardOne.SetActive(true);
                }
            });
        }

        IEnumerator CreateCardsWithDelay()
        {
            foreach (gacaData data in GakaMapData.gacaDatas)
            {
                GameObject newCard = Instantiate(GakaMapData.showGaList);
                newCard.transform.SetParent(tenContentTransform, false);
                MonoComp_CardshowList monoComp = newCard.AddComponent<MonoComp_CardshowList>();
                monoComp.gacaData = data;
                monoComp.TargetSize = 1.2f;
                monoComp.initSize = 4;
                monoComp.offset = Vector2.zero;

                yield return null; // 分帧：卡片和结果分开实例化

                GameObject resObj = Instantiate(GakaMapData.ResultList);
                resObj.transform.SetParent(tenResContentTransform, false);
                MonoComp_ResultListShow monoComp_ResultListShow = resObj.AddComponent<MonoComp_ResultListShow>();
                monoComp_ResultListShow.gacaData = data;
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerator showResult()
        {
            yield return new WaitForSeconds(2f);
            if (clip45Image != null)
            {
                PlayWhiteInBlackOut(clip45Image, 0.5f, 0.4f);
            }
            yield return new WaitForSeconds(0.6f);
            Destroy(Clip35);
            Clip55.gameObject.SetActive(true);
        }

        public void PlayFlashEffect(Image image, float flashInDuration, float fadeOutDuration)
        {
            if (image == null) return;
            image.DOKill();
            Color tempColor = image.color;
            tempColor.a = 0f;
            image.color = tempColor;
            image.gameObject.SetActive(true);
            Sequence seq = DOTween.Sequence();
            seq.Append(image.DOFade(1f, flashInDuration).SetEase(Ease.OutQuad));
            seq.Append(image.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad))
               .OnComplete(() =>
               {
                   image.gameObject.SetActive(false);
               });
        }

        public void PlayWhiteInBlackOut(Image image, float fadeInDuration, float fadeOutDuration)
        {
            if (image == null) return;

            image.DOKill();

            Color tempColor = Color.white;
            tempColor.a = 0f;
            image.color = tempColor;

            image.gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence();

            seq.Append(image.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));

            seq.AppendCallback(() =>
            {
                image.color = Color.black;
                image.transform.SetAsLastSibling();
            });

            seq.Append(image.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad))
               .OnComplete(() =>
               {
                   image.gameObject.SetActive(false);
               });
        }

        private void SetAlpha(Image img, float alpha)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }
    }
}