using System.Data.SqlTypes;
using System.Net.Sockets;
using System.Net.WebSockets;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/7
/// </summary>
[Solution(7)]
[SolutionInput("Input7.test.txt", Enabled = false)]
[SolutionInput("Input7.txt", Benchmark = true, Problem1Solution = "1581595", Problem2Solution = "1544176")]
public class Solution7Fast : Solution
{
    private readonly Dictionary<string, int> directorySizes;

    public Solution7Fast(Input input) : base(input)
    {
        directorySizes = new Dictionary<string, int>();

        string currentPath = string.Empty;
        foreach (string line in Input.Lines)
        {
            if (line == "$ cd /")
            {
                currentPath = string.Empty;
            }
            else if (line == "$ cd ..")
            {
                currentPath = Path.GetDirectoryName(currentPath)!;
            }
            else if (line.StartsWith("$ cd"))
            {
                // This must be a forward move
                currentPath = Path.Combine(currentPath, line[5..]);
            }
            else if (!line.StartsWith("$ ls"))
            {
                // This must be an output line from ls
                string[] parts = line.Split(" ");
                if (parts[0] != "dir")
                {
                    // This must be a file
                    int fileSize = int.Parse(parts[0]);
                    AddFile(currentPath, fileSize);
                }
            }
        }
    }

    private void AddFile(string path, int size)
    {
        string? cursor = path;
        while (cursor is not null)
        {
            directorySizes[cursor] = (directorySizes.TryGetValue(cursor, out int existingSize) ? existingSize : 0) + size;
            cursor = Path.GetDirectoryName(cursor);
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return directorySizes.Values.Where(size => size <= 100_000).Sum().ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        const int DiskSize = 70_000_000;
        const int UpdateSize = 30_000_000;

        int freeSpace = DiskSize - directorySizes[string.Empty];
        int spaceRequired = UpdateSize - freeSpace;

        int candidate = int.MaxValue;

        foreach (int size in directorySizes.Values)
        {
            if (size > spaceRequired & size < candidate)
            {
                candidate = size;
            }
        }

        return candidate.ToString();
    }
}