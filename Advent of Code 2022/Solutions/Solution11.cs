using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/11
/// </summary>
[Solution(11)]
[SolutionInput("Input11.test.txt")]
[SolutionInput("Input11.txt", Benchmark = true)]
public class Solution11 : Solution
{
    private static readonly Regex StartingItemsPattern = new(@"^\s*Starting items: (.*)$");
    private static readonly Regex OperationPattern = new(@"^\s*Operation: new = (.+) ([+*]) (.+)$");
    private static readonly Regex TestPattern = new(@"^\s*Test: divisible by (\d+)$");
    private static readonly Regex TestIfTruePattern = new(@"^\s*If true: throw to monkey (\d+)$");
    private static readonly Regex TestIfFalsePattern = new(@"^\s*If false: throw to monkey (\d+)$");

    private readonly List<Monkey> monkeys;

    public Solution11(Input input) : base(input)
    {
        monkeys = new List<Monkey>();

        for (int i = 0; i < Input.Lines.Length; i += 7)
        {
            Match startingItemsMatch = StartingItemsPattern.Match(Input.Lines[i + 1]);
            List<int> startingItems = startingItemsMatch.Groups[1]
                .Value
                .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            Match operationMatch = OperationPattern.Match(Input.Lines[i + 2]);
            Func<int, int> leftOperandFactory = ParseOperand(operationMatch.Groups[1].Value);
            Func<int, int> rightOperandFactory = ParseOperand(operationMatch.Groups[3].Value);
            Func<int, int, int> binaryOperator = ParseBinaryOperator(operationMatch.Groups[2].Value);
            int Operation(int worryLevel) => binaryOperator(leftOperandFactory(worryLevel), rightOperandFactory(worryLevel));

            Match testMatch = TestPattern.Match(Input.Lines[i + 3]);
            int testDivisor = int.Parse(testMatch.Groups[1].Value);
            Match testIfTrue = TestIfTruePattern.Match(Input.Lines[i + 4]);
            int testIfTrueMonkey = int.Parse(testIfTrue.Groups[1].Value);
            Match testIfFalse = TestIfFalsePattern.Match(Input.Lines[i + 5]);
            int testIfFalseMonkey = int.Parse(testIfFalse.Groups[1].Value);
            int GetMonkeyToThrowTo(int worryLevel) => worryLevel % testDivisor == 0 ? testIfTrueMonkey : testIfFalseMonkey;

            monkeys.Add(new Monkey(startingItems, Operation, GetMonkeyToThrowTo));
        }
    }

    private static Func<int, int, int> ParseBinaryOperator(string operation)
    {
        return operation switch
        {
            "*" => (a, b) => a * b,
            "+" => (a, b) => a + b,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private static Func<int, int> ParseOperand(string operand)
    {
        if (operand == "old")
        {
            return x => x;
        }
        else
        {
            int operandValue = int.Parse(operand);
            return x => operandValue;
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        return GetMonkeyBusiness(20, x => x / 3).ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return GetMonkeyBusiness(10_000, x => x).ToString();
    }

    private int GetMonkeyBusiness(int rounds, Func<int, int> worryAdjustment)
    {
        List<int> inspections = monkeys.Select(_ => 0).ToList();

        for (int round = 0; round < rounds; round++)
        {
            for (int m = 0; m < monkeys.Count; m++)
            {
                Monkey monkey = monkeys[m];

                foreach (int item in monkey.Items)
                {
                    int adjustedItem = monkey.Operation(item);
                    adjustedItem = worryAdjustment(adjustedItem);
                    int targetMonkey = monkey.GetMonkeyToThrowTo(adjustedItem);
                    monkeys[targetMonkey].Items.Add(adjustedItem);
                }
                inspections[m] += monkey.Items.Count;
                monkey.Items.Clear();
            }
        }

        return inspections.OrderByDescending(i => i).Take(2).Aggregate(1, (a, b) => a * b);
    }

    private record Monkey(List<int> Items, Func<int, int> Operation, Func<int, int> GetMonkeyToThrowTo);
}