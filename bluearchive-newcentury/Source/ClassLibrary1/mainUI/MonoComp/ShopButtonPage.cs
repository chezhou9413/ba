using newpro;
using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib
{
    /// <summary>
    /// 商店按钮页面切换控制器
    /// 点击按钮时会设置当前为选中状态，并根据选中/非选中状态更换按钮图像与文字颜色
    /// </summary>
    public class ShopButtonPage : MonoBehaviour
    {
        // ===== 组件引用 =====
        private Button buttonComponent;             // 按钮组件
        private Image backgroundImageComponent;     // 按钮背景图（Image）
        private Text labelTextComponent;            // 按钮上的文字

        // ===== 颜色常量（使用 Color32 可直接用 0~255 颜色值）=====
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

        /// <summary>
        /// 缓存本物体上的常用组件
        /// </summary>
        private void CacheComponents()
        {
            buttonComponent = GetComponent<Button>();
            backgroundImageComponent = GetComponent<Image>();
            labelTextComponent = transform.Find("Text").GetComponent<Text>();
        }

        /// <summary>
        /// 注册按钮点击事件
        /// </summary>
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

        /// <summary>
        /// 根据当前状态更新按钮外观
        /// </summary>
        /// <param name="selected">是否选中</param>
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
