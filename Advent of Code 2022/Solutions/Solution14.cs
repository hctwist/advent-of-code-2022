using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/14
/// </summary>
[Solution(14)]
[SolutionInput("Input14.test.txt", Enabled = false)]
[SolutionInput("Input14.txt", Benchmark = true, Problem1Solution = "1061", Problem2Solution = "25055")]
public class Solution14 : Solution
{
    private readonly HashSet<Point> formations;

    public Solution14(Input input) : base(input)
    {
        formations = new HashSet<Point>();

        foreach (string line in Input.Lines)
        {
            string[] points = line.Split(" -> ");

            Point start = Point.Parse(points[0]);

            foreach (string pointString in points.Skip(1))
            {
                Point end = Point.Parse(pointString);
                AddFormations(start, end);
                start = end;
            }
            
            formations.Add(start);
        }
    }

    private void AddFormations(Point from, Point to)
    {
        int dx = Math.Sign(to.X - from.X);
        int dy = Math.Sign(to.Y - from.Y);

        if ((dx == 0) == (dy == 0))
        {
            throw new InvalidOperationException("Cannot parse points changing in both X and Y directions");
        }

        if (dx != 0)
        {
            for (int i = 0; i < Math.Abs(from.X - to.X); i++)
            {
                formations.Add(new Point(from.X + i * dx, from.Y));
            }
        }
        else
        {
            for (int i = 0; i < Math.Abs(from.Y - to.Y); i++)
            {
                formations.Add(new Point(from.X, from.Y + i * dy));
            }
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        Point start = new(500, 0);
        int voidY = formations.Max(rock => rock.Y) + 1;
        
        Point sand = start;
        int count = 0;

        while (sand.Y < voidY)
        {
            if (TryGetFallPoint(sand, out Point fallPoint))
            {
                sand = fallPoint;
            }
            else
            {
                formations.Add(sand);
                count++;
                sand = start;
            }
        }

        return count.ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        Point start = new(500, 0);
        int floorY = formations.Max(rock => rock.Y) + 2;

        Point sand = start;
        int count = 0;

        while (true)
        {
            if (sand.Y == floorY - 1 || !TryGetFallPoint(sand, out Point fallPoint))
            {
                if (sand == start)
                {
                    break;
                }
                
                formations.Add(sand);
                count++;
                sand = start;
            }
            else
            {
                sand = fallPoint;
            }
        }

        return (count + 1).ToString();
    }
    
    private bool TryGetFallPoint(Point from, out Point fallPoint)
    {
        // No formation below the sand
        if (!formations.Contains(new Point(from.X, from.Y + 1)))
        {
            fallPoint = new Point(from.X, from.Y + 1);
            return true;
        }
        // No formations at the bottom left
        else if (!formations.Contains(new Point(from.X - 1, from.Y + 1)))
        {
            fallPoint = new Point(from.X - 1, from.Y + 1);
            return true;
        }
        // No formations at the bottom right
        else if (!formations.Contains(new Point(from.X + 1, from.Y + 1)))
        {
            fallPoint = new Point(from.X + 1, from.Y + 1);
            return true;
        }
        else
        {
            fallPoint = default;
            return false;
        }
    }

    private readonly record struct Point(int X, int Y)
    {
        public static Point Parse(string str)
        {
            string[] split = str.Split(',');
            return new Point(int.Parse(split[0]), int.Parse(split[1]));
        }
    }
}