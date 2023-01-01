using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
    private readonly List<RockStructure> structures;

    private readonly List<Direction> directions;

    /// <inheritdoc />
    public Solution17(Input input) : base(input)
    {
        structures = new List<RockStructure>()
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
        return Solve(2_022).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return Solve(1_000_000_000_000).ToString();
    }

    private long Solve(long rockCount)
    {
        SequenceDetector detector = new(7, structures, directions);
        Chamber chamber = new(7, structures, directions);

        if (!detector.TryDetect(rockCount, out SequenceDetector.Sequence? sequence))
        {
            // If a sequence isn't detected, then just run the simulation all the way through
            for (int i = 0; i < rockCount; i++)
            {
                chamber.DropRock();
            }

            return chamber.Height;
        }

        // Run the simulation until the sequence starts
        for (long i = 0; i < sequence.Start; i++)
        {
            chamber.DropRock();
        }

        int heightBeforeSequence = chamber.Height;

        // Run the sequence for one whole sequence
        for (long i = 0; i < sequence.Length; i++)
        {
            chamber.DropRock();
        }

        int heightAfterSequence = chamber.Height;

        long sequenceCount = (rockCount - sequence.Start) / sequence.Length;
        long remaining = rockCount - sequence.Start - sequenceCount * sequence.Length;

        // Run the simulation for the remaining rocks after all full sequences
        for (long i = 0; i < remaining; i++)
        {
            chamber.DropRock();
        }

        int heightAfterRemaining = chamber.Height;

        return heightBeforeSequence +
            (heightAfterSequence - heightBeforeSequence) * sequenceCount +
            (heightAfterRemaining - heightAfterSequence);
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

    private class Chamber
    {
        public int Height { get; private set; }

        private readonly int width;

        private readonly List<RockStructure> structures;

        private readonly List<Direction> directions;

        private readonly HashSet<Point> formations;

        private int nextStructure;

        private int nextDirection;

        public Chamber(int width, List<RockStructure> structures, List<Direction> directions)
        {
            Height = 0;

            this.width = width;
            this.structures = structures;
            this.directions = directions;

            formations = new HashSet<Point>();
            nextStructure = 0;
            nextDirection = 0;
        }

        public int NextStructure => nextStructure % structures.Count;

        public int NextDirection => nextDirection % directions.Count;

        public void DropRock()
        {
            Rock rock = new(structures[nextStructure++ % structures.Count], new Point(2, Height + 3));

            while (rock.Offset.Y >= 0)
            {
                Direction jetDirection = directions[nextDirection++ % directions.Count];
                Rock blownRock = rock.Translate(jetDirection);

                if (!Collides(blownRock))
                {
                    rock = blownRock;
                }

                Rock fallenRock = rock.Translate(0, -1);

                if (Collides(fallenRock))
                {
                    Add(rock);
                    Height = Math.Max(Height, rock.Top + 1);
                    break;
                }

                rock = fallenRock;
            }
        }

        private void Add(Rock rock)
        {
            foreach (Point point in rock.Structure)
            {
                formations.Add(point.Translate(rock.Offset));
            }
        }

        private bool Collides(Rock rock)
        {
            foreach (Point point in rock.Structure)
            {
                Point offsetPoint = point.Translate(rock.Offset);

                if (offsetPoint.X < 0 | offsetPoint.X > width - 1 | offsetPoint.Y < 0)
                {
                    return true;
                }

                if (formations.Contains(offsetPoint))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder builder = new();

            for (int i = Height - 1; i >= 0; i--)
            {
                for (int j = 0; j < width; j++)
                {
                    builder.Append(formations.Contains(new Point(j, i)) ? '#' : ' ');
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }

    private class SequenceDetector
    {
        private readonly int width;

        private readonly List<RockStructure> structures;

        private readonly List<Direction> directions;

        public SequenceDetector(int width, List<RockStructure> structures, List<Direction> directions)
        {
            this.width = width;
            this.structures = structures;
            this.directions = directions;
        }

        public bool TryDetect(long maxIterations, [NotNullWhen(true)] out Sequence? sequence)
        {
            Chamber chamber = new(width, structures, directions);

            Dictionary<SequenceIdentifier, SequenceInformation> candidateSequences = new();

            for (long i = 0; i < maxIterations; i++)
            {
                SequenceIdentifier identifier = new(chamber.NextStructure, chamber.NextDirection);

                if (candidateSequences.TryGetValue(identifier, out SequenceInformation? information))
                {
                    if (information.FirstOccurence)
                    {
                        candidateSequences[identifier] = new SequenceInformation(i, false);
                    }
                    else
                    {
                        // Make sure only a sequence which has already occured is considered valid
                        sequence = new Sequence(i, i - information.Index);
                        return true;
                    }
                }
                else
                {
                    candidateSequences.Add(identifier, new SequenceInformation(i, true));
                }

                chamber.DropRock();
            }

            sequence = default;
            return false;
        }

        public record Sequence(long Start, long Length);

        private record SequenceIdentifier(int NextStructure, int NextDirection);

        private record SequenceInformation(long Index, bool FirstOccurence);
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