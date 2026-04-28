using UnityEngine;
using TUNING;

namespace GasPressureEqualizer
{
    public class EqualizerBridgeConfig : IBuildingConfig
    {
        public const string ID = "EqualizerBridge";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID,
                3,
                1,
                "utilitygasbridge_kanim",
                10,
                3f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER1,
                MATERIALS.RAW_METALS,
                1600f,
                BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.NONE,
                NOISE_POLLUTION.NONE
            );

            def.ObjectLayer = ObjectLayer.GasConduitConnection;
            def.SceneLayer = Grid.SceneLayer.GasConduitBridges;
            def.Floodable = false;
            def.Entombable = false;
            def.Overheatable = false;
            def.ViewMode = OverlayModes.GasConduits.ID;
            def.AudioCategory = "Metal";
            def.AudioSize = "small";
            def.BaseTimeUntilRepair = -1f;
            def.PermittedRotations = PermittedRotations.R360;
            def.UtilityInputOffset = new CellOffset(-1, 0);
            def.UtilityOutputOffset = new CellOffset(1, 0);

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            go.AddOrGet<EqualizerBridge>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
        }
    }
}
