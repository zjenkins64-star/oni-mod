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

            foreach (var kvp in VentByCell)
            {
                var v = kvp.Value;
                if (v == self) continue;
                int vc = kvp.Key;
                if (visitedPipes.Contains(vc))
                {
                    result.Add(v);
                    continue;
                }
                foreach (int n in NeighborsOf(vc))
                {
                    if (visitedPipes.Contains(n))
                    {
                        result.Add(v);
                        break;
                    }
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

                int entry = -1;
                int entryDepth = int.MaxValue;
                foreach (int c in CellsAtOrAdjacent(kvp.Key))
                {
                    if (depthMap.TryGetValue(c, out int d) && d < entryDepth)
                    {
                        entry = c;
                        entryDepth = d;
                    }
                }
                if (entry == -1) continue;

                int current = entry;
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
            float wavePos = (Time.time * WAVE_SPEED_CELLS_PER_SEC) % wavePeriod - WAVE_WIDTH_CELLS;
            float dist = Mathf.Abs(idx - wavePos);
            brightness = Mathf.Clamp01(1f - dist / WAVE_WIDTH_CELLS);
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
