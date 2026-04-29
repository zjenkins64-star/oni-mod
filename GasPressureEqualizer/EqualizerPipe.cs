using UnityEngine;

namespace GasPressureEqualizer
{
    public class EqualizerPipe : KMonoBehaviour
    {
        // White base = the painted PNG colors show through unchanged.
        // Wave shifts to a strong warm yellow + brightness boost so the flow
        // pulse pops visibly over the painted texture.
        private static readonly Color BASE_TINT = Color.white;
        private static readonly Color WAVE_TINT = new Color(1.6f, 1.4f, 0.6f);

        private int cell;
        private KBatchedAnimController anim;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            cell = Grid.PosToCell(this);
            EqualizerNetwork.RegisterPipe(cell);

            var conduit = GetComponent<Conduit>();
            if (conduit != null)
            {
                conduit.Disconnect();
            }

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
