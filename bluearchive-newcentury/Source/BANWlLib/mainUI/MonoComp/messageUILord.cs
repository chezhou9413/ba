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
    //展示只有一个关闭按钮的提示框。
    public class messageUILord : MonoBehaviour
    {
        public string title;
        public string des;
        public string Buttomtext;

        //填入提示文字并绑定关闭事件。
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

    //展示确认与取消按钮，确认窗口不持有业务执行后的输入锁。
    public class messageUILordQuek : MonoBehaviour
    {
        public string title;
        public string des;
        public string CloseButtomtext;
        public string QuekButtomtext;
        public Action onQuek;

        //绑定确认事件，在执行业务前移除弹窗以防重复提交。
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
                gameObject.SetActive(false);
                GameObject.Destroy(this.gameObject);
                onQuek?.Invoke();
            });
        }
    }
}
