using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/5
/// </summary>
[Solution(5)]
[SolutionInput("Input5.test.txt")]
[SolutionInput("Input5.txt")]
public class Solution5 : Solution
{
    private static readonly Regex InstructionPattern = new(@"^move (\d+) from (\d+) to (\d+)$");

    private readonly Crates crates;

    private readonly List<Instruction> instructions;

    public Solution5(Input input) : base(input)
    {
        int divider = Array.IndexOf(input.Lines, string.Empty);

        crates = new Crates();

        for (int i = divider - 2; i >= 0; i--)
        {
            int numberOfCrates = (Input.Lines[i].Length + 1) / 4;
            for (int j = 0; j < numberOfCrates; j++)
            {
                char crate = Input.Lines[i][4 * j + 1];
                if (crate != ' ')
                {
                    crates.Add(j, crate);
                }
            }
        }

        instructions = new List<Instruction>();

        for (int i = divider + 1; i < Input.Lines.Length; i++)
        {
            Match match = InstructionPattern.Match(Input.Lines[i]);

            // Normalise to a 0 index
            instructions.Add(
                new Instruction(
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value) - 1,
                    int.Parse(match.Groups[3].Value) - 1));
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        foreach (Instruction instruction in instructions)
        {
            for (int i = 0; i < instruction.Count; i++)
            {
                crates.Move(instruction.From, instruction.To);
            }
        }

        return string.Join(null, crates.Stacks.Select(s => s.Peek()));
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        foreach (Instruction instruction in instructions)
        {
            crates.Move(instruction.Count, instruction.From, instruction.To);
        }

        return string.Join(null, crates.Stacks.Select(s => s.Peek()));
    }

    private class Crates
    {
        public readonly List<Stack<char>> Stacks;

        public Crates()
        {
            Stacks = new List<Stack<char>>();
        }

        public void Add(int index, char crate)
        {
            for (int i = 0; i < index - Stacks.Count + 1; i++)
            {
                Stacks.Add(new Stack<char>());
            }

            Stacks[index].Push(crate);
        }

        public void Move(int from, int to)
        {
            Stacks[to].Push(Stacks[from].Pop());
        }

        public void Move(int count, int from, int to)
        {
            List<char> buffer = new(count);
            for (int i = 0; i < count; i++)
            {
                buffer.Add(Stacks[from].Pop());
            }
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                Stacks[to].Push(buffer[i]);
            }
        }
    }

    private record Instruction(int Count, int From, int To);
}