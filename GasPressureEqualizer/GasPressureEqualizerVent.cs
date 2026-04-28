using UnityEngine;

namespace GasPressureEqualizer
{
    public class GasPressureEqualizerVent : KMonoBehaviour, ISim200ms
    {
        public int Cell => cell;

        private const float MAX_TRANSFER_KG = 5f;
        private const float DEAD_BAND_KG = 0.05f;
        private const float TRANSFER_RATE = 0.25f;
        private const int LOG_EVERY_N_TICKS = 5;
        private static readonly Color RED_TINT = new Color(1f, 0.45f, 0.45f);

        private int cell;
        private int tickCount;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            cell = Grid.PosToCell(this);
            EqualizerNetwork.RegisterVent(cell, this);

            var anim = GetComponent<KBatchedAnimController>();
            if (anim != null)
            {
                anim.TintColour = RED_TINT;
            }

            Debug.Log($"[GPE] Vent OnSpawn cell={cell}");
        }

        protected override void OnCleanUp()
        {
            EqualizerNetwork.UnregisterVent(cell);
            base.OnCleanUp();
        }

        public void Sim200ms(float dt)
        {
            Element roomElement = Grid.Element[cell];
            if (roomElement.IsSolid || roomElement.IsLiquid) return;
            if (!roomElement.IsGas) return;

            float myMass = Grid.Mass[cell];

            var peers = EqualizerNetwork.FindConnectedVents(this, cell);
            if (peers.Count == 0) return;

            GasPressureEqualizerVent target = null;
            float targetMass = myMass;
            foreach (var p in peers)
            {
                Element e = Grid.Element[p.Cell];
                if (e.IsSolid || e.IsLiquid) continue;
                float m = Grid.Mass[p.Cell];
                if (m < targetMass)
                {
                    target = p;
                    targetMass = m;
                }
            }

            tickCount++;
            bool shouldLog = (tickCount % LOG_EVERY_N_TICKS) == 0;

            if (target == null) return;
            float diff = myMass - targetMass;
            if (diff < DEAD_BAND_KG) return;

            float transfer = Mathf.Min(diff * TRANSFER_RATE, MAX_TRANSFER_KG);
            int diseaseTransfer = (int)(Grid.DiseaseCount[cell] * (transfer / Mathf.Max(myMass, 0.001f)));
            float temp = Grid.Temperature[cell];
            byte diseaseIdx = Grid.DiseaseIdx[cell];

            SimMessages.AddRemoveSubstance(cell, roomElement.id, null,
                -transfer, temp, diseaseIdx, -diseaseTransfer, true, -1);
            SimMessages.AddRemoveSubstance(target.Cell, roomElement.id, null,
                transfer, temp, diseaseIdx, diseaseTransfer, true, -1);

            // Only the global-maximum vent in this network drives the duct flow
            // animation. With cell-index tiebreaker so multiple max-mass vents
            // don't all overwrite each other.
            bool iAmGlobalMax = true;
            foreach (var p in peers)
            {
                float pm = Grid.Mass[p.Cell];
                if (pm > myMass || (pm == myMass && p.Cell < cell))
                {
                    iAmGlobalMax = false;
                    break;
                }
            }
            if (iAmGlobalMax)
            {
                EqualizerNetwork.UpdateActiveFlowFromSource(cell);
            }

            if (shouldLog)
            {
                Debug.Log($"[GPE] cell={cell} myMass={myMass:F2} → cell={target.Cell} (mass={targetMass:F2}) transfer={transfer:F3} peers={peers.Count}");
            }
        }
    }
}
