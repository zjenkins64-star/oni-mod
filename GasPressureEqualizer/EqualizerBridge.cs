using UnityEngine;

namespace GasPressureEqualizer
{
    public class EqualizerBridge : KMonoBehaviour
    {
        private static readonly Color BASE_TINT = Color.white;

        private int inputCell;
        private int outputCell;
        private bool registered;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            var building = GetComponent<Building>();
            inputCell = building.GetUtilityInputCell();
            outputCell = building.GetUtilityOutputCell();
            int origin = Grid.PosToCell(this);

            int inputOutward = ComputeOutward(inputCell, origin);
            int outputOutward = ComputeOutward(outputCell, origin);

            if (inputOutward != -1 && outputOutward != -1)
            {
                EqualizerNetwork.RegisterBridgeEndpoint(inputCell, inputOutward, outputCell);
                EqualizerNetwork.RegisterBridgeEndpoint(outputCell, outputOutward, inputCell);
                registered = true;
            }

            var anim = GetComponent<KBatchedAnimController>();
            if (anim != null)
            {
                anim.TintColour = BASE_TINT;
            }
        }

        protected override void OnCleanUp()
        {
            if (registered)
            {
                EqualizerNetwork.UnregisterBridgeEndpoint(inputCell);
                EqualizerNetwork.UnregisterBridgeEndpoint(outputCell);
            }
            base.OnCleanUp();
        }

        // Given an endpoint cell and the bridge's origin/middle cell,
        // returns the cell one step further in the same direction (the "outward" cell).
        private static int ComputeOutward(int endpoint, int origin)
        {
            if (endpoint == Grid.CellLeft(origin)) return Grid.CellLeft(endpoint);
            if (endpoint == Grid.CellRight(origin)) return Grid.CellRight(endpoint);
            if (endpoint == Grid.CellAbove(origin)) return Grid.CellAbove(endpoint);
            if (endpoint == Grid.CellBelow(origin)) return Grid.CellBelow(endpoint);
            return -1;
        }
    }
}
