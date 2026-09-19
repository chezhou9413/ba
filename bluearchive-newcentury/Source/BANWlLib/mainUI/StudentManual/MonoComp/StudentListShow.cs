using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.Tool;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    //展示学生列表条目并响应收集、出击与筛选状态。
    public class StudentListShow : MonoBehaviour
    {
        public Image avt;
        public UnityEngine.UI.Text studentNameText;
        public UnityEngine.UI.Text lvText;
        public GameObject back;
        public Button Collect;
        public StudentData studentData;
        public BaStudentUI BaStudentUI;
        public Button AvtButtom;
        public bool SelfisCollect = false;

        private readonly ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();

        //缓存学生列表条目的图片、文字和按钮。
        void Awake()
        {
            AvtButtom = transform.Find("avt").GetComponent<Button>();
            avt = transform.Find("avt").GetComponent<Image>();
            studentNameText = transform.Find("name").GetComponent<UnityEngine.UI.Text>();
            lvText = transform.Find("lvBack/Text").GetComponent<UnityEngine.UI.Text>();
            back = transform.Find("back").gameObject;
            Collect = transform.Find("Collect").GetComponent<Button>();
        }

        //绑定头像路径、星级与学生详情页交互。
        void Start()
        {
            StudentManualEvents.OnRefreshAllStudentData += UpDataStudentData;

            imgcvT2d.SetImage(avt, BaStudentUI.StudentAvatar);
            studentNameText.text = BaStudentUI.StudentName;
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

            UpDataStudentData();
        }

        //同步学生等级、解锁、收藏和筛选显示。
        void UpDataStudentData()
        {
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.StudentId);

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
            UpdateVisibleState();
        }

        //条目重新启用时同步学生状态。
        void OnEnable()
        {
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
