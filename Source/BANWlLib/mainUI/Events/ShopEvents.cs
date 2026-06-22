using System;

public static class ShopEvents
{
    // 事件：要求所有按钮刷新状态
    public static event Action OnRefreshAllButtons;

    // 调用此方法触发刷新事件
    public static void RaiseRefresh()
    {
        OnRefreshAllButtons?.Invoke();
    }
}
