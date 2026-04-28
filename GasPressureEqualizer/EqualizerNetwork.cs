using System.Collections.Generic;

namespace GasPressureEqualizer
{
    public static class EqualizerNetwork
    {
        private static readonly HashSet<int> PipeCells = new HashSet<int>();
        private static readonly Dictionary<int, GasPressureEqualizerVent> VentByCell = new Dictionary<int, GasPressureEqualizerVent>();

        public static void RegisterPipe(int cell) => PipeCells.Add(cell);
        public static void UnregisterPipe(int cell) => PipeCells.Remove(cell);

        public static void RegisterVent(int cell, GasPressureEqualizerVent vent) => VentByCell[cell] = vent;
        public static void UnregisterVent(int cell) => VentByCell.Remove(cell);

        public static List<GasPressureEqualizerVent> FindConnectedVents(GasPressureEqualizerVent self, int selfCell)
        {
            var result = new List<GasPressureEqualizerVent>();
            if (PipeCells.Count == 0) return result;

            var visitedPipes = new HashSet<int>();
            var queue = new Queue<int>();

            // Seed BFS with pipe cells at or adjacent to the vent's own cell.
            if (PipeCells.Contains(selfCell))
            {
                visitedPipes.Add(selfCell);
                queue.Enqueue(selfCell);
            }
            foreach (int n in Neighbors(selfCell))
            {
                if (PipeCells.Contains(n) && visitedPipes.Add(n))
                {
                    queue.Enqueue(n);
                }
            }

            if (queue.Count == 0) return result;

            // BFS through pipe cells.
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (int n in Neighbors(current))
                {
                    if (PipeCells.Contains(n) && visitedPipes.Add(n))
                    {
                        queue.Enqueue(n);
                    }
                }
            }

            // Any vent at-or-adjacent to a visited pipe cell is in our network.
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
                foreach (int n in Neighbors(vc))
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

        private static IEnumerable<int> Neighbors(int cell)
        {
            yield return Grid.CellAbove(cell);
            yield return Grid.CellBelow(cell);
            yield return Grid.CellLeft(cell);
            yield return Grid.CellRight(cell);
        }
    }
}
