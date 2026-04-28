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
                "ventgas_kanim",
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

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Operational>();
            go.AddOrGet<GasPressureEqualizerVent>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
        }
    }
}
