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
                MATERIALS.RAW_METALS,
                800f,
                BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.NONE,
                NOISE_POLLUTION.NONE
            );

            def.Floodable = false;
            def.Overheatable = false;
            def.Entombable = false;
            def.PermittedRotations = PermittedRotations.Unrotatable;

            // Declare as a gas conduit endpoint so the overlay treats us as a
            // proper vent. We don't add ConduitConsumer/Dispenser components,
            // so the engine won't actually move gas through the vent.
            def.InputConduitType = ConduitType.Gas;
            def.OutputConduitType = ConduitType.Gas;
            def.UtilityInputOffset = new CellOffset(0, 0);
            def.UtilityOutputOffset = new CellOffset(0, 0);

            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Operational>();
            go.AddOrGet<GasPressureEqualizerVent>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            // BuildingLoader auto-adds RequireInputs / ConduitConsumer / etc when
            // InputConduitType=Gas is declared on the def. We declared it only
            // for overlay recognition, not for actual gas flow — the equalizer
            // moves gas via SimMessages teleport. Strip the auto-added stock
            // plumbing so we don't get "Needs Gas In" / "Empty pipe" tooltips.
            UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireInputs>());
            UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireOutputs>());
            UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
            UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());

            go.GetComponent<KPrefabID>().AddTag(GameTags.Vents);
        }
    }
}
