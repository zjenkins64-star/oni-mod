using UnityEngine;

namespace GasPressureEqualizer
{
    public class GasPressureEqualizerVent : KMonoBehaviour, ISim200ms
    {
        public int Cell => cell;

        // True unless an automation signal (via LogicOperationalController +
        // Operational) has disabled this vent. Used by other peers and by
        // EqualizerNetwork to skip closed-off vents during target selection,
        // global-max comparison, and wave depth-map tracing.
        public bool IsOperational => operational == null || operational.IsOperational;

        private const float MAX_TRANSFER_KG = 5f;
        // 1 g — system stops moving gas once peer rooms are within a gram of
        // each other. Previously 50 g (0.05 kg), which made small absolute
        // amounts (e.g. a 100 g room balancing into a vacuum) cut off about
        // 25 g shy of true equilibrium.
        private const float DEAD_BAND_KG = 0.001f;
        private const float TRANSFER_RATE = 0.25f;
        private const int LOG_EVERY_N_TICKS = 5;
        private static readonly Color BASE_TINT = Color.white;

        // Anim names from the extracted ventgas kanim:
        //   working_loop = animated "running" cycle (used when actively moving gas)
        //   on           = static idle frame (used when network is at equilibrium)
        //   closed       = static sealed-vent frame (used when automation is red)
        // We tried gating animation rate via KBatchedAnimController.PlaySpeedMultiplier
        // for a gradient slow-down approaching equilibrium; the property was being
        // set successfully but didn't affect playback rate on batched anims. Instead
        // we swap between the looping anim and a static frame for a clean stop.
        private const string ANIM_RUNNING = "working_loop";
        private const string ANIM_IDLE = "on";
        private const string ANIM_CLOSED = "closed";

        private int cell;
        private int tickCount;
        private Operational operational;
        // Tracks the previous tick's operational state so we can invalidate
        // the network's cached flow path the moment automation flips a vent.
        private bool lastOperational = true;
        // Latches the currently-playing anim name so we don't restart the
        // loop from frame 0 every Sim200ms tick.
        private string currentAnim = "";

        // Diagnostic: track the most recent peer count we logged, so we
        // only emit a log line when it changes (otherwise every 200ms tick
        // would spam the log). We initialize to a sentinel so the first
        // tick always logs.
        private int lastPeerCount = -1;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            cell = Grid.PosToCell(this);
            EqualizerNetwork.RegisterVent(cell, this);
            operational = GetComponent<Operational>();

            var anim = GetComponent<KBatchedAnimController>();
            if (anim != null)
            {
                anim.TintColour = BASE_TINT;
            }
            UpdateAnimState(active: false);
            lastOperational = IsOperational;

            // Diagnostic: full cell snapshot so a vent placed on a non-duct
            // cell (or on a stock pipe, or on an under-construction duct) is
            // immediately visible in the log. Also dump the 4 cardinal
            // neighbors — that's what FindConnectedVents falls back on when
            // the vent's own cell isn't a pipe cell, so it tells us whether
            // any pipe is even nearby.
            bool onDuct = EqualizerNetwork.IsRegisteredDuctCell(cell);
            bool onBridge = EqualizerNetwork.IsBridgeEndpointRegistered(cell);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[GPE] Vent OnSpawn cell={cell} onDuct={onDuct} onBridgeEndpoint={onBridge}");
            sb.AppendLine($"[GPE]   self:  {EqualizerNetwork.DescribeCell(cell)}");
            sb.AppendLine($"[GPE]   above: {EqualizerNetwork.DescribeCell(Grid.CellAbove(cell))}");
            sb.AppendLine($"[GPE]   below: {EqualizerNetwork.DescribeCell(Grid.CellBelow(cell))}");
            sb.AppendLine($"[GPE]   left:  {EqualizerNetwork.DescribeCell(Grid.CellLeft(cell))}");
            sb.Append    ($"[GPE]   right: {EqualizerNetwork.DescribeCell(Grid.CellRight(cell))}");
            Debug.Log(sb.ToString());
        }

        // Picks the right anim for the current state and only swaps if it
        // differs from what's already playing — otherwise looped anims would
        // restart from frame 0 every Sim200ms tick.
        //   automation off                      -> closed (static)
        //   network pressure spread above band  -> working_loop (animated)
        //   at equilibrium / no peers / vacuum  -> on (static idle)
        private void UpdateAnimState(bool active)
        {
            string desired;
            if (!IsOperational) desired = ANIM_CLOSED;
            else if (active)    desired = ANIM_RUNNING;
            else                desired = ANIM_IDLE;
            if (currentAnim == desired) return;
            var anim = GetComponent<KBatchedAnimController>();
            if (anim == null) return;
            anim.Play(desired, KAnim.PlayMode.Loop);
            currentAnim = desired;
        }

        protected override void OnCleanUp()
        {
            EqualizerNetwork.UnregisterVent(cell);
            base.OnCleanUp();
        }

        public void Sim200ms(float dt)
        {
            // Detect operational-state flips so the wave's cached source/path
            // can be invalidated; without this, pipes leading to a disabled
            // vent keep pulsing for as long as the same global-max vent stays
            // global-max (cache hit, no recompute).
            bool nowOperational = IsOperational;
            if (nowOperational != lastOperational)
            {
                EqualizerNetwork.InvalidateActiveFlow();
                lastOperational = nowOperational;
            }

            // LogicOperationalController flips Operational.IsOperational based
            // on the automation port's signal. When red, skip the entire tick:
            // no gas transfer, no pulse animation kick.
            if (!nowOperational)
            {
                UpdateAnimState(active: false);
                return;
            }

            Element roomElement = Grid.Element[cell];
            if (roomElement.IsSolid || roomElement.IsLiquid || !roomElement.IsGas)
            {
                UpdateAnimState(active: false);
                return;
            }

            float myMass = Grid.Mass[cell];

            var peers = EqualizerNetwork.FindConnectedVents(this, cell);

            // Diagnostic: peer count flips. We only log when it changes,
            // so a steady-state vent with zero peers logs once (when it
            // first hits zero) instead of every 200ms.
            if (peers.Count != lastPeerCount)
            {
                int totalRegistered = EqualizerNetwork.RegisteredVentCount;
                bool onDuct = EqualizerNetwork.IsRegisteredDuctCell(cell);
                int reachable = EqualizerNetwork.CountReachablePipeCellsFromCell(cell);
                Debug.Log($"[GPE-Vent] cell={cell} peers={peers.Count} (was {lastPeerCount}) " +
                          $"totalVentsRegistered={totalRegistered} onDuct={onDuct} reachablePipeCells={reachable}");
                lastPeerCount = peers.Count;
            }

            if (peers.Count == 0)
            {
                UpdateAnimState(active: false);
                return;
            }

            // Walk the peer set once to find the lowest-mass enabled target
            // and (for the anim) the overall mass spread across the locally
            // connected operational vent set, including ourselves.
            GasPressureEqualizerVent target = null;
            float targetMass = myMass;
            float maxMass = myMass;
            float minMass = myMass;
            foreach (var p in peers)
            {
                if (!p.IsOperational) continue;
                Element e = Grid.Element[p.Cell];
                if (e.IsSolid || e.IsLiquid) continue;
                float m = Grid.Mass[p.Cell];
                if (m < targetMass)
                {
                    target = p;
                    targetMass = m;
                }
                if (m > maxMass) maxMass = m;
                if (m < minMass) minMass = m;
            }
            // Anim swap: every vent on the local connected set animates while
            // there's a real pressure spread, and snaps to the static idle
            // frame the moment the rooms have equalized within the dead band.
            UpdateAnimState(active: (maxMass - minMass) >= DEAD_BAND_KG);

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
            // don't all overwrite each other. Disabled peers are excluded —
            // otherwise a closed-off high-mass vent could still claim "max"
            // and silently suppress every other vent's wave update.
            bool iAmGlobalMax = true;
            foreach (var p in peers)
            {
                if (!p.IsOperational) continue;
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
