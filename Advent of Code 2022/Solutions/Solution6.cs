using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/6
/// </summary>
[Solution(6)]
[SolutionInput("Input6.test.txt")]
[SolutionInput("Input6.txt", Benchmark = true)]
public class Solution6 : Solution
{
    public Solution6(Input input) : base(input)
    {
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return FindMarkerIndex(4).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return FindMarkerIndex(14).ToString();
    }

    private int FindMarkerIndex(int size)
    {
        HashSet<char> buffer = new(size);
        for (int i = size; i < Input.Raw.Length; i++)
        {
            for (int j = 0; j < size; j++)
            {
                buffer.Add(Input.Raw[i - j - 1]);
            }

            if (buffer.Count == size)
            {
                return i;
            }
            
            buffer.Clear();
        }

        throw new Exception("Couldn't find marker");
    }
}