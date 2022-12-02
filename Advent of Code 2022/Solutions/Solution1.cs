using AdventOfCode.Framework;

namespace Advent_of_Code_2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/1
/// </summary>
[Solution(1, "Input/Input1.txt")]
public class Solution1 : Solution
{
    private readonly List<List<int>> calories;

    public Solution1(Input input) : base(input)
    {
        calories = new List<List<int>>();
        
        List<int> current = new();

        foreach (string line in Input.Lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                calories.Add(current);
                current = new List<int>();
            }
            else
            {
                current.Add(int.Parse(line));
            }
        }
    }

    /// <inheritdoc />
    protected override string Problem1()
    {
        return calories.Select(c => c.Sum())
            .Max()
            .ToString();
    }

    /// <inheritdoc />
    protected override string Problem2()
    {
        return calories.Select(c => c.Sum())
            .OrderByDescending(x => x)
            .Take(3)
            .Sum()
            .ToString();
    }
}