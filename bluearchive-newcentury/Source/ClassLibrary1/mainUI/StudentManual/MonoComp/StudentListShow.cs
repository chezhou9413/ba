using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.Tool;
using MyCoolMusicMod.MyCoolMusicMod;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
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

        void Awake()
        {
            AvtButtom = transform.Find("avt").GetComponent<Button>();
            avt = transform.Find("avt").GetComponent<Image>();
            studentNameText = transform.Find("name").GetComponent<UnityEngine.UI.Text>();
            lvText = transform.Find("lvBack/Text").GetComponent<UnityEngine.UI.Text>();
            back = transform.Find("back").gameObject;
            Collect = transform.Find("Collect").GetComponent<Button>();
        }

        void Start()
        {
            StudentManualEvents.OnRefreshAllStudentData += UpDataStudentData;

            avt.sprite = BaStudentUI.StudentAvatarSprite;
            studentNameText.text = BaStudentUI.StudentName;
            Collect.interactable = true;

            Collect.onClick.AddListener(() =>
            {
                if (!SelfisCollect)
                {
                    SelfisCollect = true;
                    if (!tracker.StudentCollect.Contains(BaStudentUI.RaceDefName))
                    {
                        tracker.StudentCollect.Add(BaStudentUI.RaceDefName);
                    }
                }
                else
                {
                    SelfisCollect = false;
                    ListUtils.RemoveStringAndClean(tracker.StudentCollect, BaStudentUI.RaceDefName);
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

        void UpDataStudentData()
        {
            StudentRosterUtility.SyncAllStudentRuntimeState(tracker);
            studentData = StudentRosterUtility.GetStudentData(tracker, BaStudentUI.RaceDefName);

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

        void OnEnable()
        {
            if (BaStudentUI == null)
            {
                return;
            }

            UpDataStudentData();
        }

        void OnDestroy()
        {
            StudentManualEvents.OnRefreshAllStudentData -= UpDataStudentData;
        }

        private void UpdateCollectState()
        {
            SelfisCollect = tracker.StudentCollect.Contains(BaStudentUI.RaceDefName);
            Collect.gameObject.GetComponent<Image>().color = SelfisCollect ? Color.yellow : Color.white;
        }

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
