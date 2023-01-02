using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/18
/// </summary>
[Solution(18)]
[SolutionInput("Input18.test.txt")]
[SolutionInput("Input18.txt", Benchmark = true)]
public class Solution18 : Solution
{
    private readonly List<Point> cubes;

    /// <inheritdoc />
    public Solution18(Input input) : base(input)
    {
        cubes = new List<Point>();

        foreach (string line in input.Lines)
        {
            string[] split = line.Split(',');
            cubes.Add(
                new Point(
                    int.Parse(split[0]),
                    int.Parse(split[1]),
                    int.Parse(split[2])));
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return GetSurfaceArea(new ApproximateScanner()).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return GetSurfaceArea(new AdvancedScanner()).ToString();
    }

    private int GetSurfaceArea(IScanner scanner)
    {
        foreach (Point cube in cubes)
        {
            scanner.Scan(cube);
        }

        return scanner.GetSurfaceArea();
    }

    private interface IScanner
    {
        public void Scan(Point point);

        public int GetSurfaceArea();
    }

    private class ApproximateScanner : IScanner
    {
        private readonly HashSet<Face> nonTouchingFaces;

        private readonly HashSet<Face> touchingFaces;

        public ApproximateScanner()
        {
            nonTouchingFaces = new HashSet<Face>();
            touchingFaces = new HashSet<Face>();
        }

        public void Scan(Point point)
        {
            AddFace(new Face(point, FaceDirection.X));
            AddFace(new Face(point with { X = point.X - 1 }, FaceDirection.X));
            AddFace(new Face(point, FaceDirection.Y));
            AddFace(new Face(point with { Y = point.Y - 1 }, FaceDirection.Y));
            AddFace(new Face(point, FaceDirection.Z));
            AddFace(new Face(point with { Z = point.Z - 1 }, FaceDirection.Z));
        }

        /// <inheritdoc />
        public int GetSurfaceArea()
        {
            return nonTouchingFaces.Count;
        }

        private void AddFace(Face face)
        {
            if (touchingFaces.Contains(face))
            {
                // No-op
            }
            else if (nonTouchingFaces.Contains(face))
            {
                nonTouchingFaces.Remove(face);
                touchingFaces.Add(face);
            }
            else
            {
                nonTouchingFaces.Add(face);
            }
        }

        private record Face(Point Point, FaceDirection Direction);

        private enum FaceDirection
        {
            X,
            Y,
            Z
        }
    }

    private class AdvancedScanner : IScanner
    {
        private readonly HashSet<Point> points;

        private int minX;
        private int maxX;
        private int minY;
        private int maxY;
        private int minZ;
        private int maxZ;

        public AdvancedScanner()
        {
            points = new HashSet<Point>();
        }

        /// <inheritdoc />
        public void Scan(Point point)
        {
            points.Add(point);

            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);
            minZ = Math.Min(minZ, point.Z);
            maxZ = Math.Max(maxZ, point.Z);
        }

        /// <inheritdoc />
        public int GetSurfaceArea()
        {
            Point startPoint = new(minX - 1, minY - 1, minZ - 1);
            
            int surfaceArea = 0;

            Queue<Point> pointsQueue = new();
            HashSet<Point> previouslyQueuedPoints = new();

            pointsQueue.Enqueue(startPoint);
            previouslyQueuedPoints.Add(startPoint);

            void Consider(Point point)
            {
                if (!IsWithinBounds(point))
                {
                    return;
                }
                
                if (points.Contains(point))
                {
                    surfaceArea++;
                }
                else if (!previouslyQueuedPoints.Contains(point))
                {
                    pointsQueue.Enqueue(point);
                    previouslyQueuedPoints.Add(point);
                }
            }

            while (pointsQueue.Count > 0)
            {
                Point point = pointsQueue.Dequeue();
                
                Consider(point with { X = point.X + 1 });
                Consider(point with { X = point.X - 1 });
                Consider(point with { Y = point.Y + 1 });
                Consider(point with { Y = point.Y - 1 });
                Consider(point with { Z = point.Z + 1 });
                Consider(point with { Z = point.Z - 1 });
            }

            return surfaceArea;
        }
        
        private bool IsWithinBounds(Point point)
        {
            return minX - 1 <= point.X && point.X <= maxX + 1 &&
                minY - 1 <= point.Y && point.Y <= maxY + 1 &&
                minZ - 1 <= point.Z && point.Z <= maxZ + 1;
        }
    }

    private record Point(int X, int Y, int Z);
}