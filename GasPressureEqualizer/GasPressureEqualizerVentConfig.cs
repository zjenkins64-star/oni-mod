using System.Collections.Generic;
using UnityEngine;
using TUNING;

namespace GasPressureEqualizer
{
    public class GasPressureEqualizerVentConfig : IBuildingConfig
    {
        public const string ID = "GasPressureEqualizerVent";

        public override BuildingDef CreateBuildingDef()
        {
            var def = BuildingTemplates.CreateBuildingDef(
                ID,
                1,
                1,
                "equalizer_vent_kanim",
                30,
                30f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER1,
                MATERIALS.ALL_METALS,
                800f,
                BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.NONE,
                NOISE_POLLUTION.NONE
            );

            def.Floodable = false;
            def.Overheatable = false;
            def.Entombable = false;
            def.PermittedRotations = PermittedRotations.Unrotatable;

            // We deliberately do NOT set def.InputConduitType / OutputConduitType.
            // Declaring those caused the engine to register the vent as a
            // gas-network endpoint at the same cell as the underlying duct's
            // Conduit, stomping the duct's existing endpoint and emitting a
            // "Adding FlowUtilityNetwork+NetworkItem will stomp previous endpoint"
            // warning. They also auto-added RequireInputs / ConduitConsumer /
            // ConduitDispenser components which we had to DestroyImmediate.
            // RegisterWithOverlay below is sufficient for the gas overlay to
            // highlight us; gas movement is done via SimMessages teleport.

            // Single logic input port — green enables this specific vent's
            // gas-equalization behavior, red disables it. Other vents on the
            // same network are controlled independently.
            def.LogicInputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.InputPort(
                    LogicOperationalController.PORT_ID,
                    new CellOffset(0, 0),
                    "Logic Control",
                    "Enable this vent's gas equalization.",
                    "Disable this vent's gas equalization.")
            };

            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Operational>();
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGet<GasPressureEqualizerVent>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().AddTag(GameTags.Vents);
        }
    }
}
