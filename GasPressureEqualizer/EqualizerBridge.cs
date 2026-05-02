using System.Text;
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

            // One-shot snapshot of bridge spawn state, including what's at
            // every relevant cell. Cheap to keep and invaluable when a user
            // reports a "doesn't work" bug — the bridge cells (especially
            // when overlapping a stock pipe) are non-trivial to inspect
            // from in-game.
            var sb = new StringBuilder();
            sb.AppendLine($"[GPE-Bridge] OnSpawn origin={origin} input={inputCell} output={outputCell} inputOutward={inputOutward} outputOutward={outputOutward} registered={registered}");
            sb.AppendLine($"[GPE-Bridge]   origin: {EqualizerNetwork.DescribeCell(origin)}");
            sb.AppendLine($"[GPE-Bridge]   input:  {EqualizerNetwork.DescribeCell(inputCell)}");
            sb.AppendLine($"[GPE-Bridge]   output: {EqualizerNetwork.DescribeCell(outputCell)}");
            sb.AppendLine($"[GPE-Bridge]   inOut:  {EqualizerNetwork.DescribeCell(inputOutward)}");
            sb.Append    ($"[GPE-Bridge]   outOut: {EqualizerNetwork.DescribeCell(outputOutward)}");
            Debug.Log(sb.ToString());
        }

        protected override void OnCleanUp()
        {
            if (registered)
            {
                EqualizerNetwork.UnregisterBridgeEndpoint(inputCell);
                EqualizerNetwork.UnregisterBridgeEndpoint(outputCell);
                registered = false;
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
