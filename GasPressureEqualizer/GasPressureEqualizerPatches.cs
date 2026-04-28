using HarmonyLib;
using STRINGS;

namespace GasPressureEqualizer
{
    // Suppress visual auto-connect between Equalizer Ducts and stock gas pipes.
    // GetNeighbour finds the visualizer at an adjacent cell; if exactly one side
    // is an EqualizerPipe, refuse the connection so they render as separate runs.
    [HarmonyPatch(typeof(KAnimGraphTileVisualizer), "GetNeighbour")]
    public static class KAnimGraphTileVisualizer_GetNeighbour_Patch
    {
        public static void Postfix(KAnimGraphTileVisualizer __instance, ref KAnimGraphTileVisualizer __result)
        {
            if (__result == null) return;

            bool myIsEqualizer = __instance.GetComponent<EqualizerPipe>() != null;
            bool neighborIsEqualizer = __result.GetComponent<EqualizerPipe>() != null;

            if (myIsEqualizer != neighborIsEqualizer)
            {
                __result = null;
            }
        }
    }

    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    public class GasPressureEqualizerPatch
    {
        public static void Prefix()
        {
            string ventId = GasPressureEqualizerVentConfig.ID.ToUpperInvariant();
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{ventId}.NAME",
                UI.FormatAsLink("Equalizer Vent", ventId)
            );
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{ventId}.DESC",
                "A powerless vent that exchanges gas with its room. Connect two vents with Equalizer Ducts to passively equalize pressure — gas flows from high pressure to low."
            );
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{ventId}.EFFECT",
                "Equalizes gas pressure between rooms when connected to other Equalizer Vents via Equalizer Ducts. Requires no power."
            );

            string pipeId = EqualizerPipeConfig.ID.ToUpperInvariant();
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{pipeId}.NAME",
                UI.FormatAsLink("Equalizer Duct", pipeId)
            );
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{pipeId}.DESC",
                "Custom conduit for the Equalizer system. Connects Equalizer Vents into a network. Does not interact with stock gas pipes."
            );
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{pipeId}.EFFECT",
                "Carries pressure information between Equalizer Vents. Place adjacent to or beneath an Equalizer Vent to bridge it into the network."
            );

            ModUtil.AddBuildingToPlanScreen("Utilities", GasPressureEqualizerVentConfig.ID);
            ModUtil.AddBuildingToPlanScreen("Utilities", EqualizerPipeConfig.ID);
        }
    }
}
