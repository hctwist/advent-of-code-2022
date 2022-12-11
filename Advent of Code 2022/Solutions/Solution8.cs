using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/8
/// </summary>
[Solution(8)]
[SolutionInput("Input8.test.txt", Enabled = false)]
[SolutionInput("Input8.txt", Benchmark = true, Problem1Solution = "1543", Problem2Solution = "595080")]
public class Solution8 : Solution
{
    private static readonly Dictionary<Direction, Point> Steps = new()
    {
        { Direction.Down, new Point(0, 1) },
        { Direction.Left, new Point(-1, 0) },
        { Direction.Up, new Point(0, -1) },
        { Direction.Right, new Point(1, 0) }
    };

    private readonly Tree[,] trees;

    public Solution8(Input input) : base(input)
    {
        trees = new Tree[Input.Lines.Length, Input.Lines[0].Length];

        for (int row = 0; row < Input.Lines.Length; row++)
        {
            for (int col = 0; col < Input.Lines[0].Length; col++)
            {
                trees[row, col] = new Tree(int.Parse(Input.Lines[row][col].ToString()));
            }
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        foreach (Direction direction in Enum.GetValues<Direction>())
        {
            UpdateVisibilityFromEdges(direction);
        }

        int count = 0;
        for (int row = 0; row < trees.GetLength(0); row++)
        {
            for (int col = 0; col < trees.GetLength(1); col++)
            {
                if (trees[row, col].VisibleFromEdge)
                {
                    count++;
                }
            }
        }

        return count.ToString();
    }

    private void UpdateVisibilityFromEdges(Direction direction)
    {
        // The starting point of the scan
        Point start = direction switch
        {
            Direction.Down => new Point(0, 0),
            Direction.Left => new Point(trees.GetUpperBound(1), 0),
            Direction.Up => new Point(trees.GetUpperBound(1), trees.GetUpperBound(0)),
            Direction.Right => new Point(0, trees.GetUpperBound(0)),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        Point step = Steps[direction];

        // A unit movement in the direction perpendicular to that specified
        Point scanDirection = new(step.Y, -step.X);

        Point cursor = start;
        while (WithinBounds(cursor))
        {
            UpdateVisibilityFrom(cursor, step);
            cursor = cursor.Move(scanDirection);
        }
    }

    private void UpdateVisibilityFrom(Point start, Point step)
    {
        Point cursor = start;
        int maxHeight = -1;
        while (WithinBounds(cursor))
        {
            if (trees[cursor.Y, cursor.X].Height > maxHeight)
            {
                trees[cursor.Y, cursor.X].VisibleFromEdge = true;
            }
            maxHeight = Math.Max(maxHeight, trees[cursor.Y, cursor.X].Height);
            cursor = cursor.Move(step);
        }
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        int bestScore = -1;
        
        for (int row = 0; row < trees.GetLength(0); row++)
        {
            for (int col = 0; col < trees.GetLength(1); col++)
            {
                int score = GetScenicScore(new Point(col, row));
                bestScore = Math.Max(bestScore, score);
            }
        }

        return bestScore.ToString();
    }

    private int GetScenicScore(Point point)
    {
        int score = 1;
        foreach (Direction direction in Enum.GetValues<Direction>())
        {
            score *= GetScenicScore(point, direction);
        }
        return score;
    }

    private int GetScenicScore(Point point, Direction direction)
    {
        Point step = Steps[direction];
        Point cursor = point.Move(step);

        int score = 0;
        
        while (WithinBounds(cursor))
        {
            score++;
            
            if (trees[cursor.X, cursor.Y].Height >= trees[point.X, point.Y].Height)
            {
                break;
            }

            cursor = cursor.Move(step);
        }

        return score;
    }
    
    private bool WithinBounds(Point point)
    {
        return (0 <= point.X & point.X <= trees.GetUpperBound(1)) & (0 <= point.Y & point.Y <= trees.GetUpperBound(0));
    }

    private class Tree
    {
        public readonly int Height;

        public bool VisibleFromEdge;

        public Tree(int height)
        {
            Height = height;

            VisibleFromEdge = false;
        }
    }

    private readonly record struct Point(int X, int Y)
    {
        public Point Move(Point move)
        {
            return new Point(X + move.X, Y + move.Y);
        }
    }

    private enum Direction
    {
        Down,
        Left,
        Up,
        Right
    }
}