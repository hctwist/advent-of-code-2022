using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/21
/// </summary>
[Solution(21)]
[SolutionInput("Input21.test.txt")]
[SolutionInput("Input21.txt", Benchmark = true)]
public class Solution21 : Solution
{
    private static readonly Regex IndependentMonkeyPattern = new(@"^(.+): (\d+)$");
    private static readonly Regex DependentMonkeyPattern = new(@"^(.+): (.+) ([\+-\/\*]) (.+)$");

    private readonly List<IndependentMonkey> independentMonkeys;

    private readonly List<OperatorDependentMonkey> dependentMonkeys;

    private readonly OperatorDependentMonkey root;

    public Solution21(Input input) : base(input)
    {
        independentMonkeys = new List<IndependentMonkey>(Input.Lines.Length);
        dependentMonkeys = new List<OperatorDependentMonkey>(Input.Lines.Length);

        foreach (string line in Input.Lines)
        {
            Match independentMatch = IndependentMonkeyPattern.Match(line);
            Match dependentMatch = DependentMonkeyPattern.Match(line);

            if (independentMatch.Success)
            {
                IndependentMonkey monkey = new(
                    independentMatch.Groups[1].Value,
                    long.Parse(independentMatch.Groups[2].Value));
                independentMonkeys.Add(monkey);
            }
            else if (dependentMatch.Success)
            {
                OperatorDependentMonkey monkey = new(
                    dependentMatch.Groups[1].Value,
                    GetOperator(dependentMatch.Groups[3].Value),
                    dependentMatch.Groups[2].Value,
                    dependentMatch.Groups[4].Value);
                dependentMonkeys.Add(monkey);

                if (monkey.Name == "root")
                {
                    root = monkey;
                }
            }
            else
            {
                throw new Exception($"Could not parse monkey {line}");
            }
        }

        if (root is null)
        {
            throw new Exception("Could not find root monkey");
        }
    }

    private static BijectiveOperator GetOperator(string str)
    {
        return str switch
        {
            "+" => BijectiveOperators.Addition,
            "-" => BijectiveOperators.Subtraction,
            "*" => BijectiveOperators.Multiplication,
            "/" => BijectiveOperators.Division,
            _ => throw new ArgumentOutOfRangeException(nameof(str))
        };
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        Dictionary<string, long> yelledNumbers = independentMonkeys.ToDictionary(monkey => monkey.Name, monkey => monkey.Number);
        Queue<DependentMonkey> pendingMonkeys = new(dependentMonkeys);

        while (pendingMonkeys.Count > 0)
        {
            DependentMonkey monkey = pendingMonkeys.Dequeue();

            if (!monkey.TryUpdateYelledNumbers(yelledNumbers))
            {
                pendingMonkeys.Enqueue(monkey);
            }
        }

        return yelledNumbers[root.Name].ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        const string MyName = "humn";
        
        Dictionary<string, long> yelledNumbers = independentMonkeys
            .Where(monkey => monkey.Name != MyName)
            .ToDictionary(monkey => monkey.Name, monkey => monkey.Number);
        
        Queue<DependentMonkey> pendingMonkeys = new(dependentMonkeys.Where(monkey => monkey != root));
        pendingMonkeys.Enqueue(new EqualityDependentMonkey(root.Name, root.Operand1Name, root.Operand2Name));
        
        while (pendingMonkeys.Count > 0)
        {
            DependentMonkey monkey = pendingMonkeys.Dequeue();

            if (!monkey.TryUpdateYelledNumbers(yelledNumbers))
            {
                pendingMonkeys.Enqueue(monkey);
            }
        }

        return yelledNumbers[MyName].ToString();
    }

    private abstract class Monkey
    {
        public readonly string Name;

        protected Monkey(string name)
        {
            Name = name;
        }
    }

    private class IndependentMonkey : Monkey
    {
        public readonly long Number;

        public IndependentMonkey(string name, long number) : base(name)
        {
            Number = number;
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}: {Number}";
        }
    }
    
    private abstract class DependentMonkey : Monkey
    {
        /// <inheritdoc />
        protected DependentMonkey(string name) : base(name)
        {
        }

        public abstract bool TryUpdateYelledNumbers(Dictionary<string, long> yelledNumbers);
    }

    private class OperatorDependentMonkey : DependentMonkey
    {
        private readonly BijectiveOperator op;

        public readonly string Operand1Name;

        public readonly string Operand2Name;

        public OperatorDependentMonkey(string name, BijectiveOperator op, string operand1Name, string operand2Name) : base(name)
        {
            this.op = op;
            Operand1Name = operand1Name;
            Operand2Name = operand2Name;
        }

        /// <inheritdoc />
        public override bool TryUpdateYelledNumbers(Dictionary<string, long> yelledNumbers)
        {
            bool foundOperand1 = yelledNumbers.TryGetValue(Operand1Name, out long operand1);
            bool foundOperand2 = yelledNumbers.TryGetValue(Operand2Name, out long operand2);

            if (foundOperand1 && foundOperand2)
            {
                yelledNumbers.Add(Name, op.Invoke(operand1, operand2));
                return true;
            }
            else
            {
                bool foundResult = yelledNumbers.TryGetValue(Name, out long result);

                if (foundResult && foundOperand1)
                {
                    yelledNumbers.Add(Operand2Name, op.SolveForOperand2(operand1, result));
                    return true;
                }
                else if (foundResult && foundOperand2)
                {
                    yelledNumbers.Add(Operand1Name, op.SolveForOperand1(operand2, result));
                    return true;
                }
            }

            return false;
        }
    }

    private class EqualityDependentMonkey : DependentMonkey
    {
        private readonly string operand1Name;

        private readonly string operand2Name;

        /// <inheritdoc />
        public EqualityDependentMonkey(string name, string operand1Name, string operand2Name) : base(name)
        {
            this.operand1Name = operand1Name;
            this.operand2Name = operand2Name;
        }

        /// <inheritdoc />
        public override bool TryUpdateYelledNumbers(Dictionary<string, long> yelledNumbers)
        {
            if (yelledNumbers.TryGetValue(operand1Name, out long operand1))
            {
                yelledNumbers.Add(operand2Name, operand1);
                return true;
            }
            else if (yelledNumbers.TryGetValue(operand2Name, out long operand2))
            {
                yelledNumbers.Add(operand1Name, operand2);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private abstract class BijectiveOperator
    {
        public abstract long Invoke(long operand1, long operand2);

        public abstract long SolveForOperand1(long operand2, long result);

        public abstract long SolveForOperand2(long operand1, long result);
    }

    private abstract class SimpleBijectiveOperator : BijectiveOperator
    {
        /// <inheritdoc />
        public override long SolveForOperand2(long operand1, long result)
        {
            return SolveForOperand1(operand1, result);
        }
    }

    private class AdditionOperator : SimpleBijectiveOperator
    {
        /// <inheritdoc />
        public override long Invoke(long operand1, long operand2)
        {
            return operand1 + operand2;
        }

        /// <inheritdoc />
        public override long SolveForOperand1(long operand2, long result)
        {
            return result - operand2;
        }
    }

    private class SubtractionOperator : BijectiveOperator
    {
        /// <inheritdoc />
        public override long Invoke(long operand1, long operand2)
        {
            return operand1 - operand2;
        }

        /// <inheritdoc />
        public override long SolveForOperand1(long operand2, long result)
        {
            return result + operand2;
        }

        /// <inheritdoc />
        public override long SolveForOperand2(long operand1, long result)
        {
            return operand1 - result;
        }
    }

    private class MultiplicationOperator : SimpleBijectiveOperator
    {
        /// <inheritdoc />
        public override long Invoke(long operand1, long operand2)
        {
            return operand1 * operand2;
        }

        /// <inheritdoc />
        public override long SolveForOperand1(long operand2, long result)
        {
            return result / operand2;
        }
    }

    private class DivisionOperator : BijectiveOperator
    {
        /// <inheritdoc />
        public override long Invoke(long operand1, long operand2)
        {
            return operand1 / operand2;
        }

        /// <inheritdoc />
        public override long SolveForOperand1(long operand2, long result)
        {
            return result * operand2;
        }

        /// <inheritdoc />
        public override long SolveForOperand2(long operand1, long result)
        {
            return operand1 / result;
        }
    }

    private static class BijectiveOperators
    {
        public static readonly BijectiveOperator Addition = new AdditionOperator();
        public static readonly BijectiveOperator Subtraction = new SubtractionOperator();
        public static readonly BijectiveOperator Multiplication = new MultiplicationOperator();
        public static readonly BijectiveOperator Division = new DivisionOperator();
    }
}