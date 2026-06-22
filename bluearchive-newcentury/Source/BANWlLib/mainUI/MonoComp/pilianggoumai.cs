using BANWlLib.BaDef;
using BANWlLib.mainUI.pojo;
using BANWlLib.uicreater.tool;
using MyCoolMusicMod;
using MyCoolMusicMod.MyCoolMusicMod;
using newpro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.MonoComp
{
    public class pilianggoumai:MonoBehaviour
    {
        public shotData shotData;

        //商品名
        public string goodName;
        //商品价格
        public int goodPrice;
        //货币
        public string currency;
        // 商品数量
        public int goodAmount;

        //显示购买数量
        public UnityEngine.UI.Text amountText;
        //显示总计
        public UnityEngine.UI.Text totalText;
        //滑条
        public Slider Slider;
        void Start()
        {
            goodName = shotData.shot.ProductDefName;
            goodPrice = shotData.shot.CurrencyAmount;
            currency = shotData.shot.CurrencyDefName;
            goodAmount = shotData.shot.ProductAmount;
            this.transform.Find("pilianggoumai/shangpingname").GetComponent<UnityEngine.UI.Text>().text = shotData.gameObject.transform.Find("title").GetComponent<UnityEngine.UI.Text>().text;
            this.transform.Find("pilianggoumai/goodback/good").GetComponent<Image>().sprite = shotData.gameObject.transform.Find("bodytitle").GetComponent<Image>().sprite;
            this.transform.Find("pilianggoumai/buttommin2/huobiimage").GetComponent<Image>().sprite = shotData.gameObject.transform.Find("goumai/jiageback/shotimag").GetComponent<Image>().sprite;
            this.transform.Find("pilianggoumai/buttommin1/huobiimage").GetComponent<Image>().sprite = shotData.gameObject.transform.Find("goumai/jiageback/shotimag").GetComponent<Image>().sprite;
            this.transform.Find("pilianggoumai/buttommin2/cont").GetComponent<UnityEngine.UI.Text>().text = goodPrice.ToString();
            this.transform.Find("pilianggoumai/goodback").GetComponent<Image>().sprite = shotData.gameObject.transform.Find("pingzhi/" + shotData.shot.ProductQuality).GetComponent<Image>().sprite;
            amountText = this.transform.Find("pilianggoumai/goodback/cont").GetComponent<UnityEngine.UI.Text>();
            totalText = this.transform.Find("pilianggoumai/buttommin1/cont").GetComponent<UnityEngine.UI.Text>();
            Slider = this.transform.Find("pilianggoumai/Slider").GetComponent<Slider>();
            Slider.wholeNumbers = true;
            Slider.value = 1;
            Slider.minValue = 1;
            setCloseButtom();
            setBuyButtom();
        }
        void Update()
        {
            amountText.text = "X"+Slider.value.ToString();
            totalText.text = (goodPrice * Slider.value).ToString();
            Slider.maxValue = (ItemUtility.GetTotalItemCount(currency) / goodPrice);
            if (!UiMapData.isOpenShop) 
            {
                GameObject.Destroy(this.gameObject);
            }
        }
        void setCloseButtom()
        {
            this.transform.Find("pilianggoumai/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject.Destroy(this.gameObject);
            });
            this.transform.Find("pilianggoumai/quxiao").GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject.Destroy(this.gameObject);
            });
        }

        void setBuyButtom()
        {
            this.transform.Find("pilianggoumai/goumai").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (ItemUtility.GetTotalItemCount(currency) < goodPrice * Slider.value)
                {
                    BamessageUI.ShowBaMessageUI("购买失败", "当前所需货币数量已不够支持本次购买，请减少购买数量", "返回");
                    return;
                }
                int a = (int)(goodPrice * Slider.value);
                ItemUtility.TryRemoveItem(currency, a);
                ThingDef thing = DefDatabase<ThingDef>.GetNamed(goodName);
                PawnDropHelper.DropProp(Find.CurrentMap, thing, (int)(Slider.value* goodAmount));
                LoopBGMManager.playEffAudio("shotgoumai");
                ShopEvents.RaiseRefresh();
                GameObject.Destroy(this.gameObject);
            });
        }
    }
}
