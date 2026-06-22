using System.Collections.Generic;
using Verse;

namespace BANWlLib.DamageFontSystem.Comp
{
    public class DisableCriticalComp : GameComponent
    {
        public List<DamageDef> DisableCritical = new List<DamageDef>();
        public List<DamageDef> EnsureCritical = new List<DamageDef>();
        public List<DamageDef> DisableIncomingDamageFactorCritical = new List<DamageDef>();
        public float savePosX = 780.6f;
        public float savePosY = -477.1f;

        // RimWorld要求GameComponent子类必须有public DisableCriticalComp(Game game)构造函数，注意不能写base(game)
        public override void ExposeData()
        {
            Scribe_Values.Look(ref savePosX, "dfPosX", 780.6f);
            Scribe_Values.Look(ref savePosY, "dfPosY", -477.1f);
        }
        public DisableCriticalComp(Game game)
        {
            // 可在此处实现组件所需初始化
        }
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            InitCriticalLists();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            InitCriticalLists();
        }

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
