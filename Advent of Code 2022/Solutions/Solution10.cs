using System.Text;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/10
/// </summary>
[Solution(10)]
[SolutionInput("Input10.test.txt", Enabled = false)]
[SolutionInput("Input10.txt", Benchmark = true, Problem1Solution = "14040")]
public class Solution10 : Solution
{
    private readonly List<int> registerHistory;

    public Solution10(Input input) : base(input)
    {
        registerHistory = new List<int>(Input.Lines.Length) { 1 };
        
        foreach (string line in Input.Lines)
        {
            if (line == "noop")
            {
                registerHistory.Add(registerHistory[^1]);
            }
            else
            {
                registerHistory.Add(registerHistory[^1]);
                int addX = int.Parse(line[5..]);
                registerHistory.Add(registerHistory[^1] + addX);
            }
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        int signalStrength = 0;

        for (int i = 0; i < 6; i++)
        {
            int cycle = 20 + i * 40;
            signalStrength += cycle * registerHistory[cycle - 1];
        }

        return signalStrength.ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        StringBuilder builder = new(260);
        
        for (int i = 0; i < registerHistory.Count - 1; i++)
        {
            int x = i % 40;
            builder.Append(Math.Abs(x - registerHistory[i]) <= 1 ? '@' : ' ');

            if (x == 39 & i < registerHistory.Count - 2)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }
}