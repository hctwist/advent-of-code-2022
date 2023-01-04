using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/15
/// </summary>
[Solution(15, Enabled = false)]
[SolutionInput("Input15.txt", Benchmark = true, Problem1Solution = "5147333", Problem2Solution = "13734006908372")]
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
            int yDistanceToTargetRow = Math.Abs(sensor.Position.Y - targetRow);
            int distanceRemaining = sensor.Range - yDistanceToTargetRow;

            if (distanceRemaining > 0)
            {
                ranges.Add(
                    new ClosedRange(
                        sensor.Position.X - distanceRemaining,
                        sensor.Position.X + distanceRemaining));
            }
        }

        return ClosedRange.Aggregate(ranges).Sum(r => r.Length).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        Point point = new(0, 0);

        int searchRange = 4_000_000;

        while (point.X <= searchRange & point.Y <= searchRange)
        {
            if (!TryFindSensorCovering(point, out Sensor sensor))
            {
                return ((long)point.X * searchRange + point.Y).ToString();
            }

            int distanceToTargetY = Math.Abs(sensor.Position.Y - point.Y);
            int rangeRemainingX = sensor.Range - distanceToTargetY;

            if (rangeRemainingX <= 0)
            {
                throw new InvalidOperationException($"Found a sensor covering {point} with no range remaining");
            }

            int sensorMaxX = sensor.Position.X + rangeRemainingX;

            if (sensorMaxX < searchRange)
            {
                point = point with { X = sensorMaxX + 1 };
            }
            else
            {
                int sensorMinX = sensor.Position.X - rangeRemainingX;

                if (sensorMinX < 0)
                {
                    point = point with { X = 0, Y = point.Y - sensorMinX + 1 };
                }
                else
                {
                    point = point with { X = 0, Y = point.Y + 1 };
                }
            }
        }

        throw new Exception("Could not find the distress beacon");
    }

    private bool TryFindSensorCovering(Point point, out Sensor sensor)
    {
        foreach (Sensor s in sensors)
        {
            if (s.Position.ManhattanDistanceTo(point) <= s.Range)
            {
                sensor = s;
                return true;
            }
        }

        sensor = default;
        return false;
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

        public int Range => Position.ManhattanDistanceTo(BeaconPosition);
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

            return aggregatedRanges;
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