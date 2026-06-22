using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using LudeonTK;
using Newtonsoft.Json.Linq;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.Dev
{
    public static class StudentListShow
    {
        [DebugAction("BA", "查看当前学生列表", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogMyTrackerValues()
        {
            // 获取你的 GameComponent
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();

            if (tracker == null)
            {
                Log.Warning("ManualDataGameComp 未找到!"); // 注释：普通log输出，屏蔽
                return;
            }
            foreach (var student in tracker.HaveStudent)
            {
                //使用 Log.Message 打印你想看的值
                Log.Warning("----学生---- "); 
                Log.Message(student.ToString()); 
            }
        }

        [DebugAction("BA", "查看当前收藏的学生", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogCollectTrackerValues()
        {
            // 获取你的 GameComponent
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();

            if (tracker == null)
            {
                Log.Warning("ManualDataGameComp 未找到!");
                return;
            }
             Log.Warning("已收藏的学生"); 
            foreach (var student in tracker.StudentCollect)
            {;
                Log.Message(student.ToString()); 
            }
        }

        [DebugAction("BA", "选择一个学生查看序列化的数据", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SelectStudentData()
        {
            // 获取你的 GameComponent
            ManualDataGameComp tracker = Current.Game.GetComponent<ManualDataGameComp>();

            if (tracker == null)
            {
                Log.Warning("ManualDataGameComp 未找到!");
                return;
            }
            Action<LocalTargetInfo> afterTargetSelected = (LocalTargetInfo target) =>
            {
                Pawn selectedPawn = target.Thing as Pawn;
                StudentSave StudentSave = tracker.studentSaves.FirstOrDefault(p => p.DefName == selectedPawn.def.defName);
                if (selectedPawn != null)
                {
                    string message = "";
                    if (StudentSave == null) {
                        message = "该pawn从未序列化过，无法查看";
                    }
                    else 
                    {
                        message = $"学生姓名：{selectedPawn.Name}\n\n学生种族DefName:{StudentSave.DefName}\n\n学生等级:{StudentSave.StudentLvInt}\n\n学生等级严重度:{StudentSave.StudentLvInt}\n\n学生经验值:{StudentSave.StudentLvInt}\n\n学生基础能力列表：";
                        foreach (KeyValuePair<string,int> pair in StudentSave.SkillXPs)
                        {
                            message += "\n\n技能名:"+pair.Key+" 当前等级："+pair.Value;
                        }
                    }

                    // 2. 直接创建 Dialog_MessageBox 实例
                    // 我们使用它的构造函数来精确控制
                    Dialog_MessageBox infoDialog = new Dialog_MessageBox(
                        message,        // [必要] 对话框显示的文本
                        "返回游戏",         // [必要] 按钮A的文本
                        null,           // 按钮A的回调，设为 null，点击后默认只关闭窗口
                        null,           // 按钮B的文本，设为 null 则不显示此按钮
                        null,           // 按钮B的回调
                        "学生序列化数据展示",         // (可选) 窗口的标题
                        false,          // (可选) 是否立即打开信件
                        null,           // (可选) 窗口关闭时执行的回调
                        null            // (可选) 窗口背景
                    );

                    // 3. 将创建好的窗口添加到窗口堆栈以显示出来
                    Find.WindowStack.Add(infoDialog);
                }
            };
            TargetingParameters targetingParams = new TargetingParameters
            {
                validator = (TargetInfo target) =>
                {
                    return target.HasThing && target.Thing is Pawn;
                }
            };
            Find.Targeter.BeginTargeting(targetingParams, afterTargetSelected, null);
    }
    }
}
