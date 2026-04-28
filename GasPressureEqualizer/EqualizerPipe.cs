using UnityEngine;

namespace GasPressureEqualizer
{
    public class EqualizerPipe : KMonoBehaviour
    {
        private static readonly Color RED_BASE = new Color(0.7f, 0.2f, 0.2f);
        private static readonly Color RED_BRIGHT = new Color(1f, 0.95f, 0.45f);

        private int cell;
        private KBatchedAnimController anim;

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

            anim = GetComponent<KBatchedAnimController>();
            if (anim != null)
            {
                anim.TintColour = RED_BASE;
            }
        }

        protected override void OnCleanUp()
        {
            EqualizerNetwork.UnregisterPipe(cell);
            base.OnCleanUp();
        }

        private void Update()
        {
            if (anim == null) return;

            if (EqualizerNetwork.TryGetWaveBrightness(cell, out float b))
            {
                anim.TintColour = Color.Lerp(RED_BASE, RED_BRIGHT, b);
            }
            else
            {
                anim.TintColour = RED_BASE;
            }
        }
    }
}
