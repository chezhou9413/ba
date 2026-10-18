using Spine.Unity;
using RimUnity;
using RimUnity.EventSystems;
using RimUnity.UI;

public class BoneDragControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public SkeletonGraphic skeletonGraphic;
    public Button InteractButton;
    [Header("1. 必须调整的设置")]
    [Tooltip("如果拖动方向不对，就旋转这个转盘")]
    [Range(-180f, 180f)]
    public float inputRotation = -90f;

    [Tooltip("移动范围限制")]
    public Vector2 limitX = new Vector2(-0.5f, 0.5f);
    public Vector2 limitY = new Vector2(-0.5f, 0.5f);

    [Header("2. 手感调节")]
    [Tooltip("长按多少秒才算拖拽")]
    public float longPressTime = 0.1f; //长按判定时间，单位为秒。

    [Tooltip("灵敏度")]
    public float sensitivity = 0.0015f;

    [Tooltip("平滑度")]
    public float smoothSpeed = 50.0f;
    public float restoreSpeed = 15.0f;

    public string boneName;
    public string DisableAnimation;
    public string ResetAnimation;
    private Vector2 startPos;
    private Vector2 targetOffset;
    private Vector2 currentSmoothedOffset;
    private bool isInitialized = false;

    private bool isDragging = false;       // 是否正在拖拽骨骼
    private bool isPointerDown = false;   // 手指是否按在屏幕上
    private float pressTimer = 0f;        // 按下的计时器
    
    private void Start()
    {
        if (skeletonGraphic != null)
            skeletonGraphic.UpdateLocal += MoveBone;
    }

    private void Update()
    {
        if (skeletonGraphic.AnimationState.GetCurrent(0).Animation.Name != ResetAnimation)
        {
            return;
        }
        // 如果按下了，且还没进入拖拽状态，就开始计时
        if (isPointerDown && !isDragging)
        {
            pressTimer += Time.deltaTime;

            // 如果时间超过了阈值，正式激活拖拽模式
            if (pressTimer >= longPressTime)
            {
                StartDragMode();
            }
        }
    }

    //按下时只标记状态。
    public void OnPointerDown(PointerEventData eventData)
    {
        if (skeletonGraphic.AnimationState.GetCurrent(0).Animation.Name == ResetAnimation)
        {
            isPointerDown = true;
            pressTimer = 0f; // 重置计时器
        }
    }

    // 辅助函数：激活拖拽逻辑
    void StartDragMode()
    {
        isDragging = true;
        skeletonGraphic.AnimationState.SetAnimation(1, DisableAnimation, true);
    }

    //拖拽开始后更新位置。
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 rawDelta = eventData.delta;
            Vector2 rotatedDelta = RotateVector(rawDelta, inputRotation);
            targetOffset += rotatedDelta * sensitivity;
            targetOffset.x = Mathf.Clamp(targetOffset.x, limitX.x, limitX.y);
            targetOffset.y = Mathf.Clamp(targetOffset.y, limitY.x, limitY.y);
        }
    }

    //松开时结束拖拽，未拖动则穿透点击。
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (isDragging)
        {
            isDragging = false;
            targetOffset = Vector2.zero;
            const float TransitionTime = 0.2f;
            //复位后保持末帧，不重复拉动骨骼。
            Spine.TrackEntry entry = skeletonGraphic.AnimationState.SetAnimation(1, ResetAnimation, false);
            entry.MixDuration = TransitionTime;
        }
        else
        {
            PassClickToUnderneath(eventData);
        }
    }

    //将点击传给下层对象。
    private void PassClickToUnderneath(PointerEventData data)
    {
       InteractButton.onClick.Invoke();
    }

    //移动骨骼。
    void MoveBone(ISkeletonAnimation anim)
    {
        var bone = anim.Skeleton.FindBone(boneName);
        if (bone == null) return;

        if (!isInitialized)
        {
            startPos = new Vector2(bone.X, bone.Y);
            isInitialized = true;
        }

        float speed = isDragging ? smoothSpeed : restoreSpeed;
        currentSmoothedOffset = Vector2.Lerp(currentSmoothedOffset, targetOffset, Time.deltaTime * speed);

        bone.X = startPos.x + currentSmoothedOffset.x;
        bone.Y = startPos.y + currentSmoothedOffset.y;
    }

    private Vector2 RotateVector(Vector2 v, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    private void OnDestroy()
    {
        if (skeletonGraphic != null)
            skeletonGraphic.UpdateLocal -= MoveBone;
    }
}
