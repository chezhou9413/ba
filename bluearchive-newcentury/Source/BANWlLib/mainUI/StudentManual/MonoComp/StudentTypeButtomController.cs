using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib.mainUI.StudentManual.MonoComp
{
    public class StudentTypeButtomController : MonoBehaviour
    {
        public Button AllSelect;
        public Button HasSelect;
        public Button NoHasSelect;

        private Color color;

        void Start()
        {
            AllSelect = transform.Find("AllSelect").GetComponent<Button>();
            HasSelect = transform.Find("HasSelect").GetComponent<Button>();
            NoHasSelect = transform.Find("NoHasSelect").GetComponent<Button>();

            AllSelect.onClick.AddListener(() =>
            {
                defbuttomSyle();
                selctAllSelect();
            });
            HasSelect.onClick.AddListener(() =>
            {
                defbuttomSyle();
                selctHasSelect();
            });
            NoHasSelect.onClick.AddListener(() =>
            {
                defbuttomSyle();
                selctNoHasSelect();
            });

            selctAllSelect();
        }

        void defbuttomSyle()
        {
            AllSelect.GetComponent<Image>().color = Color.white;
            HasSelect.GetComponent<Image>().color = Color.white;
            NoHasSelect.GetComponent<Image>().color = Color.white;
            AllSelect.transform.Find("Text").GetComponent<Text>().color = Color.black;
            color = new Color32(255, 0, 0, 255);
            NoHasSelect.transform.Find("Text").GetComponent<Text>().color = color;
            color = new Color32(0, 104, 255, 255);
            HasSelect.transform.Find("Text").GetComponent<Text>().color = color;
        }

        public void setUiFix()
        {
            int activeChildCount = ManualMapData.ManualScrollView.transform.Cast<Transform>().Count(t => t.gameObject.activeSelf);
            int hsiz = activeChildCount / 6 + 2;
            RectTransform rt = ManualMapData.ManualScrollView.GetComponent<RectTransform>();
            Vector2 size = rt.sizeDelta;
            size.y = 400f * hsiz;
            rt.sizeDelta = size;
            StudentManualEvents.resetUIlistRefresh();
        }

        void selctAllSelect()
        {
            ManualMapData.selectStudentIndex = 0;
            StudentManualEvents.RaiseRefresh();
            color = new Color32(21, 55, 86, 255);
            AllSelect.GetComponent<Image>().color = color;
            AllSelect.transform.Find("Text").GetComponent<Text>().color = Color.white;
            setUiFix();
        }

        void selctHasSelect()
        {
            ManualMapData.selectStudentIndex = 1;
            StudentManualEvents.RaiseRefresh();
            color = new Color32(0, 119, 255, 255);
            HasSelect.GetComponent<Image>().color = color;
            HasSelect.transform.Find("Text").GetComponent<Text>().color = Color.white;
            setUiFix();
        }

        void selctNoHasSelect()
        {
            ManualMapData.selectStudentIndex = 2;
            StudentManualEvents.RaiseRefresh();
            color = new Color32(255, 0, 0, 255);
            NoHasSelect.GetComponent<Image>().color = color;
            NoHasSelect.transform.Find("Text").GetComponent<Text>().color = Color.white;
            setUiFix();
        }
    }
}
