#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace BANWlLib.mainUI.MonoComp
{
    public class messageUILord : MonoBehaviour
    {
        public string title;
        public string des;
        public string Buttomtext;

        void Start()
        {
            this.gameObject.transform.Find("UIback/Title").GetComponent<Text>().text = title;
            this.gameObject.transform.Find("UIback/des").GetComponent<Text>().text = des;
            this.gameObject.transform.Find("UIback/Button/Text").GetComponent<Text>().text = Buttomtext;
            this.gameObject.transform.Find("UIback/Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject.Destroy(this.gameObject);
            });
        }
    }

    public class messageUILordQuek : MonoBehaviour
    {
        public string title;
        public string des;
        public string CloseButtomtext;
        public string QuekButtomtext;
        public Action onQuek;

        void Start()
        {
            this.gameObject.transform.Find("UIback/Title").GetComponent<Text>().text = title;
            this.gameObject.transform.Find("UIback/des").GetComponent<Text>().text = des;
            this.gameObject.transform.Find("UIback/Close/Text").GetComponent<Text>().text = CloseButtomtext;
            this.gameObject.transform.Find("UIback/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject.Destroy(this.gameObject);
            });
            this.gameObject.transform.Find("UIback/Quek/Text").GetComponent<Text>().text = QuekButtomtext;
            this.gameObject.transform.Find("UIback/Quek").GetComponent<Button>().onClick.AddListener(() =>
            {
                onQuek?.Invoke(); 
                GameObject.Destroy(this.gameObject);
            });
        }
    }
}
