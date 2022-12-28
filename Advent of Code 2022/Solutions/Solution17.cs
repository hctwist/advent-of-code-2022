using System.Collections;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/17
/// </summary>
[Solution(17)]
[SolutionInput("Input17.test.txt")]
[SolutionInput("Input17.txt", Benchmark = true)]
public class Solution17 : Solution
{
    private readonly List<RockStructure> rocks;

    private readonly List<Direction> directions;

    /// <inheritdoc />
    public Solution17(Input input) : base(input)
    {
        rocks = new List<RockStructure>()
        {
            RockStructure.Minus,
            RockStructure.Plus,
            RockStructure.Corner,
            RockStructure.Tall,
            RockStructure.Square
        };

        directions = new List<Direction>(Input.Raw.Length);

        foreach (char direction in Input.Raw)
        {
            directions.Add(
                direction switch
                {
                    '<' => Direction.Left,
                    '>' => Direction.Right,
                    _ => throw new Exception($"Could not parse direction: {direction}")
                });
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        int rockCount = 2_022;

        Chamber chamber = new(7, new HashSet<Point>(rockCount * 4));

        int height = 0;

        int d = 0;
        for (int i = 0; i < rockCount; i++)
        {
            Rock rock = new(rocks[i % rocks.Count], new Point(2, height + 3));

            while (rock.Offset.Y >= 0)
            {
                Direction jetDirection = directions[d++ % directions.Count];
                Rock blownRock = rock.Translate(jetDirection);

                if (!chamber.Collides(blownRock))
                {
                    rock = blownRock;
                }

                Rock fallenRock = rock.Translate(0, -1);

                if (chamber.Collides(fallenRock))
                {
                    chamber.Add(rock);
                    height = Math.Max(height, rock.Top + 1);
                    break;
                }

                rock = fallenRock;
            }
        }

        return height.ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }
    
    private record RockStructure(string Name, IReadOnlyList<Point> Points) : IEnumerable<Point>
    {
        public static readonly RockStructure Minus = new(
            "Minus",
            new[]
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(2, 0),
                new Point(3, 0)
            });

        public static readonly RockStructure Plus = new(
            "Plus",
            new[]
            {
                new Point(1, 0),
                new Point(0, 1),
                // new Coordinate(1, 1), TODO
                new Point(2, 1),
                new Point(1, 2)
            });

        public static readonly RockStructure Corner = new(
            "Corner",
            new[]
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(2, 0),
                new Point(2, 1),
                new Point(2, 2)
            });

        public static readonly RockStructure Tall = new(
            "Tall",
            new[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(0, 2),
                new Point(0, 3)
            });

        public static readonly RockStructure Square = new(
            "Square",
            new[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 0),
                new Point(1, 1)
            });

        public readonly int Height = Points.Max(point => point.Y) + 1;

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator()
        {
            return Points.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private record Chamber(int Width, HashSet<Point> RockFormations)
    {
        public void Add(Rock rock)
        {
            foreach (Point point in rock.Structure)
            {
                RockFormations.Add(point.Translate(rock.Offset));
            }
        }

        public bool Collides(Rock rock)
        {
            foreach (Point point in rock.Structure)
            {
                Point offsetPoint = point.Translate(rock.Offset);

                if (offsetPoint.X < 0 | offsetPoint.X > Width - 1 | offsetPoint.Y < 0)
                {
                    return true;
                }

                if (RockFormations.Contains(offsetPoint))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private readonly record struct Rock(RockStructure Structure, Point Offset)
    {
        public int Top => Structure.Height + Offset.Y - 1;

        public Rock Translate(int x, int y)
        {
            return new Rock(Structure, Offset.Translate(x, y));
        }

        public Rock Translate(Direction direction)
        {
            return new Rock(Structure, Offset.Translate(direction));
        }
    }

    private readonly record struct Point(int X, int Y)
    {
        public Point Translate(int x, int y)
        {
            return new Point(X + x, Y + y);
        }

        public Point Translate(Point by)
        {
            return Translate(by.X, by.Y);
        }

        public Point Translate(Direction direction)
        {
            return direction switch
            {
                Direction.Left => new Point(X - 1, Y),
                Direction.Right => new Point(X + 1, Y),
                _ => throw new ArgumentException($"Not a valid direction: {direction}")
            };
        }
    }

    private enum Direction
    {
        Left,
        Right
    }
}