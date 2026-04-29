using UnityEngine;

namespace GasPressureEqualizer
{
    public class EqualizerPipe : KMonoBehaviour
    {
        // White base = the painted PNG colors show through unchanged.
        // Wave darkens the painted texture toward middle grey for a subdued
        // greyscale pulse instead of a coloured highlight.
        private static readonly Color BASE_TINT = Color.white;
        private static readonly Color WAVE_TINT = new Color(0.5f, 0.5f, 0.5f);

        private int cell;
        private KBatchedAnimController anim;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            cell = Grid.PosToCell(this);
            EqualizerNetwork.RegisterPipe(cell);

            // Previously we called conduit.Disconnect() to keep our ducts off
            // the stock gas network, but that made the gas-pipe overlay flag
            // them as disconnected (red X). Leaving them connected; isolation
            // from stock gas pipes is now handled by the Refresh / SetConnections
            // Harmony patches and (eventually) per-cell flow filtering.
            // var conduit = GetComponent<Conduit>();
            // if (conduit != null) conduit.Disconnect();

            anim = GetComponent<KBatchedAnimController>();
            if (anim != null)
            {
                anim.TintColour = BASE_TINT;
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
                anim.TintColour = Color.Lerp(BASE_TINT, WAVE_TINT, b);
            }
            else
            {
                anim.TintColour = BASE_TINT;
            }
        }
    }
}
