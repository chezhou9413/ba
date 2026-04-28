using System;

public static class StudentManualEvents
{

    public static event Action OnRefreshAllStudentData;
    public static event Action resetUIlist;

    public static void RaiseRefresh()
    {
        OnRefreshAllStudentData?.Invoke();
    }

    public static void resetUIlistRefresh()
    {
        resetUIlist?.Invoke();
    }
}

