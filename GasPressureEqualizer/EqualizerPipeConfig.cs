using System.Collections.Generic;
using UnityEngine;
using TUNING;

namespace GasPressureEqualizer
{
    public class EqualizerPipeConfig : IBuildingConfig
    {
        public const string ID = "EqualizerDuct";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID,
                1,
                1,
                "equalizer_duct_kanim",
                10,
                3f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER0,
                MATERIALS.RAW_MINERALS_OR_METALS,
                1600f,
                BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.NONE,
                NOISE_POLLUTION.NONE
            );

            def.Floodable = false;
            def.Overheatable = false;
            def.Entombable = false;
            def.ViewMode = OverlayModes.GasConduits.ID;
            def.ObjectLayer = ObjectLayer.GasConduit;
            def.TileLayer = ObjectLayer.GasConduitTile;
            def.ReplacementLayer = ObjectLayer.ReplacementGasConduit;
            def.AudioCategory = "Metal";
            def.AudioSize = "small";
            def.BaseTimeUntilRepair = 0f;
            def.UtilityInputOffset = new CellOffset(0, 0);
            def.UtilityOutputOffset = new CellOffset(0, 0);
            def.SceneLayer = Grid.SceneLayer.GasConduits;
            def.isKAnimTile = true;
            def.isUtility = true;
            def.DragBuild = true;
            def.PermittedRotations = PermittedRotations.Unrotatable;
            def.ReplacementTags = new List<Tag>();

            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            go.AddOrGet<Conduit>().type = ConduitType.Gas;
            go.AddOrGet<EqualizerPipe>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<Building>().Def.BuildingUnderConstruction.GetComponent<Constructable>().isDiggingRequired = false;
            go.AddComponent<EmptyConduitWorkable>();
            var visualizer = go.AddComponent<KAnimGraphTileVisualizer>();
            visualizer.connectionSource = KAnimGraphTileVisualizer.ConnectionSource.Gas;
            visualizer.isPhysicalBuilding = true;
            go.GetComponent<KPrefabID>().AddTag(GameTags.Vents);
            GeneratedBuildings.RemoveLoopingSounds(go);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            // Mirror stock GasConduitConfig - the under-construction prefab also
            // needs a tile visualizer, otherwise dupe-built ducts can fail to
            // refresh tile-connection visuals after completion.
            var visualizer = go.AddComponent<KAnimGraphTileVisualizer>();
            visualizer.connectionSource = KAnimGraphTileVisualizer.ConnectionSource.Gas;
            visualizer.isPhysicalBuilding = false;
        }

    }
}
