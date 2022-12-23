using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/12
/// </summary>
[Solution(12)]
[SolutionInput("Input12.test.txt", Enabled = false)]
[SolutionInput("Input12.txt", Benchmark = true, Problem1Solution = "394", Problem2Solution = "388")]
public class Solution12 : Solution
{
    private readonly DijkstraNode[,] map;

    private readonly DijkstraNode start;

    private readonly DijkstraNode end;

    public Solution12(Input input) : base(input)
    {
        map = new DijkstraNode[Input.Lines.Length, Input.Lines[0].Length];

        for (int r = 0; r < map.GetLength(0); r++)
        {
            for (int c = 0; c < map.GetLength(1); c++)
            {
                Point point = new(r, c);
                switch (Input.Lines[r][c])
                {
                    case 'S':
                        start = new DijkstraNode(point, 0);
                        map[r, c] = start;
                        break;
                    case 'E':
                        end = new DijkstraNode(point, 'z' - 'a');
                        map[r, c] = end;
                        break;
                    default:
                        map[r, c] = new DijkstraNode(point, Input.Lines[r][c] - 'a');
                        break;
                }
            }
        }

        if (start == null || end == null)
        {
            throw new Exception("Could not find both start and end points");
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return GetShortestPath(start).ToString();
    }
    
    /// <inheritdoc />
    protected override string? Problem2()
    {
        void Reset()
        {
            for (int r = 0; r < map.GetLength(0); r++)
            {
                for (int c = 0; c < map.GetLength(1); c++)
                {
                    map[r, c].TentativeDistance = int.MaxValue;
                    map[r, c].Visited = false;
                }
            }
        }

        List<DijkstraNode> startingPoints = new();
        for (int r = 0; r < map.GetLength(0); r++)
        {
            for (int c = 0; c < map.GetLength(1); c++)
            {
                if (map[r, c].Height == 0)
                {
                    startingPoints.Add(map[r, c]);
                }
            }
        }

        int bestDistance = int.MaxValue;

        for (int i = 0; i < startingPoints.Count; i++)
        {
            if (i != 0)
            {
                Reset();
            }

            bestDistance = int.Min(bestDistance, GetShortestPath(startingPoints[i]));
        }

        return bestDistance.ToString();
    }
    

    private int GetShortestPath(DijkstraNode from)
    {
        from.TentativeDistance = 0;
        HashSet<DijkstraNode> queue = new() { from };

        while (queue.Count > 0)
        {
            DijkstraNode node = queue.MinBy(node => node.TentativeDistance)!;
            if (node == end)
            {
                break;
            }

            node.Visited = true;
            queue.Remove(node);

            foreach (DijkstraNode adjacent in GetAdjacent(node))
            {
                if (adjacent.Height - node.Height <= 1)
                {
                    adjacent.TentativeDistance = Math.Min(adjacent.TentativeDistance, node.TentativeDistance + 1);

                    if (!adjacent.Visited)
                    {
                        queue.Add(adjacent);
                    }
                }
            }
        }

        return end.TentativeDistance;
    }

    private IEnumerable<DijkstraNode> GetAdjacent(DijkstraNode node)
    {
        Point point = node.Point;

        // Left
        if (IsWithinMap(point.Row, point.Column - 1))
        {
            yield return map[point.Row, point.Column - 1];
        }

        // Top
        if (IsWithinMap(point.Row - 1, point.Column))
        {
            yield return map[point.Row - 1, point.Column];
        }

        // Right
        if (IsWithinMap(point.Row, point.Column + 1))
        {
            yield return map[point.Row, point.Column + 1];
        }

        // Bottom
        if (IsWithinMap(point.Row + 1, point.Column))
        {
            yield return map[point.Row + 1, point.Column];
        }
    }

    private bool IsWithinMap(int row, int column)
    {
        return (0 <= row & row < map.GetLength(0)) & (0 <= column & column < map.GetLength(1));
    }

    private class DijkstraNode : IComparable<DijkstraNode>
    {
        public readonly Point Point;

        public readonly int Height;

        public int TentativeDistance;

        public bool Visited;

        public DijkstraNode(Point point, int height)
        {
            Point = point;
            Height = height;
            
            TentativeDistance = int.MaxValue;
            Visited = false;
        }

        /// <inheritdoc />
        public int CompareTo(DijkstraNode? other)
        {
            return other is null ? 1 : TentativeDistance.CompareTo(other.TentativeDistance);
        }
    }

    private readonly record struct Point(int Row, int Column);
}