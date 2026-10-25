using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
using newpro;
using RimUnity;
using RimUnity.UI;

namespace BANWlLib
{
    //切换商店分页，并更新按钮图片和文字颜色。
    public class ShopButtonPage : MonoBehaviour
    {
        //组件引用。
        private Button buttonComponent;             // 按钮组件
        private Image backgroundImageComponent;     // 按钮背景图（Image）
        private Text labelTextComponent;            // 按钮上的文字

        //Color32 使用 0～255 的颜色分量。
        private static readonly Color32 SelectedTextColor = new Color32(255, 204, 0, 255); // 金黄色
        private static readonly Color32 UnselectedTextColor = new Color32(50, 50, 50, 255);  // 深灰色

        // 当前是否选中（用于减少重复刷新）
        private bool isSelectedState = false;

        private void Start()
        {
            CacheComponents();
            RegisterButtonClickEvent();

            // 初始化时更新一次显示
            UpdateVisual(UiMapData.selectShotPage == gameObject);
        }

        private void Update()
        {
            // 监测状态变化，只在选中状态变更时更新显示
            bool shouldBeSelected = (UiMapData.selectShotPage == gameObject);
            if (shouldBeSelected != isSelectedState)
            {
                UpdateVisual(shouldBeSelected);
            }
        }

        //缓存本物体上的常用组件
        private void CacheComponents()
        {
            buttonComponent = GetComponent<Button>();
            backgroundImageComponent = GetComponent<Image>();
            labelTextComponent = transform.Find("Text").GetComponent<Text>();
        }

        //注册按钮点击事件
        private void RegisterButtonClickEvent()
        {
            buttonComponent.onClick.AddListener(() =>
            {
                UiMapData.selectShotPage = gameObject;
                UpdateVisual(true);
                if(this.name == "yiban")
                {
                    shotlord.showpageUI("ordinary");
                }
                else if(this.name == "shenmingwenzi1")
                {
                    shotlord.showpageUI("Fragment1");
                }
                else if(this.name == "shenmingwenzi2")
                {
                    shotlord.showpageUI("Fragment2");
                }
            });
        }

        //按选中状态更新按钮外观。
        private void UpdateVisual(bool selected)
        {
            isSelectedState = selected;

            if (selected)
            {
                labelTextComponent.color = SelectedTextColor;
                backgroundImageComponent.sprite = UiMapData.shopButtonImageSelect;
            }
            else
            {
                labelTextComponent.color = UnselectedTextColor;
                backgroundImageComponent.sprite = UiMapData.shopButtonImageNoSelect;
            }
        }
    }
}
