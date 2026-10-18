using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using RimUnity;
using RimUnity.UI;

public class RingSpineUI : MonoBehaviour
{
    public SkeletonGraphic graphic;
    public Button button;
    public Image targetImage;
    private Text targetIext;
    public List<AudioClip> audios = new List<AudioClip>();
    public List<string> RingText = new List<string>();
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();
    private SkeletonData skeletonData;
    private int lastValue = -1;

    private bool startSpine = true;
    //在界面启用前绑定交互控件。
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        skeletonData = graphic.SkeletonDataAsset.GetSkeletonData(true);
        targetIext = targetImage.transform.Find("Text").GetComponent<Text>();
        foreach (var clip in audios)
        {
            audioDict[clip.name] = clip;
        }
        RingText.Add("恭候光临\n老师。");
        RingText.Add("在基沃托斯发生的各种事件，\n希望能交由「夏莱」负责处理。");
        RingText.Add("我们马上开始吧。&&50");
        RingText.Add("我不能允许这样的事情存在。&&30");
        RingText.Add("没办法。\n因为现在只有「夏莱」是空闲的&&30");
        RingText.Add("我知道我让您很辛苦，\n老师。&&35");
        RingText.Add("……。\n辛苦了。&&11");
        RingText.Add("老师的活跃，\n值得学习。&&20");
    }

    private void OnEnable()
    {
        if (graphic == null)
        {
            Debug.LogError("没有找到 SkeletonGraphic 组件！");
            return;
        }

        //OnEnable 可能早于 Awake，需要先初始化组件。
        graphic.Initialize(false);

        //清空动画轨道。
        // 这会移除之前点击按钮产生的 Track 1 (口型) 动画，防止它覆盖 Track 0
        var state = graphic.AnimationState;
        state.ClearTracks();

        //将骨骼和插槽恢复到初始姿态。
        graphic.Skeleton.SetToSetupPose();
        graphic.Skeleton.SetSlotsToSetupPose(); //恢复插槽颜色和附件。

        if (startSpine)
        {
            startSpine = !startSpine;
            state.SetAnimation(0, "Start01_Idle_01", false);
            state.AddAnimation(0, "Idle_01", true, 0);
            Spine.Animation anim = skeletonData.FindAnimation("Start01_Idle_01");

            if (anim != null)
            {
                StartFadeOut(anim.Duration, 0.5f, RingText[0]);
            }
            StartCoroutine(PlaySoundAfterDelay("Rin_RaidLobby_In_1", 2.333f));
        }
        else
        {
            startSpine = !startSpine;
            state.SetAnimation(0, "Start02_Idle_01", false);
            state.AddAnimation(0, "Idle_01", true, 0);
            Spine.Animation anim = skeletonData.FindAnimation("Start02_Idle_01");
            if (anim != null)
            {
                StartFadeOut(anim.Duration, 0.5f, RingText[1]);
            }
            StartCoroutine(PlaySoundAfterDelay("Rin_RaidLobby_In_2", 3.533f));
        }
        graphic.Update(0);
    }
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (graphic == null) return;
        var state = graphic.AnimationState;
        var current = state.GetCurrent(0);
        if (current == null || current.Animation.Name != "Idle_01")
        {
            return;
        }
        int value = GetRandomNumber();
        state.SetAnimation(0, "Talk_0" + value + "_A", false);
        state.SetAnimation(1, "Talk_0" + value + "_M", false);
        state.AddAnimation(0, "Idle_01", true, 0);
        Spine.Animation anim = skeletonData.FindAnimation("Talk_0" + value + "_A");
        string[] separators = new string[] {"&&"};
        string[] A = RingText[1 + value].Split(separators, StringSplitOptions.None);
        float delay = float.Parse(A[1]);
        StartFadeOut(anim.Duration, 0.5f, A[0]);
        StartCoroutine(PlaySoundAfterDelay("Rin_RaidLobby_Talk_" + value, delay/30));
    }
    //按帧更新交互状态。
    void Update()
    {
        
    }

    private IEnumerator PlaySoundAfterDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioDict.ContainsKey(key))
            audioSource.PlayOneShot(audioDict[key]);
    }
    public int GetRandomNumber()
    {
        int newValue;
        int maxRange = RingText.Count - 1;
        if (maxRange <= 1) return 0;
        do
        {
            newValue = UnityEngine.Random.Range(1, maxRange);
        }
        while (newValue == lastValue);
        lastValue = newValue; // 更新记录
        return newValue;
    }
    public void StartFadeOut(float delay, float fadeDuration, string text)
    {
        delay -= 2f;
        targetIext.text = text;
        RimUnity.Color color = targetImage.color;
        RimUnity.Color color2 = targetIext.color;
        color.a = 1f;
        color2.a = 1f;
        targetImage.color = color;
        targetIext.color = color2;
        targetImage.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDisable(delay, fadeDuration));
    }

    private IEnumerator FadeOutAndDisable(float delay, float fadeDuration)
    {
        // 等待 delay 秒后开始淡出
        yield return new WaitForSeconds(delay);
        RimUnity.Color color = targetImage.color;
        RimUnity.Color color2 = targetIext.color;
        float startAlpha = color.a;
        float time = 0f;

        // 逐渐降低 alpha
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, 0f, t);
            color2.a = Mathf.Lerp(startAlpha, 0f, t);
            targetImage.color = color;
            targetIext.color = color2;
            yield return null;
        }

        color.a = 0f;
        targetImage.color = color;

        // 禁用整个物体
        targetImage.gameObject.SetActive(false);
    }
}
