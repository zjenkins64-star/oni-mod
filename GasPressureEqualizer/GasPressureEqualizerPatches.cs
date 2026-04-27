using HarmonyLib;

namespace GasPressureEqualizer
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    public class GasPressureEqualizerPatch
    {
        public static void Prefix()
        {
            ModUtil.AddBuildingToPlanScreen(
                "Utilities",
                GasPressureEqualizerVentConfig.ID
            );
        }
    }
}