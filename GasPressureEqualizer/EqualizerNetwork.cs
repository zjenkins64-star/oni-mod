using System.Collections.Generic;
using UnityEngine;

namespace GasPressureEqualizer
{
    public static class EqualizerNetwork
    {
        public struct BridgeEndpoint
        {
            public int OutwardCell;
            public int TunnelPartner;
            public BridgeEndpoint(int outward, int partner) { OutwardCell = outward; TunnelPartner = partner; }
        }

        private static readonly HashSet<int> DuctCells = new HashSet<int>();
        private static readonly Dictionary<int, BridgeEndpoint> BridgeEndpoints = new Dictionary<int, BridgeEndpoint>();
        private static readonly Dictionary<int, GasPressureEqualizerVent> VentByCell = new Dictionary<int, GasPressureEqualizerVent>();

        private const float WAVE_SPEED_CELLS_PER_SEC = 4f;
        private const float WAVE_WIDTH_CELLS = 1.5f;
        private const float ACTIVITY_TIMEOUT = 0.6f;
        // Aim for one visible pulse roughly every WAVE_SPACING_CELLS along the
        // active path. Short runs get one slow-moving pulse; longer runs get
        // additional pulses staggered evenly so something is always on screen.
        private const float WAVE_SPACING_CELLS = 8f;

        private static readonly Dictionary<int, int> WaveIndexByCell = new Dictionary<int, int>();
        private static int activeSourceCell = -1;
        private static int activePathLength = 0;
        private static float lastActivityTime = -100f;

        private static bool IsPipeCell(int cell) => DuctCells.Contains(cell) || BridgeEndpoints.ContainsKey(cell);

        public static void RegisterPipe(int cell)
        {
            DuctCells.Add(cell);
            activeSourceCell = -1;
        }

        public static void UnregisterPipe(int cell)
        {
            DuctCells.Remove(cell);
            activeSourceCell = -1;
        }

        public static void RegisterVent(int cell, GasPressureEqualizerVent vent)
        {
            VentByCell[cell] = vent;
            activeSourceCell = -1;
        }

        public static void UnregisterVent(int cell)
        {
            VentByCell.Remove(cell);
            activeSourceCell = -1;
        }

        // Drops the cached active-flow source so the next call to
        // UpdateActiveFlowFromSource does a fresh BFS. Called when a vent's
        // Operational state flips so pipes leading to a now-disabled vent
        // stop pulsing on the next sim tick.
        public static void InvalidateActiveFlow()
        {
            activeSourceCell = -1;
        }

        public static void RegisterBridgeEndpoint(int cell, int outwardCell, int tunnelPartner)
        {
            BridgeEndpoints[cell] = new BridgeEndpoint(outwardCell, tunnelPartner);
            activeSourceCell = -1;
        }

        public static void UnregisterBridgeEndpoint(int cell)
        {
            BridgeEndpoints.Remove(cell);
            activeSourceCell = -1;
        }

        public static List<GasPressureEqualizerVent> FindConnectedVents(GasPressureEqualizerVent self, int selfCell)
        {
            var result = new List<GasPressureEqualizerVent>();
            if (DuctCells.Count == 0 && BridgeEndpoints.Count == 0) return result;

            var visitedPipes = new HashSet<int>();
            var queue = new Queue<int>();

            if (IsPipeCell(selfCell))
            {
                visitedPipes.Add(selfCell);
                queue.Enqueue(selfCell);
            }
            foreach (int n in NeighborsOf(selfCell))
            {
                if (IsPipeCell(n) && visitedPipes.Add(n))
                {
                    queue.Enqueue(n);
                }
            }

            if (queue.Count == 0) return result;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (int n in NeighborsOf(current))
                {
                    if (IsPipeCell(n) && visitedPipes.Add(n))
                    {
                        queue.Enqueue(n);
                    }
                }
            }

            // Strict on-top placement: a vent counts as connected only when
            // its own cell is a duct/bridge cell. Adjacent vents are ignored.
            foreach (var kvp in VentByCell)
            {
                var v = kvp.Value;
                if (v == self) continue;
                if (visitedPipes.Contains(kvp.Key))
                {
                    result.Add(v);
                }
            }

            return result;
        }

        public static void UpdateActiveFlowFromSource(int sourceCell)
        {
            lastActivityTime = Time.time;

            if (sourceCell == activeSourceCell && activePathLength > 0)
            {
                return;
            }

            activeSourceCell = sourceCell;
            WaveIndexByCell.Clear();
            activePathLength = 0;

            if (DuctCells.Count == 0 && BridgeEndpoints.Count == 0) return;

            var depthMap = new Dictionary<int, int>();
            var parent = new Dictionary<int, int>();
            var queue = new Queue<int>();

            foreach (int c in CellsAtOrAdjacent(sourceCell))
            {
                if (IsPipeCell(c) && !depthMap.ContainsKey(c))
                {
                    depthMap[c] = 0;
                    parent[c] = -1;
                    queue.Enqueue(c);
                }
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                int depth = depthMap[current];
                foreach (int n in NeighborsOf(current))
                {
                    if (IsPipeCell(n) && !depthMap.ContainsKey(n))
                    {
                        depthMap[n] = depth + 1;
                        parent[n] = current;
                        queue.Enqueue(n);
                    }
                }
            }

            int maxDepth = 0;
            foreach (var kvp in VentByCell)
            {
                if (kvp.Key == sourceCell) continue;
                // Strict on-top: peer vent's cell must itself be a duct cell.
                if (!depthMap.ContainsKey(kvp.Key)) continue;
                // Disabled vents are sealed off — pipes leading to them
                // shouldn't pulse, since no gas is flowing there.
                if (!kvp.Value.IsOperational) continue;

                int current = kvp.Key;
                while (current != -1)
                {
                    int d = depthMap[current];
                    WaveIndexByCell[current] = d;
                    if (d > maxDepth) maxDepth = d;
                    current = parent.TryGetValue(current, out int p) ? p : -1;
                }
            }

            if (WaveIndexByCell.Count > 0)
            {
                activePathLength = maxDepth + 1;
            }
        }

        public static bool TryGetWaveBrightness(int cell, out float brightness)
        {
            brightness = 0f;
            if (activePathLength == 0) return false;
            if (Time.time - lastActivityTime > ACTIVITY_TIMEOUT) return false;
            if (!WaveIndexByCell.TryGetValue(cell, out int idx)) return false;

            float wavePeriod = activePathLength + WAVE_WIDTH_CELLS * 2f;
            int waveCount = Mathf.Max(1, Mathf.RoundToInt(activePathLength / WAVE_SPACING_CELLS));
            float baseWavePos = (Time.time * WAVE_SPEED_CELLS_PER_SEC) % wavePeriod;
            float best = 0f;
            for (int i = 0; i < waveCount; i++)
            {
                float phaseOffset = wavePeriod * i / waveCount;
                float wavePos = ((baseWavePos + phaseOffset) % wavePeriod) - WAVE_WIDTH_CELLS;
                float dist = Mathf.Abs(idx - wavePos);
                float b = Mathf.Clamp01(1f - dist / WAVE_WIDTH_CELLS);
                if (b > best) best = b;
            }
            brightness = best;
            return true;
        }

        private static IEnumerable<int> CellsAtOrAdjacent(int cell)
        {
            yield return cell;
            foreach (int n in NeighborsOf(cell)) yield return n;
        }

        // Returns the BFS neighbors of a cell. For bridge endpoints, only the
        // outward cell and the tunnel partner are valid; the perpendicular sides
        // are blocked, which is what keeps two crossing networks from merging
        // through the bridge cell.
        private static IEnumerable<int> NeighborsOf(int cell)
        {
            if (BridgeEndpoints.TryGetValue(cell, out BridgeEndpoint ep))
            {
                yield return ep.OutwardCell;
                yield return ep.TunnelPartner;
                yield break;
            }

            int up = Grid.CellAbove(cell);
            int down = Grid.CellBelow(cell);
            int left = Grid.CellLeft(cell);
            int right = Grid.CellRight(cell);

            if (CanReach(cell, up)) yield return up;
            if (CanReach(cell, down)) yield return down;
            if (CanReach(cell, left)) yield return left;
            if (CanReach(cell, right)) yield return right;
        }

        // A cell can reach a neighbor unless that neighbor is a bridge endpoint
        // approached from a non-outward direction.
        private static bool CanReach(int from, int to)
        {
            if (BridgeEndpoints.TryGetValue(to, out BridgeEndpoint ep))
            {
                return ep.OutwardCell == from;
            }
            return true;
        }
    }
}
