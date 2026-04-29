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

    // The Refresh() method computes connections via connectionManager.GetConnections(),
    // which bypasses GetNeighbour and gives us cross-system bits (e.g. after a stock
    // pipe is drag-placed across our duct). After Refresh runs, mask out any direction
    // bit whose cardinal neighbor is a different system (one Equalizer + one stock).
    [HarmonyPatch(typeof(KAnimGraphTileVisualizer), "Refresh")]
    public static class KAnimGraphTileVisualizer_Refresh_Patch
    {
        public static void Postfix(KAnimGraphTileVisualizer __instance)
        {
            if (__instance == null) return;

            UtilityConnections conns = __instance.Connections;
            if (conns == 0) return;

            bool selfIsEqualizer = __instance.GetComponent<EqualizerPipe>() != null;
            UtilityConnections newConns = conns;

            if ((conns & UtilityConnections.Up) != 0 && IsCrossSystem(__instance, Direction.Up, selfIsEqualizer))
                newConns &= ~UtilityConnections.Up;
            if ((conns & UtilityConnections.Down) != 0 && IsCrossSystem(__instance, Direction.Down, selfIsEqualizer))
                newConns &= ~UtilityConnections.Down;
            if ((conns & UtilityConnections.Left) != 0 && IsCrossSystem(__instance, Direction.Left, selfIsEqualizer))
                newConns &= ~UtilityConnections.Left;
            if ((conns & UtilityConnections.Right) != 0 && IsCrossSystem(__instance, Direction.Right, selfIsEqualizer))
                newConns &= ~UtilityConnections.Right;

            if (newConns != conns)
            {
                __instance.UpdateConnections(newConns);
                // Manually replay the kanim symbol since the original Refresh
                // already played the unfiltered one. GetVisualizerString reads
                // visualGrid which we just rewrote via UpdateConnections.
                var manager = __instance.connectionManager;
                if (manager != null)
                {
                    int cell = Grid.PosToCell(__instance.transform.GetPosition());
                    string text = manager.GetVisualizerString(cell);
                    var anim = __instance.GetComponent<KBatchedAnimController>();
                    if (anim != null && !string.IsNullOrEmpty(text))
                    {
                        anim.Play(text);
                    }
                }
            }
        }

        private static bool IsCrossSystem(KAnimGraphTileVisualizer self, Direction d, bool selfIsEqualizer)
        {
            // GetNeighbour is patched above to return null for cross-system pairs,
            // so call the original logic directly via cell lookup.
            int cell = Grid.PosToCell(self.transform.GetPosition());
            int neighborCell = -1;
            switch (d)
            {
                case Direction.Up: neighborCell = Grid.CellAbove(cell); break;
                case Direction.Down: neighborCell = Grid.CellBelow(cell); break;
                case Direction.Left: neighborCell = Grid.CellLeft(cell); break;
                case Direction.Right: neighborCell = Grid.CellRight(cell); break;
            }
            if (neighborCell < 0) return false;

            int layer = self.connectionSource switch
            {
                KAnimGraphTileVisualizer.ConnectionSource.Gas => 13,
                KAnimGraphTileVisualizer.ConnectionSource.Liquid => 17,
                KAnimGraphTileVisualizer.ConnectionSource.Solid => 21,
                KAnimGraphTileVisualizer.ConnectionSource.Electrical => 27,
                KAnimGraphTileVisualizer.ConnectionSource.Logic => 32,
                _ => -1,
            };
            if (layer < 0) return false;

            var go = Grid.Objects[neighborCell, layer];
            if (go == null) return false;

            bool neighborIsEqualizer = go.GetComponent<EqualizerPipe>() != null;
            return selfIsEqualizer != neighborIsEqualizer;
        }
    }

    // Suppress the "Contents: Empty" pipe status item on any of our equalizer
    // buildings. Conduit.OnStructureTemperatureRegistered adds
    // Db.Get().BuildingStatusItems.Pipe; we yank it back off for our ducts,
    // vents, and bridges (the engine auto-adds a Conduit to anything with
    // InputConduitType / OutputConduitType set).
    [HarmonyPatch(typeof(Conduit), "OnStructureTemperatureRegistered")]
    public static class Conduit_OnStructureTemperatureRegistered_StatusItemPatch
    {
        public static void Postfix(Conduit __instance)
        {
            if (__instance == null) return;
            bool isOurs = __instance.GetComponent<EqualizerPipe>() != null
                       || __instance.GetComponent<GasPressureEqualizerVent>() != null
                       || __instance.GetComponent<EqualizerBridge>() != null;
            if (!isOurs) return;
            var selectable = __instance.GetComponent<KSelectable>();
            if (selectable != null)
            {
                selectable.RemoveStatusItem(Db.Get().BuildingStatusItems.Pipe);
            }
        }
    }

    // Whenever the gas conduit network updates a cell's connections (Reconnect
    // updates neighbor data but doesn't refresh their kanim), force-refresh any
    // EqualizerPipe in the 4 adjacent cells so stale cross-system stubs clear.
    [HarmonyPatch(typeof(UtilityNetworkManager<FlowUtilityNetwork, Vent>), "SetConnections")]
    public static class GasUtilityNetwork_SetConnections_Patch
    {
        [System.ThreadStatic] private static bool inRefresh;

        public static void Postfix(int cell)
        {
            if (inRefresh) return;
            inRefresh = true;
            try
            {
                RefreshEqualizerNeighbor(Grid.CellAbove(cell));
                RefreshEqualizerNeighbor(Grid.CellBelow(cell));
                RefreshEqualizerNeighbor(Grid.CellLeft(cell));
                RefreshEqualizerNeighbor(Grid.CellRight(cell));
            }
            finally
            {
                inRefresh = false;
            }
        }

        private static void RefreshEqualizerNeighbor(int cell)
        {
            if (cell < 0 || cell >= Grid.CellCount) return;
            var go = Grid.Objects[cell, 13]; // ObjectLayer.GasConduit
            if (go == null) return;
            if (go.GetComponent<EqualizerPipe>() == null) return;
            var viz = go.GetComponent<KAnimGraphTileVisualizer>();
            if (viz != null) viz.Refresh();
        }
    }

    // Add our buildings to the GasPiping tech (same node that unlocks stock
    // GasConduit / GasConduitBridge / GasPump / GasVent).
    [HarmonyPatch(typeof(Db), "Initialize")]
    public static class Db_Initialize_TechPatch
    {
        public static void Postfix()
        {
            var tech = Db.Get().Techs.TryGet("GasPiping");
            if (tech == null) return;
            tech.unlockedItemIDs.Add(GasPressureEqualizerVentConfig.ID);
            tech.unlockedItemIDs.Add(EqualizerPipeConfig.ID);
            tech.unlockedItemIDs.Add(EqualizerBridgeConfig.ID);
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

            string bridgeId = EqualizerBridgeConfig.ID.ToUpperInvariant();
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{bridgeId}.NAME",
                UI.FormatAsLink("Equalizer Bridge", bridgeId)
            );
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{bridgeId}.DESC",
                "A bridge for the Equalizer Duct network. Tunnels its two endpoints together so an Equalizer network can cross over a cell that already contains a stock pipe, wire, or perpendicular duct without merging networks."
            );
            Strings.Add(
                $"STRINGS.BUILDINGS.PREFABS.{bridgeId}.EFFECT",
                "Place across a 3-cell span to bridge two Equalizer Ducts together. The middle cell carries no network connection — perpendicular networks can pass through it freely."
            );

            ModUtil.AddBuildingToPlanScreen("HVAC", GasPressureEqualizerVentConfig.ID);
            ModUtil.AddBuildingToPlanScreen("HVAC", EqualizerPipeConfig.ID);
            ModUtil.AddBuildingToPlanScreen("HVAC", EqualizerBridgeConfig.ID);
        }
    }
}
