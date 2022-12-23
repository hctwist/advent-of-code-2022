using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/11
/// </summary>
[Solution(11)]
[SolutionInput("Input11.test.txt", Enabled = false)]
[SolutionInput("Input11.txt", Benchmark = true, Problem1Solution = "61005", Problem2Solution = "20567144694")]
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
            List<long> startingItems = startingItemsMatch.Groups[1]
                .Value
                .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToList();

            Match operationMatch = OperationPattern.Match(Input.Lines[i + 2]);
            Func<int, int> leftOperandFactory = ParseOperand(operationMatch.Groups[1].Value);
            Func<int, int> rightOperandFactory = ParseOperand(operationMatch.Groups[3].Value);
            Func<long, long, long> binaryOperator = ParseBinaryOperator(operationMatch.Groups[2].Value);
            long Operation(int worryLevel) => binaryOperator(leftOperandFactory(worryLevel), rightOperandFactory(worryLevel));

            Match testMatch = TestPattern.Match(Input.Lines[i + 3]);
            int testDivisor = int.Parse(testMatch.Groups[1].Value);
            Match testIfTrue = TestIfTruePattern.Match(Input.Lines[i + 4]);
            int testIfTrueMonkey = int.Parse(testIfTrue.Groups[1].Value);
            Match testIfFalse = TestIfFalsePattern.Match(Input.Lines[i + 5]);
            int testIfFalseMonkey = int.Parse(testIfFalse.Groups[1].Value);

            monkeys.Add(new Monkey(startingItems, Operation, new Test(testDivisor, testIfTrueMonkey, testIfFalseMonkey)));
        }
    }

    private static Func<long, long, long> ParseBinaryOperator(string operation)
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
            return _ => operandValue;
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
        int t = monkeys.Aggregate(1, (a, b) => a * b.Test.Divisor);
        return GetMonkeyBusiness(10_000, x => x % t).ToString();
    }

    private long GetMonkeyBusiness(int rounds, Func<long, long> worryAdjustment)
    {
        List<long> inspections = monkeys.Select(_ => 0L).ToList();

        for (int round = 0; round < rounds; round++)
        {
            for (int m = 0; m < monkeys.Count; m++)
            {
                Monkey monkey = monkeys[m];

                foreach (int item in monkey.Items)
                {
                    long adjustedItem = monkey.Operation(item);
                    adjustedItem = worryAdjustment(adjustedItem);
                    int targetMonkey = adjustedItem % monkey.Test.Divisor == 0 ?
                        monkey.Test.MonkeyIfDivisible :
                        monkey.Test.MonkeyOtherwise;
                    monkeys[targetMonkey].Items.Add(adjustedItem);
                }
                inspections[m] += monkey.Items.Count;
                monkey.Items.Clear();
            }
        }

        return inspections.OrderByDescending(i => i).Take(2).Aggregate(1L, (a, b) => a * b);
    }

    private record Monkey(List<long> Items, Func<int, long> Operation, Test Test);

    private record Test(int Divisor, int MonkeyIfDivisible, int MonkeyOtherwise);
}