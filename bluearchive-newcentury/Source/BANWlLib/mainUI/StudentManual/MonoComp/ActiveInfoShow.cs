using BANWlLib.BaDef;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    public class ActiveInfoShow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string ID;
        public BaStudentUI BaStudentUI;
        public GameObject Abilityinfo;
        public GameObject penter;
        private Ability ability;
        // 这个函数会在鼠标指针进入该对象的可交互区域时被自动调用
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowInfo();
        }

        // 这个函数会在鼠标指针离开该对象的可交互区域时被自动调用
        public void OnPointerExit(PointerEventData eventData)
        {
            hidderInfo();
        }

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
            Abilityinfo.transform.Find("Introduction").GetComponent<Text>().text = ability.AbilityIntroduction;
            penter.transform.Find("Heading").gameObject.SetActive(true);
        }

        void hidderInfo()
        {
            Abilityinfo.SetActive(false);
            penter.transform.Find("Heading").gameObject.SetActive(false);
        }
    }
}
