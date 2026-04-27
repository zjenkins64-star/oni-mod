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
                "utilityconduitgas_kanim",   // MUST be valid
                10,
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
        
            def.InputConduitType = ConduitType.Gas;
            def.OutputConduitType = ConduitType.Gas;

            def.UtilityInputOffset = new CellOffset(0, 0);
            def.UtilityOutputOffset = new CellOffset(0, 0);

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Operational>();

            var storage = go.AddOrGet<Storage>();
            storage.capacityKg = 10f;
            storage.showInUI = true;
            storage.allowItemRemoval = true;

            var equalizer = go.AddOrGet<GasPressureEqualizerVent>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            // NOTHING animation-related goes here
        }
    }
}