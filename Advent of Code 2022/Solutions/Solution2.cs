using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/2
/// </summary>
[Solution(2)]
[SolutionInput("Input2.test.txt", Enabled = false)]
[SolutionInput("Input2.txt", Benchmark = true, Problem1Solution = "13484", Problem2Solution = "13433")]
public class Solution2 : Solution
{
    private static readonly Dictionary<string, Move> MoveLookup = new()
    {
        { "A", Move.Rock },
        { "B", Move.Paper },
        { "C", Move.Scissors }
    };

    private static readonly Dictionary<string, Response> ResponseLookup = new()
    {
        { "X", new Response(Move.Rock, Outcome.Lose) },
        { "Y", new Response(Move.Paper, Outcome.Draw) },
        { "Z", new Response(Move.Scissors, Outcome.Win) }
    };

    private static readonly Dictionary<Move, Move> WinningMoves = new()
    {
        { Move.Rock, Move.Paper },
        { Move.Paper, Move.Scissors },
        { Move.Scissors, Move.Rock }
    };

    private static readonly Dictionary<Move, Move> LosingMoves = WinningMoves
        .ToDictionary(m => m.Value, m => m.Key);

    private static readonly Dictionary<Move, int> MoveScores = new()
    {
        { Move.Rock, 1 },
        { Move.Paper, 2 },
        { Move.Scissors, 3 }
    };
    
    private static readonly Dictionary<Outcome, int> OutcomeScores = new()
    {
        { Outcome.Lose, 0 },
        { Outcome.Draw, 3 },
        { Outcome.Win, 6 }
    };

    private readonly List<Strategy> guide;

    public Solution2(Input input) : base(input)
    {
        guide = new List<Strategy>(input.Lines.Length);

        foreach (string line in input.Lines)
        {
            string[] split = line.Split(' ');
            guide.Add(new Strategy(MoveLookup[split[0]], ResponseLookup[split[1]]));
        }
    }

    /// <inheritdoc />
    protected override string Problem1()
    {
        int score = 0;

        foreach (Strategy strategy in guide)
        {
            Outcome outcome;
            if (strategy.Move == strategy.Response.Move)
            {
                outcome = Outcome.Draw;
            }
            else if (strategy.Response.Move == WinningMoves[strategy.Move])
            {
                outcome = Outcome.Win;
            }
            else
            {
                outcome = Outcome.Lose;
            }

            score += OutcomeScores[outcome];
            score += MoveScores[strategy.Response.Move];
        }

        return score.ToString();
    }

    /// <inheritdoc />
    protected override string Problem2()
    {
        int score = 0;

        foreach (Strategy strategy in guide)
        {
            Move response = strategy.Response.Outcome switch
            {
                Outcome.Lose => LosingMoves[strategy.Move],
                Outcome.Draw => strategy.Move,
                Outcome.Win => WinningMoves[strategy.Move],
                _ => throw new ArgumentOutOfRangeException()
            };
            
            score += OutcomeScores[strategy.Response.Outcome];
            score += MoveScores[response];
        }

        return score.ToString();
    }

    private record Strategy(Move Move, Response Response);

    private record Response(Move Move, Outcome Outcome);

    private enum Move
    {
        Rock,
        Paper,
        Scissors
    }

    private enum Outcome
    {
        Lose,
        Draw,
        Win
    }
}