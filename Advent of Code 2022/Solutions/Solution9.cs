using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/9
/// </summary>
[Solution(9)]
[SolutionInput("Input9.test.txt", Enabled = false)]
[SolutionInput("Input9.txt", Benchmark = true, Problem1Solution = "6271", Problem2Solution = "2458")]
public class Solution9 : Solution
{
    private static readonly Dictionary<string, Point> Moves = new()
    {
        { "D", new Point(0, -1) },
        { "L", new Point(-1, 0) },
        { "U", new Point(0, 1) },
        { "R", new Point(1, 0) }
    };

    private readonly List<Motion> motions;

    public Solution9(Input input) : base(input)
    {
        motions = new List<Motion>();

        foreach (string line in Input.Lines)
        {
            string[] split = line.Split(' ');
            motions.Add(new Motion(Moves[split[0]], int.Parse(split[1])));
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return Simulate(2).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return Simulate(10).ToString();
    }

    private int Simulate(int knotCount)
    {
        List<Point> knots = Enumerable.Range(0, knotCount).Select(_ => new Point()).ToList();
        HashSet<Point> tailVisitations = new() { knots[^1] };

        foreach (Motion motion in motions)
        {
            for (int i = 0; i < motion.Count; i++)
            {
                knots[0] = knots[0].Move(motion.Move);
                
                for (int k = 1; k < knots.Count; k++)
                {
                    if (!knots[k - 1].IsTouching(knots[k]))
                    {
                        int xDiff = knots[k - 1].X - knots[k].X;
                        int yDiff = knots[k - 1].Y - knots[k].Y;

                        knots[k] = knots[k].Move(
                            new Point(
                                xDiff == 0 ? 0 : xDiff / Math.Abs(xDiff),
                                yDiff == 0 ? 0 : yDiff / Math.Abs(yDiff)));
                    }
                }
                
                tailVisitations.Add(knots[^1]);
            }
        }

        return tailVisitations.Count;
    }

    private readonly record struct Point(int X, int Y)
    {
        public Point Move(Point move)
        {
            return new Point(X + move.X, Y + move.Y);
        }

        public bool IsTouching(Point other)
        {
            return Math.Abs(X - other.X) <= 1 && Math.Abs(Y - other.Y) <= 1;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
    }

    private readonly record struct Motion(Point Move, int Count);
}