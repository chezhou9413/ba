#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
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

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    public class StudentInfoType : MonoBehaviour
    {
        public GameObject StudentInfo;
        public GameObject StudentBio;
        private Button StudentInfoButton;
        private Text StudentInfoButtonText;
        private Button StudentBioButton;
        private Text StudentBioButtonText;
        private Color colorShow;
        private Color colorHide;
        void Start()
        {
            colorShow = new Color32(255, 255, 255, 255);
            colorHide = new Color32(255,255, 255, 0);
            StudentBioButton = this.transform.Find("StudentBioButton").GetComponent<Button>();
            StudentBioButtonText = this.transform.Find("StudentBioText").GetComponent<Text>();
            StudentInfoButton = this.transform.Find("StudentInfoButton").GetComponent<Button>();
            StudentInfoButtonText = this.transform.Find("StudentInfoText").GetComponent<Text>();
            StudentInfoButton.onClick.AddListener(() =>
            {
                setStudentInfo();
            });
            StudentBioButton.onClick.AddListener(() =>
            {
                setStudentBio();
            });
            StudentInfoButton.gameObject.SetActive(true);
            StudentBioButton.gameObject.SetActive(true);
            setStudentInfo();
        }

        void setStudentInfo()
        {
            StudentInfoButton.image.color = colorShow;
            StudentBioButton.image.color = colorHide;
            StudentBioButtonText.color = Color.white;
            StudentInfoButtonText.color = Color.black;
            StudentInfo.SetActive(true);
            StudentBio.SetActive(false);
        }

        void setStudentBio()
        {
            StudentInfoButton.image.color = colorHide;
            StudentBioButton.image.color = colorShow;
            StudentInfoButtonText.color = Color.white;
            StudentBioButtonText.color = Color.black;
            StudentInfo.SetActive(false);
            StudentBio.SetActive(true);
        }
    }
}
