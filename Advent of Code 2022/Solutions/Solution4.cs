using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/4
/// </summary>
[Solution(4)]
[SolutionInput("Input4.test.txt")]
[SolutionInput("Input4.txt", Benchmark = true)]
public class Solution4 : Solution
{
    private readonly List<(SectionRange, SectionRange)> sectionAssignments;

    public Solution4(Input input) : base(input)
    {
        sectionAssignments = new List<(SectionRange, SectionRange)>();
        
        foreach (string line in Input.Lines)
        {
            string[] pair = line.Split(",");
            if (pair.Length != 2)
            {
                throw new Exception($"Found a pair with {pair.Length} ranges");
            }

            sectionAssignments.Add((SectionRange.Parse(pair[0]), SectionRange.Parse(pair[1])));
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return sectionAssignments
            .Count(assignment => assignment.Item1.Contains(assignment.Item2) || assignment.Item2.Contains(assignment.Item1))
            .ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return sectionAssignments
            .Count(assignment => assignment.Item1.Overlaps(assignment.Item2) || assignment.Item2.Overlaps(assignment.Item1))
            .ToString();
    }

    private record SectionRange(int Start, int End)
    {
        public static SectionRange Parse(string range)
        {
            string[] split = range.Split("-");

            if (split.Length != 2 ||
                !int.TryParse(split[0], out int start) ||
                !int.TryParse(split[1], out int end))
            {
                throw new Exception($"Could not parse range {range}");
            }

            return new SectionRange(start, end);
        }

        public bool Contains(SectionRange other)
        {
            return Contains(other.Start) && Contains(other.End);
        }

        public bool Overlaps(SectionRange other)
        {
            return Contains(other.Start) || Contains(other.End);
        }

        private bool Contains(int Section)
        {
            return Start <= Section && Section <= End;
        }
    }
}