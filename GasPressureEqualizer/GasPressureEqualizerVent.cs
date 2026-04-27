using UnityEngine;

namespace GasPressureEqualizer
{
    public class GasPressureEqualizerVent : KMonoBehaviour, ISim200ms
    {
        private int cell;
        private ConduitFlow gasFlow;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            cell = Grid.PosToCell(this);
            gasFlow = Conduit.GetFlowManager(ConduitType.Gas);
        }

        public void Sim200ms(float dt)
        {
            if (gasFlow == null)
                return;

            if (!gasFlow.HasConduit(cell))
                return;

            // -------------------------
            // INPUT: pull small gas packet
            // -------------------------
            var input = gasFlow.RemoveElement(cell, 0.1f);

            if (input.mass > 0f)
            {
                // -------------------------
                // OUTPUT: immediately re-inject
                // (minimal equalizer behavior)
                // -------------------------
                gasFlow.AddElement(
                    cell,
                    input.element,
                    input.mass,
                    input.temperature,
                    input.diseaseIdx,
                    input.diseaseCount
                );
            }
        }
    }
}