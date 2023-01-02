using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/15
/// </summary>
[Solution(15)]
[SolutionInput("Input15.test.txt")]
[SolutionInput("Input15.txt", Benchmark = true)]
public class Solution15 : Solution
{
    private static readonly Regex SensorPattern = new(@"^Sensor at x=(-?\d+), y=(-?\d+): closest beacon is at x=(-?\d+), y=(-?\d+)$");

    private readonly List<Sensor> sensors;

    /// <inheritdoc />
    public Solution15(Input input) : base(input)
    {
        sensors = new List<Sensor>(input.Lines.Length);

        foreach (string line in Input.Lines)
        {
            Match match = SensorPattern.Match(line);

            Sensor sensor = new(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                int.Parse(match.Groups[4].Value));
            sensors.Add(sensor);
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        int targetRow = 2_000_000;
        
        List<ClosedRange> ranges = new();

        foreach (Sensor sensor in sensors)
        {
            int beaconDistance = sensor.Position.ManhattanDistanceTo(sensor.BeaconPosition);

            int yDistanceToTargetRow = Math.Abs(sensor.Position.Y - targetRow);
            int distanceRemaining = beaconDistance - yDistanceToTargetRow;

            if (distanceRemaining > 0)
            {
                ranges.Add(new ClosedRange(
                    sensor.Position.X - distanceRemaining, 
                    sensor.Position.X + distanceRemaining));
            }
        }

        return ClosedRange.Aggregate(ranges).Sum(r => r.Length).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }

    private readonly record struct Point(int X, int Y)
    {
        public int ManhattanDistanceTo(Point other)
        {
            return Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
        }
    }

    private readonly record struct Sensor(Point Position, Point BeaconPosition)
    {
        public Sensor(int positionX, int positionY, int beaconPositionX, int beaconPositionY) :
            this(new Point(positionX, positionY), new Point(beaconPositionX, beaconPositionY))
        {
        }
    }

    private readonly record struct ClosedRange(int From, int To)
    {
        public int Length => To - From;

        public static List<ClosedRange> Aggregate(IEnumerable<ClosedRange> ranges)
        {
            List<ClosedRange> aggregatedRanges = new();

            void AggregateRange(ClosedRange range)
            {
                for (int i = 0; i < aggregatedRanges.Count; i++)
                {
                    if (range.Intersects(aggregatedRanges[i]))
                    {
                        aggregatedRanges[i] = aggregatedRanges[i].Union(range);
                        return;
                    }
                }
            
                aggregatedRanges.Add(range);
            }

            foreach (ClosedRange range in ranges)
            {
                AggregateRange(range);
            }
        }

        public bool Intersects(ClosedRange other)
        {
            return Contains(other.From) | Contains(other.To);
        }

        public ClosedRange Union(ClosedRange other)
        {
            return new ClosedRange(Math.Min(From, other.From), Math.Max(To, other.To));
        }

        public bool Contains(int point)
        {
            return From <= point | point <= To;
        }
    }
}