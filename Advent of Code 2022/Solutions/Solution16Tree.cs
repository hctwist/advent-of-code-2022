using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/16
/// </summary>
[Solution(16, Enabled = false)]
[SolutionInput("Input16.test.txt")]
[SolutionInput("Input16.txt", Benchmark = true)]
public class Solution16Tree : Solution
{
    private const int Minutes = 30;

    private static readonly Regex Pattern = new(@"^Valve (\w\w) has flow rate=(\d+); tunnels? leads? to valves? (.+)$");

    private readonly Dictionary<string, Valve> valves;

    private readonly List<Valve> orderedValves;

    /// <inheritdoc />
    public Solution16Tree(Input input) : base(input)
    {
        valves = new Dictionary<string, Valve>(Input.Lines.Length);
        orderedValves = new List<Valve>(Input.Lines.Length);

        foreach (string line in Input.Lines)
        {
            GroupCollection groups = Pattern.Match(line).Groups;

            string name = groups[1].Value;
            int flowRate = int.Parse(groups[2].Value);
            string[] connectedValves = groups[3].Value.Split(", ");

            Valve valve = new(name, flowRate, connectedValves);
            valves.Add(name, valve);
            orderedValves.Add(valve);
        }

        orderedValves.Sort(Comparer<Valve>.Create((v1, v2) => -v1.FlowRate.CompareTo(v2.FlowRate)));
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        PriorityQueue<State, int> runningStates = new();
        runningStates.Enqueue(new State("AA", 1, 0, ImmutableHashSet<string>.Empty), 0);

        int bestFlow = 0;

        while (runningStates.Count > 0)
        {
            State state = runningStates.Dequeue();

            if (state.Minute > Minutes)
            {
                if (state.EventualFlow > bestFlow)
                {
                    bestFlow = Math.Max(bestFlow, state.EventualFlow);
                    Console.WriteLine(bestFlow);
                }

                continue;
            }

            if (GetPotential(state) <= bestFlow)
            {
                continue;
            }

            // Open the current valve
            if (!state.OpenValves.Contains(state.Valve) && valves[state.Valve].FlowRate > 0)
            {
                int eventualFlow = state.EventualFlow + GetEventualFlow(valves[state.Valve].FlowRate, state.Minute);
                runningStates.Enqueue(state with
                {
                    Minute = state.Minute + 1,
                    OpenValves = state.OpenValves.Add(state.Valve),
                    EventualFlow = eventualFlow
                }, -eventualFlow);
            }

            // Move to connected valves
            foreach (string connectedValve in valves[state.Valve].ConnectedValves)
            {
                runningStates.Enqueue(state with
                {
                    Valve = connectedValve,
                    Minute = state.Minute + 1
                }, -state.EventualFlow);
            }
        }

        return bestFlow.ToString();
    }

    private int GetPotential(State state)
    {
        int maximumNumberOfValvesThatCanBeOpened = (Minutes - state.Minute) / 2 + 1;

        int i = 0;
        int sum = state.EventualFlow;
        foreach (Valve valve in orderedValves
                     .Where(valve => !state.OpenValves.Contains(valve.Name))
                     .Take(maximumNumberOfValvesThatCanBeOpened))
        {
            sum += GetEventualFlow(valve.FlowRate, state.Minute + i * 2);
        }

        return sum;
    }

    private int GetEventualFlow(int flowRate, int minute)
    {
        return flowRate * (Minutes - minute);
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }

    private class Valve
    {
        public readonly string Name;

        public readonly int FlowRate;

        public readonly string[] ConnectedValves;

        public Valve(string name, int flowRate, string[] connectedValves)
        {
            Name = name;
            FlowRate = flowRate;
            ConnectedValves = connectedValves;
        }
    }

    private record State(string Valve, int Minute, int EventualFlow, ImmutableHashSet<string> OpenValves)
    {
        public bool IsEqualOrSuperior(State other)
        {
            return EventualFlow >= other.EventualFlow && OpenValves.IsSubsetOf(other.OpenValves);
        }

        public override string ToString()
        {
            return
                $"Valve = {Valve}, Minute = {Minute}, Eventual flow = {EventualFlow}, Open valves = {string.Join(", ", OpenValves)}";
        }
    }
}