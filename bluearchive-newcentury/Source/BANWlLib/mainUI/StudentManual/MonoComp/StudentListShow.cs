#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.Tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
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
using Verse;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    //展示学生列表条目并响应收集、出击与筛选状态。
    public class StudentListShow : MonoBehaviour
    {
        public Image avt;
        public Engine.UI.Text studentNameText;
        public Engine.UI.Text lvText;
        public GameObject back;
        public Button Collect;
        public StudentData studentData;
        public BaStudentUI BaStudentUI;
        public Button AvtButtom;
        public bool SelfisCollect = false;
#if RW_1_7
        private bool pooled;

        //复用卡片时仅绑定当前学生；名册同步、筛选与排序由网格统一执行。
        internal void BindStudent(BaStudentUI student, StudentData data)
        {
            pooled = true;
            BaStudentUI = student;
            studentData = data;
            studentNameText.text = student.StudentName;
            Images.BAUIImageCache.SetPooledImage(avt, student.StudentAvatar);
            UpdateStudentState();
        }
#endif

        private readonly ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();

        //缓存学生列表条目的图片、文字和按钮。
        void Awake()
        {
            AvtButtom = transform.Find("avt").GetComponent<Button>();
            avt = transform.Find("avt").GetComponent<Image>();
            studentNameText = transform.Find("name").GetComponent<Engine.UI.Text>();
            lvText = transform.Find("lvBack/Text").GetComponent<Engine.UI.Text>();
            back = transform.Find("back").gameObject;
            Collect = transform.Find("Collect").GetComponent<Button>();
        }

        //绑定头像路径、星级与学生详情页交互。
        void Start()
        {
#if RW_1_7
            if (!pooled)
            {
#endif
            StudentManualEvents.OnRefreshAllStudentData += UpDataStudentData;

            imgcvT2d.SetImage(avt, BaStudentUI.StudentAvatar);
            studentNameText.text = BaStudentUI.StudentName;
#if RW_1_7
            }
#endif
            Collect.interactable = true;

            Collect.onClick.AddListener(() =>
            {
                if (!SelfisCollect)
                {
                    SelfisCollect = true;
                    if (!tracker.StudentCollect.Contains(BaStudentUI.StudentId))
                    {
                        tracker.StudentCollect.Add(BaStudentUI.StudentId);
                    }
                }
                else
                {
                    SelfisCollect = false;
                    ListUtils.RemoveStringAndClean(tracker.StudentCollect, BaStudentUI.StudentId);
                }

                UpdateCollectState();
                StudentManualEvents.RaiseRefresh();
                StudentManualEvents.resetUIlistRefresh();
            });

            AvtButtom.onClick.AddListener(() =>
            {
                StudentDetailsLord.ShowStudentDetail(BaStudentUI);
                LoopBGMManager.playEffAudio("鼠标点击音效");
            });

#if RW_1_7
            if (!pooled)
#endif
                UpDataStudentData();
        }

        //同步学生等级、解锁、收藏和筛选显示。
        void UpDataStudentData()
        {
            studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.StudentId);
            //每张卡片只同步自己的学生，避免整份名册被重复扫描。
            StudentRosterUtility.SyncStudentRuntimeState(tracker, studentData);
            UpdateStudentState();
            UpdateVisibleState();
        }

        //根据已同步的数据更新卡片内容，不改变虚拟网格分配的显示位置。
        private void UpdateStudentState()
        {
            if (studentData != null)
            {
                lvText.text = "Lv." + studentData.StudentLv;
                back.SetActive(false);
                Collect.gameObject.SetActive(true);
            }
            else
            {
                lvText.text = "Lv.1";
                back.SetActive(true);
                Collect.gameObject.SetActive(false);
            }

            UpdateCollectState();
        }

        //条目重新启用时同步学生状态。
        void OnEnable()
        {
#if RW_1_7
            if (pooled) return;
#endif
            if (BaStudentUI == null)
            {
                return;
            }

            UpDataStudentData();
        }

        //解除学生状态刷新事件订阅。
        void OnDestroy()
        {
            StudentManualEvents.OnRefreshAllStudentData -= UpDataStudentData;
        }

        //同步学生收藏状态与收藏按钮。
        private void UpdateCollectState()
        {
            SelfisCollect = tracker.StudentCollect.Contains(BaStudentUI.StudentId);
            Collect.gameObject.GetComponent<Image>().color = SelfisCollect ? Color.yellow : Color.white;
        }

        //按当前手册筛选规则更新条目显示。
        private void UpdateVisibleState()
        {
            switch (ManualMapData.selectStudentIndex)
            {
                case 0:
                    gameObject.SetActive(true);
                    break;
                case 1:
                    gameObject.SetActive(studentData != null);
                    break;
                case 2:
                    gameObject.SetActive(studentData == null);
                    break;
                default:
                    gameObject.SetActive(true);
                    break;
            }
        }

        public bool HasOwnedStudent()
        {
            return studentData != null;
        }

        public bool IsStudentAvailable()
        {
            if (studentData == null)
            {
                return false;
            }

            if (studentData.StudentPawn == null || studentData.StudentPawn.DestroyedOrNull())
            {
                return true;
            }

            return !studentData.StudentPawn.Dead;
        }

        public int GetSortLevel()
        {
            if (studentData == null)
            {
                return 0;
            }

            return studentData.StudentLv;
        }
    }
}
