using UnityEngine;

namespace GasPressureEqualizer
{
    public class EqualizerPipe : KMonoBehaviour
    {
        private static readonly Color RED_TINT = new Color(1f, 0.45f, 0.45f);

        private int cell;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            cell = Grid.PosToCell(this);
            EqualizerNetwork.RegisterPipe(cell);

            // Detach from the stock gas conduit network so our ducts don't
            // share gas with stock gas pipes connected adjacent to them.
            var conduit = GetComponent<Conduit>();
            if (conduit != null)
            {
                conduit.Disconnect();
            }

            var anim = GetComponent<KBatchedAnimController>();
            if (anim != null)
            {
                anim.TintColour = RED_TINT;
            }

            Debug.Log($"[GPE] Pipe OnSpawn cell={cell}");
        }

        protected override void OnCleanUp()
        {
            EqualizerNetwork.UnregisterPipe(cell);
            base.OnCleanUp();
        }
    }
}
