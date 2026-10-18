using Spine;
using Spine.Unity;
using RimUnity;

//驱动阿罗娜眨眼和随语音变化的嘴型。
public class aronaSpineUI : MonoBehaviour
{
    private Bone mouthBone;
    private float[] samples;
    private float smoothRms;
    private float silenceTimer;

    public SkeletonGraphic graphic;
    public AudioSource audioSource;

    public float silenceThreshold = 0.03f;
    public float silenceHold = 0.12f;

    public float mouthCloseScale = 0.4f;
    public float mouthOpenScale = 1f;
    public float mouthAnimSpeed = 6f;

    public string MouseImage;
    public string defMouseImage;
    public bool isBlink = true;
    private float blinkDelay;
    public bool isMotou = true;
    public aronaSpineUIController controller;
    
    private void Awake()
    {
        graphic = GetComponent<SkeletonGraphic>();
        audioSource = GetComponent<AudioSource>();
        samples = new float[256];
    }

    private void Start()
    {
        mouthBone = graphic.Skeleton.FindBone(controller.MouseDefaultName);
        if (mouthBone == null)
        {
            Debug.LogError("找不到骨骼: Mouse_01");
            return;
        }
        graphic.UpdateLocal += ApplyMouthAnimation;
    }

    //用秒计时，避免高帧率缩短眨眼间隔，重开界面时重新开始等待。
    private void OnEnable() => blinkDelay = Random.Range(3.5f, 5f);

    //眨眼覆盖表情轨道，完整播放一次后让出眼部控制。
    void Update()
    {
        blinkDelay -= Time.deltaTime;
        if (blinkDelay <= 0)
        {
            blinkDelay = Random.Range(3.5f, 5f);
            if (isBlink && isMotou)
            {
                TrackEntry blink = graphic.AnimationState.SetAnimation(3, controller.bikeAnimName, false);
                blink.MixDuration = 0;
                graphic.AnimationState.AddEmptyAnimation(3, 0.1f, blink.Animation.Duration);
            }
        }
    }
    private void OnDestroy()
    {
        graphic.UpdateLocal -= ApplyMouthAnimation;
    }

    private void SetMouthAttachment(string attachmentName)
    {
        var slot = graphic.Skeleton.FindSlot(controller.MouseDefaultName);
        if (slot == null) return;

        var attachment = graphic.Skeleton.GetAttachment(slot.Data.Index, attachmentName);
        if (attachment == null) return;

        slot.Attachment = attachment;
    }
    //语音停止时复位一次并保持末帧，避免短复位动画循环拉动眼部。
    private void ApplyMouthAnimation(ISkeletonAnimation skeletonAnimation)
    {
        if (!audioSource.isPlaying)
        {
            TrackEntry current = graphic.AnimationState.GetCurrent(2);
            if (current == null || current.Animation.Name != controller.defAnimName)
                graphic.AnimationState.SetAnimation(2, controller.defAnimName, false);
            isBlink = true;
            SetMouthAttachment(controller.MouseDefaultName);
            mouthBone.ScaleX = Mathf.Lerp(mouthBone.ScaleX, mouthCloseScale, Time.deltaTime * 12f);
            return;
        }
        if (mouthBone == null) return;

        // 获取音量
        audioSource.GetOutputData(samples, 0);
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];

        float rms = Mathf.Sqrt(sum / samples.Length);
        smoothRms = Mathf.Lerp(smoothRms, rms, Time.deltaTime * 10f);


        if (smoothRms < silenceThreshold)
        {
            //低音量说话状态。
            silenceTimer += Time.deltaTime;

            if (silenceTimer >= silenceHold)
            {
                // 状态：确认安静 (设置闭嘴贴图)
                string attachment = !string.IsNullOrEmpty(defMouseImage) ? defMouseImage : controller.MouseDefaultName;
                SetMouthAttachment(attachment);

                // 缩放到闭合
                mouthBone.ScaleX = Mathf.Lerp(mouthBone.ScaleX, mouthCloseScale, Time.deltaTime * 20f);
            }
            else
            {
                if (!string.IsNullOrEmpty(MouseImage))
                {
                    SetMouthAttachment(MouseImage);
                }
                mouthBone.ScaleX = Mathf.Lerp(mouthBone.ScaleX, mouthCloseScale, Time.deltaTime * 20f);
            }
        }
        else
        {
            silenceTimer = 0f; // 重置计时器
            if (!string.IsNullOrEmpty(MouseImage))
            {
                SetMouthAttachment(MouseImage);
            }
            float t = (Mathf.Sin(Time.time * mouthAnimSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            mouthBone.ScaleX = Mathf.Lerp(mouthCloseScale, mouthOpenScale, t);
        }
    }
}
