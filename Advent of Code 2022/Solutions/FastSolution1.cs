using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/1
/// </summary>
[Solution(1)]
[SolutionInput("Input1.txt")]
public class FastSolution1 : Solution
{
    public FastSolution1(Input input) : base(input)
    {
    }

    /// <inheritdoc />
    protected override string Problem1()
    {
        int max = 0;
        int sum = 0;
        
        foreach (string line in Input.Lines)
        {
            if (line.Length == 0)
            {
                if (sum > max)
                {
                    max = sum;
                }
                sum = 0;
            }
            else
            {
                sum += int.Parse(line);
            }
        }
        
        return max.ToString();
    }

    /// <inheritdoc />
    protected override string Problem2()
    {
        int[] maxCandidates = { 0, 0, 0 };
        int sum = 0;
        
        foreach (string line in Input.Lines)
        {
            if (line.Length == 0)
            {
                if (sum > maxCandidates[0])
                {
                    if (sum > maxCandidates[2])
                    {
                        maxCandidates[0] = maxCandidates[1];
                        maxCandidates[1] = maxCandidates[2];
                        maxCandidates[2] = sum;
                    }
                    else if (sum > maxCandidates[1])
                    {
                        maxCandidates[0] = maxCandidates[1];
                        maxCandidates[1] = sum;
                    }
                    else if (sum > maxCandidates[0])
                    {
                        maxCandidates[0] = sum;
                    }
                }

                sum = 0;
            }
            else
            {
                sum += int.Parse(line);
            }
        }

        return (maxCandidates[0] + maxCandidates[1] + maxCandidates[2]).ToString();
    }
}