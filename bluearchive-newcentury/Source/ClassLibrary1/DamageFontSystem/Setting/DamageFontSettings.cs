using Verse;

namespace BANWlLib.DamageFontSystem.Setting
{
    public class DamageFontSettings : ModSettings
    {
        public bool enableDamageFloat = true;
        public bool enableBurstParticle = true;
        public float dfPosX = 780.6f;
        public float dfPosY = -477.1f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableDamageFloat, "enableDamageFloat", true);
            Scribe_Values.Look(ref enableBurstParticle, "enableBurstParticle", true);
            Scribe_Values.Look(ref dfPosX, "dfPosX", 780.6f);
            Scribe_Values.Look(ref dfPosY, "dfPosY", -477.1f);
        }
    }
}
