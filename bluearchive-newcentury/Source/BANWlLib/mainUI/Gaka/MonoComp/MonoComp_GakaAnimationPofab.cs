#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.BANWGamecomp;
using BANWlLib.DamageFontSystem.Setting;
using BANWlLib.uicreater.tool;
#if RW_1_7
using RimUnity.Tweening;
#else
using DG.Tweening;
#endif
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System.Collections;
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
#if RW_1_7
using RimUnity.Video;
#else
using UnityEngine.Video;
#endif
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

        //视频组件。
        public GameObject Clip1;
        public VideoPlayer videoPlayer;

        // 双击ESC跳过相关
        private float lastEscTime = -1f;
        private float doubleClickThreshold = 0.3f;
        private bool videoSkipped = false;
        private Tween delayedCallTween;
        private Sequence flashSequence;
        private Sequence resultTransitionSequence;

        //缓存组件引用。
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
            this.transform.Find("Clip55/poit/poit_cont").gameObject.GetComponent<Engine.UI.Text>().text = gamecomp.gacaPoit.ToString();
            Flash2 = this.transform.Find("Clip25").GetComponent<Image>();
            clip2 = this.transform.Find("Clip2").gameObject;
            Card = this.transform.Find("Clip2/Card").GetComponent<Image>();
            showEnd = this.transform.Find("Clip2/showEnd").GetComponent<Button>();
            Clip35 = this.transform.Find("Clip35").gameObject;
            Clip55 = this.transform.Find("Clip55").gameObject;
            showEnd.interactable = false;
            //获取 Clip1 和 VideoPlayer。
            Clip1 = this.transform.Find("Clip1").gameObject;
            videoPlayer = Clip1.GetComponent<VideoPlayer>();

            //缓存子对象引用。
            CacheChildReferences();

            //预热 GameObject。
            PrewarmGameObjects();

            //预加载卡面纹理。
            PreloadSprites();

            Button tryAgainBtn = Clip55.transform.Find("TryAgainButtom").GetComponent<Button>();

            int cost = GakaMapData.tenGacha ? 1200 : 120;
            int current = ItemUtility.GetTotalItemCount("QinghuiStone");
            bool isGodMode = Prefs.DevMode && DebugSettings.godMode;
            bool canAfford = isGodMode || current >= cost;

            tryAgainBtn.interactable = canAfford;

            if (!canAfford)
            {
                var btnText = tryAgainBtn.GetComponentInChildren<Engine.UI.Text>();
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

        //白幕完全遮住画面后再切换，使用同一动画时钟决定交接点。
        private void OnVideoEnd()
        {
            videoSkipped = true;
            FadeToWhite(Flash2, 2f).OnComplete(() => StartCoroutine(ActivateClip2AndSetup()));
        }

        //分帧激活卡片场景，闪光覆盖完成后释放开场视频。
        private IEnumerator ActivateClip2AndSetup()
        {
            //先激活下一幕，白幕在布局准备期间保持完全不透明。
            Card.transform.localScale = Vector3.one * 1.5f;
            Card.transform.localRotation = Quaternion.Euler(0, 0, -10);
            clip2.SetActive(true);
            Card.gameObject.SetActive(false);
            showEnd.gameObject.SetActive(false);

            yield return null; //等待下一幕完成首帧布局。

#if RW_1_7
            videoPlayer.Stop();
            Clip1.SetActive(false);
#endif
            FadeOutFlash(Flash2, 1f);

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
                    //保持白幕到下一幕就绪，不在两次闪光之间提前淡出。
                    FadeToWhite(Flash2, 0.5f);
                    Card.sprite = GakaMapData.cardFan;
                    Card.transform.localRotation = Quaternion.Euler(0, -90, 0);
                });
                cardSeq.Append(Card.transform.DOLocalRotate(new Vector3(0, 0, 0), flipSpeed).SetEase(Ease.Linear));
                cardSeq.Join(Card.transform.DOScale(scaleBig, flipSpeed).SetEase(Ease.OutCubic));
                cardSeq.Join(Card.transform.DOLocalMoveY(-700f, flipSpeed).SetEase(Ease.OutCubic));
                cardSeq.AppendCallback(() =>
                {
                    //低帧率可能跨过淡入时段，换景前明确保持白幕全不透明。
                    SetAlpha(Flash2, 1f);
                    Clip35.SetActive(true);
                    clip2.SetActive(false);
                    //只让底板承接白幕，退场后才启动可见动画。
                    FadeOutFlash(Flash2, 0.15f, BeginCardReveal);
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

        //白幕消失后再启动粒子和卡片入场，展示时长从实际可见时开始计算。
        private void BeginCardReveal()
        {
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
                MonoComp_CardshowList cardShow = gameObject.AddComponent<MonoComp_CardshowList>();
                cardShow.gacaData = GakaMapData.gacaDatas[0];
                GameObject result = Instantiate(GakaMapData.ResultList);
                result.transform.SetParent(danTransform55, false);
                MonoComp_ResultListShow resultShow = result.AddComponent<MonoComp_ResultListShow>();
                resultShow.gacaData = GakaMapData.gacaDatas[0];
            }
            else
            {
                StartCoroutine(CreateCardsWithDelay());
            }
            StartCoroutine(showResult());
        }

        //十连卡片按原有间隔入场，结果卡片分帧创建。
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

        //结果页在白幕峰值切入，黑色淡出层独立于即将销毁的上一幕。
        IEnumerator showResult()
        {
            yield return new WaitForSeconds(2f);
            clip45Image.transform.SetParent(transform, false);
            PlayWhiteInBlackOut(clip45Image, 0.5f, 0.4f, () =>
            {
                Clip55.SetActive(true);
                Destroy(Clip35);
            });
        }

        //替换根序列并保留当前透明度，让连续白幕从上一段自然接续。
        private Sequence FadeToWhite(Image image, float duration)
        {
            flashSequence?.Kill();
            image.gameObject.SetActive(true);
            flashSequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            flashSequence.Append(image.DOFade(1f, duration).SetEase(Ease.OutQuad));
            return flashSequence;
        }

        //白幕完全退场后再通知调用方启动需要完整展示的动画。
        private void FadeOutFlash(Image image, float duration, System.Action onRevealed = null)
        {
            flashSequence?.Kill();
            flashSequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            flashSequence.Append(image.DOFade(0f, duration).SetEase(Ease.InQuad));
            flashSequence.OnComplete(() =>
            {
                image.gameObject.SetActive(false);
                onRevealed?.Invoke();
            });
        }

        //在白幕完全遮挡时交接页面，再以独立黑幕淡出。
        public void PlayWhiteInBlackOut(Image image, float fadeInDuration, float fadeOutDuration, System.Action switchScene)
        {
            if (image == null) return;
            resultTransitionSequence?.Kill();

            Color tempColor = Color.white;
            tempColor.a = 0f;
            image.color = tempColor;

            image.gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            resultTransitionSequence = seq;

            seq.Append(image.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));

            seq.AppendCallback(() =>
            {
                switchScene();
                image.color = Color.black;
                image.transform.SetAsLastSibling();
            });

            seq.Append(image.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad))
               .OnComplete(() =>
               {
                   image.gameObject.SetActive(false);
               });
        }

        //界面关闭时取消仍持有该界面的白幕序列和开场计时。
        private void OnDestroy()
        {
            delayedCallTween?.Kill();
            flashSequence?.Kill();
            resultTransitionSequence?.Kill();
        }

        //设置初始透明度，不启动补间。
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
