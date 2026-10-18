using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using RimUnity;
using RimUnity.UI;
public class SoraSpineUI : MonoBehaviour
{
    public SkeletonGraphic graphic;
    public Button button;
    public Image targetImage;
    private Text targetIext;
    public List<AudioClip> audios = new List<AudioClip>();
    public List<string> soraText = new List<string>();
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();
    private SkeletonData skeletonData;
    private int lastValue = -1;

    private bool startSpine = false;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        button = GetComponent<Button>();
        skeletonData = graphic.SkeletonDataAsset.GetSkeletonData(true);
        targetIext = targetImage.transform.Find("Text").GetComponent<Text>();
        foreach (var clip in audios)
        {
            audioDict[clip.name] = clip;
        }
    }

    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnEnable()
    {
        if (graphic == null)
        {
            Debug.LogError("没有找到 SkeletonGraphic 组件！");
            return;
        }

        //初始化组件。
        graphic.Initialize(false);

        //清除上一次播放的状态。
        var state = graphic.AnimationState;
        state.ClearTracks();                    // 清除所有轨道（包括点击按钮留下的 Track 1）
        graphic.Skeleton.SetToSetupPose();      // 强制骨骼回到初始位置
        graphic.Skeleton.SetSlotsToSetupPose(); // 强制插槽回到初始状态（颜色、附件等）

        if (startSpine)
        {
            startSpine = !startSpine;
            state.SetAnimation(0, "Start01_Idle_01", false);
            state.AddAnimation(0, "Idle_01", true, 0);

            Spine.Animation anim = skeletonData.FindAnimation("Start01_Idle_01");
            // 加一个安全检查防止动画名写错导致报错
            if (anim != null)
            {
                StartFadeOut(anim.Duration, 0.5f, soraText[1]);
            }

            StartCoroutine(PlaySoundAfterDelay("Sora_Shop_In_2", 0.3f));
        }
        else
        {
            startSpine = !startSpine;
            state.SetAnimation(0, "Start02_Idle_01", false);
            state.AddAnimation(0, "Idle_01", true, 0);

            Spine.Animation anim = skeletonData.FindAnimation("Start02_Idle_01");
            if (anim != null)
            {
                StartFadeOut(anim.Duration, 0.5f, soraText[0]);
            }

            StartCoroutine(PlaySoundAfterDelay("Sora_Shop_In_1", 1f));
        }

        //立即刷新姿态，避免首帧显示 T-Pose。
        graphic.Update(0);
    }

    private IEnumerator PlaySoundAfterDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioDict.ContainsKey(key))
            audioSource.PlayOneShot(audioDict[key]);
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
        state.SetAnimation(0, "Talk_0"+value+"_A", false);
        state.SetAnimation(1, "Talk_0"+value+"_M", false);
        state.AddAnimation(0, "Idle_01", true, 0);
        Spine.Animation anim = skeletonData.FindAnimation("Talk_0" + value + "_A");
        StartFadeOut(anim.Duration, 0.5f, soraText[1+ value]);
        StartCoroutine(PlaySoundAfterDelay("Sora_Shop_Talk_" + value, 0.3f));
    }

    public int GetRandomNumber()
    {
        int newValue;
        do
        {
            newValue = Random.Range(1, 8);
        }
        while(newValue == lastValue);
        lastValue = newValue; // 更新记录
        return newValue;
    }
    public void StartFadeOut(float delay, float fadeDuration,string text)
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
