using Verse;
using RimWorld;
using UnityEngine;

namespace BANWlLib
{
    // 人类自定义整数属性组件
    // 给角色添加一个可以保存的整数属性，比如经验值、积分什么的
    public class HumanIntPropertyComp : ThingComp
    {
        // 存储自定义整数属性的变量
        private int customIntValue = 0;
        
        // 获取或设置自定义整数属性
        public int CustomIntValue
        {
            get { return customIntValue; }
            set { customIntValue = value; }
        }
        
        // 获取组件的配置属性
        public HumanIntPropertyCompProperties Props => (HumanIntPropertyCompProperties)this.props;
        
        // 组件初始化方法
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            
            // 从XML配置中设置初始值
            if (Props != null)
            {
                customIntValue = Props.defaultValue;
            }
        }
        
        // 保存和加载数据的方法
        // 确保属性值能够正确保存到存档文件中
        public override void PostExposeData()
        {
            base.PostExposeData();
            
            // 保存/加载自定义整数属性
            Scribe_Values.Look(ref customIntValue, "customIntValue", 0);
        }
        
        // 在角色信息面板中显示额外信息
        public override string CompInspectStringExtra()
        {
            return $"学生经验: {customIntValue}";
        }
        
        // 增加属性值
        public void IncreaseValue(int amount)
        {
            customIntValue += amount;
        }
        
        // 减少属性值
        public void DecreaseValue(int amount)
        {
            customIntValue = Mathf.Max(0, customIntValue - amount); // 不会小于0
        }
        
        // 设置属性值
        public void SetValue(int value)
        {
            customIntValue = value;
        }
    }
    
    // 组件配置属性类
    // 定义可以在XML文件中配置的属性
    public class HumanIntPropertyCompProperties : CompProperties
    {
        // 默认值 - 组件初始化时使用的数值
        public int defaultValue = 0;
        
        // 最小值 - 属性不能低于这个值
        public int minValue = 0;
        
        // 最大值 - 属性不能高于这个值
        public int maxValue = 100;
        
        public HumanIntPropertyCompProperties()
        {
            this.compClass = typeof(HumanIntPropertyComp);
        }
    }
} 