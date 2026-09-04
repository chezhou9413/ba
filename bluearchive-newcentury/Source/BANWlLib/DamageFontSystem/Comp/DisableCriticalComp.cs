using System.Collections.Generic;
using Verse;

namespace BANWlLib.DamageFontSystem.Comp
{
    //游戏级UI与暴击配置组件负责保存入口位置、COST轮盘位置和暴击规则列表。
    public class DisableCriticalComp : GameComponent
    {
        public List<DamageDef> DisableCritical = new List<DamageDef>();
        public List<DamageDef> EnsureCritical = new List<DamageDef>();
        public List<DamageDef> DisableIncomingDamageFactorCritical = new List<DamageDef>();
        public float savePosX = 780.6f;
        public float savePosY = -477.1f;
        public float costUiPosX = 0f;
        public float costUiPosY = 115f;

        //保存入口按钮与COST轮盘位置，使读档后恢复玩家布局。
        public override void ExposeData()
        {
            Scribe_Values.Look(ref savePosX, "dfPosX", 780.6f);
            Scribe_Values.Look(ref savePosY, "dfPosY", -477.1f);
            Scribe_Values.Look(ref costUiPosX, "costUiPosX", 0f);
            Scribe_Values.Look(ref costUiPosY, "costUiPosY", 115f);
        }

        //构造RimWorld要求的游戏组件实例。
        public DisableCriticalComp(Game game)
        {
        }

        //新游戏开始后载入暴击规则列表。
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            InitCriticalLists();
        }

        //读取存档后重新载入暴击规则列表。
        public override void LoadedGame()
        {
            base.LoadedGame();
            InitCriticalLists();
        }

        //从FontDef刷新禁用与强制暴击规则。
        private void InitCriticalLists()
        {
            var def = DefDatabase<FontDef>.GetNamedSilentFail("BANW_FontDef");
            if (def != null)
            {
                EnsureCritical = def.EnsureCritical;
                DisableCritical = def.DisableCritical;
                DisableIncomingDamageFactorCritical = def.DisableIncomingDamageFactorCritical;
            }
        }
    }
}
