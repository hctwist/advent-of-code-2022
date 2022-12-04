using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/3
/// </summary>
[Solution(3)]
[SolutionInput("Input3.test.txt")]
[SolutionInput("Input3.txt")]
public class Solution3 : Solution
{
    private readonly List<Rucksack> rucksacks;

    public Solution3(Input input) : base(input)
    {
        rucksacks = Input.Lines.Select(line => new Rucksack(line.ToCharArray())).ToList();
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        int totalPriority = 0;

        foreach (Rucksack rucksack in rucksacks)
        {
            HashSet<char> secondCompartment = rucksack.SecondCompartment.ToHashSet();
            foreach (char item in rucksack.FirstCompartment)
            {
                if (secondCompartment.Contains(item))
                {
                    totalPriority += GetPriority(item);
                    break;
                }
            }
        }

        return totalPriority.ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        int totalPriority = 0;

        if (rucksacks.Count % 3 != 0)
        {
            throw new Exception($"Invalid number of rucksacks: {rucksacks.Count}");
        }

        for (int i = 0; i < rucksacks.Count; i += 3)
        {
            HashSet<char> secondRucksack = rucksacks[i + 1].Items.ToHashSet();
            HashSet<char> thirdRucksack = rucksacks[i + 2].Items.ToHashSet();

            foreach (char item in rucksacks[i].Items)
            {
                if (secondRucksack.Contains(item) && thirdRucksack.Contains(item))
                {
                    totalPriority += GetPriority(item);
                    break;
                }
            }
        }

        return totalPriority.ToString();
    }

    private static int GetPriority(char item)
    {
        return char.IsLower(item) ? item - 96 : item - 38;
    }

    private record Rucksack(char[] Items)
    {
        public IEnumerable<char> FirstCompartment => Items.Take(Items.Length / 2);

        public IEnumerable<char> SecondCompartment => Items.Skip(Items.Length / 2);
    }
}