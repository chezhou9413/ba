using System.Collections;
using System.Collections.Generic;
using RimUnity;
using RimUnity.UI;

public class aronaSpineUIController : MonoBehaviour
{
    public AudioSource audioSource;
    public Button Button;
    public aronaSpineUI AronaSpineUI;
    public AudioClip audioClip;

    public Image targetImage;
    private Text targetIext;
    
    public List<aronaAnimation> oneClick = new List<aronaAnimation>();
    public List<aronaAnimation> start = new List<aronaAnimation>();
    public bool isStartPlay = false;
    public string MouseDefaultName;
    public string defAnimName;
    public string bikeAnimName;
    public int nextIndex = 0;
    public int nextIndex2 = 0;
    //在界面启用前绑定交互控件。
    void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(() =>
        {
            ControllerArona();
        });
        targetIext = targetImage.transform.Find("Text").GetComponent<Text>();
    }

    void OnEnable()
    {
        StartCoroutine(PlayOnEnableRoutine());
    }

    private IEnumerator PlayOnEnableRoutine()
    {
        // 等一帧，让 Awake 等初始化先跑
        yield return null;

        // 额外再检查一次依赖，避免 null
        if (start == null || start.Count == 0)
        {
            Debug.LogWarning("aronaSpineUIController: start 列表为空，无法播放。");
            yield break;
        }
        int count = start.Count;
        if (count <= 1)
        {
            nextIndex2 = 0;
        }
        else
        {
            nextIndex2 = (nextIndex2 + Random.Range(1, count)) % count;
        }
        var anim = start[nextIndex2];
        if (anim == null)
        {
            Debug.LogWarning("aronaSpineUIController: 从 start 列表取到 null 元素。");
            yield break;
        }

        PlayArona(anim);
    }
    public void ControllerArona()
    {
        if (!audioSource.isPlaying)
        {
            int count = oneClick.Count;
            if (count <= 1)
            {
                nextIndex = 0;
            }
            else
            {
                nextIndex = (nextIndex + Random.Range(1, count)) % count;
            }
            PlayArona(oneClick[nextIndex]);
        }
    }

    public void PlayArona(aronaAnimation aronaAnimation)
    {
        //对话表情只播放一次，语音结束后由口型组件恢复待机，避免短动作反复抽动。
        AronaSpineUI.graphic.AnimationState.SetAnimation(2, aronaAnimation.spineAnimationName, false);
        audioSource.PlayOneShot(aronaAnimation.aronaAudioClip);
        AronaSpineUI.MouseImage = aronaAnimation.Mouth_Tex;
        AronaSpineUI.defMouseImage = aronaAnimation.defMouse_Tex;
        AronaSpineUI.isBlink = aronaAnimation.isBlink;
        StartFadeOut(aronaAnimation.aronaAudioClip.length + 3f, 0.5f, aronaAnimation.text);
    }
    //按帧更新交互状态。
    void Update()
    {

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
