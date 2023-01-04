using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/22
/// </summary>
[Solution(22)]
[SolutionInput("Input22.test.txt")]
[SolutionInput("Input22.txt", Benchmark = true)]
public class Solution22 : Solution
{
    private readonly List<MapRow> mapRows;

    private readonly List<IInstruction> instructions;

    public Solution22(Input input) : base(input)
    {
        mapRows = new List<MapRow>(Input.Lines.Length);

        foreach (string line in Input.Lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            int rowStartIndex = line.IndexOfAny(new[] { '.', '#' });

            if (rowStartIndex == -1)
            {
                throw new Exception($"Could not parse row: {line}");
            }

            List<Tile> rowTiles = line.Skip(rowStartIndex).Select(GetTile).ToList();
            mapRows.Add(new MapRow(rowStartIndex, rowTiles));
        }

        string path = Input.Lines[^1];
        instructions = new List<IInstruction>();

        int countBuffer = 0;
        foreach (char c in path)
        {
            if (char.IsDigit(c))
            {
                countBuffer *= 10;
                countBuffer += c - '0';
            }
            else
            {
                instructions.Add(new MoveInstruction(countBuffer));
                countBuffer = 0;

                instructions.Add(new TurnInstruction(GetTurn(c)));
            }
        }
        instructions.Add(new MoveInstruction(countBuffer));
    }

    private static Tile GetTile(char c)
    {
        return c switch
        {
            '.' => Tile.Open,
            '#' => Tile.Wall,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }

    private static TurnInstruction.TurnDirection GetTurn(char c)
    {
        return c switch
        {
            'R' => TurnInstruction.TurnDirection.Clockwise,
            'L' => TurnInstruction.TurnDirection.Counterclockwise,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        Position initialPosition = new(mapRows[0].GetStartX(), 0, Position.Direction.Right);
        Map map = new(mapRows, initialPosition);

        foreach (IInstruction instruction in instructions)
        {
            instruction.Execute(map);
        }

        int row = map.Position.Y + 1;
        int column = map.Position.X + 1;
        int facing = map.Position.Facing switch
        {
            Position.Direction.Left => 2,
            Position.Direction.Up => 3,
            Position.Direction.Right => 0,
            Position.Direction.Down => 1,
            _ => throw new ArgumentOutOfRangeException()
        };

        int password = (1_000 * row) + (4 * column) + facing;
        return password.ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }

    private record MapRow(int X, IReadOnlyList<Tile> Tiles)
    {
        public int GetStartX()
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i] == Tile.Open)
                {
                    return X + i;
                }
            }

            throw new Exception("Could not find a start position for the row");
        }

        public bool HasTileAt(int x)
        {
            return 0 <= x - X && x - X < Tiles.Count;
        } 

        public Tile GetTile(int x)
        {
            return Tiles[x - X];
        }
    }

    private enum Tile
    {
        Open,
        Wall
    }

    private interface IInstruction
    {
        void Execute(Map map);
    }

    private class MoveInstruction : IInstruction
    {
        private readonly int count;

        public MoveInstruction(int count)
        {
            this.count = count;
        }

        /// <inheritdoc />
        public void Execute(Map map)
        {
            switch (map.Position.Facing)
            {
                case Position.Direction.Left:
                    MoveHorizontally(map, -1);
                    break;
                case Position.Direction.Up:
                    MoveVertically(map, -1);
                    break;
                case Position.Direction.Right:
                    MoveHorizontally(map, 1);
                    break;
                case Position.Direction.Down:
                    MoveVertically(map, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MoveHorizontally(Map map, int step)
        {
            for (int i = 0; i < count; i++)
            {
                MapRow row = map.MapRows[map.Position.Y];

                int nextX = map.Position.X + step;

                if (nextX == row.X - 1)
                {
                    nextX = row.X + row.Tiles.Count - 1;
                }
                else if (nextX == row.X + row.Tiles.Count)
                {
                    nextX = row.X;
                }

                Tile tile = row.GetTile(nextX);

                if (tile == Tile.Wall)
                {
                    break;
                }

                map.Position.X = nextX;
            }
        }

        private void MoveVertically(Map map, int step)
        {
            for (int i = 0; i < count; i++)
            {
                int nextY = map.Position.Y + step;

                if (nextY == -1 ||
                    nextY == map.MapRows.Count ||
                    !map.MapRows[nextY].HasTileAt(map.Position.X) ||
                    !map.MapRows[nextY].HasTileAt(map.Position.X))
                {
                    nextY = FindLastY(map, -step);
                }

                MapRow row = map.MapRows[nextY];
                Tile tile = row.GetTile(map.Position.X);

                if (tile == Tile.Wall)
                {
                    break;
                }

                map.Position.Y = nextY;
            }
        }

        private static int FindLastY(Map map, int step)
        {
            int y = map.Position.Y + step;
            while (true)
            {
                if (y < 0 | y >= map.MapRows.Count || !map.MapRows[y].HasTileAt(map.Position.X))
                {
                    return y - step;
                }
                y += step;
            }
        }
    }

    private class TurnInstruction : IInstruction
    {
        private readonly TurnDirection turn;

        public TurnInstruction(TurnDirection turn)
        {
            this.turn = turn;
        }

        /// <inheritdoc />
        public void Execute(Map map)
        {
            int rotationDirection = turn switch
            {
                TurnDirection.Clockwise => 1,
                TurnDirection.Counterclockwise => -1,
                _ => throw new ArgumentOutOfRangeException(nameof(turn))
            };

            int directionCount = 4;
            map.Position.Facing = (Position.Direction)((((int)map.Position.Facing + rotationDirection) + directionCount) % directionCount);
        }

        public enum TurnDirection
        {
            Clockwise,
            Counterclockwise
        }
    }

    private class Position
    {
        public int X;

        public int Y;

        public Direction Facing;

        public Position(int x, int y, Direction facing)
        {
            X = x;
            Y = y;
            Facing = facing;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({X}, {Y}) ~ {Facing}";
        }

        public enum Direction
        {
            Left,
            Up,
            Right,
            Down
        }
    }

    private class Map
    {
        public readonly IReadOnlyList<MapRow> MapRows;

        public readonly Position Position;

        public Map(IReadOnlyList<MapRow> mapRows, Position position)
        {
            MapRows = mapRows;
            Position = position;
        }
    }
}