using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using BANWlLib.BaDef;
using RimUnity;
using RimUnity.EventSystems;
using RimUnity.UI;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    //显示学生技能悬浮信息，并让长描述适配文本框。
    public class ActiveInfoShow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string ID;
        public BaStudentUI BaStudentUI;
        public GameObject Abilityinfo;
        public GameObject penter;
        private Ability ability;
        //鼠标移入技能按钮时显示详情。
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowInfo();
        }

        //鼠标离开技能按钮时隐藏详情。
        public void OnPointerExit(PointerEventData eventData)
        {
            hidderInfo();
        }

        //填充当前技能信息，以原字号为上限自动缩小过长的描述。
        void ShowInfo()
        {
            switch (ID)
            {
                case "A1":
                    ability = BaStudentUI.Ability1;
                    break;
                case "A2":
                    ability = BaStudentUI.Ability2;
                    break;
                case "A3":
                    ability = BaStudentUI.Ability3;
                    break;
                case "A4":
                    ability = BaStudentUI.Ability4;
                    break;
            }
            Abilityinfo.SetActive(true);
            Abilityinfo.transform.Find("Title").GetComponent<Text>().text = ability.AbilityTitle;
            Abilityinfo.transform.Find("Subtitle").GetComponent<Text>().text = ability.AbilitySubtitle;
            Text introduction = Abilityinfo.transform.Find("Introduction").GetComponent<Text>();
            introduction.resizeTextMaxSize = introduction.fontSize;
            introduction.resizeTextMinSize = 1;
            introduction.resizeTextForBestFit = true;
            introduction.text = ability.AbilityIntroduction;
            penter.transform.Find("Heading").gameObject.SetActive(true);
        }

        //关闭技能详情和对应按钮的高亮。
        void hidderInfo()
        {
            Abilityinfo.SetActive(false);
            penter.transform.Find("Heading").gameObject.SetActive(false);
        }
    }
}
