using System.Collections.Immutable;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/16
/// </summary>
[Solution(16)]
[SolutionInput("Input16.test.txt")]
[SolutionInput("Input16.txt", Benchmark = true)]
public class Solution16Dijkstra : Solution
{
    private const int Minutes = 30;
    private const string StartingValve = "AA";
    
    private static readonly Regex Pattern = new(@"^Valve (\w\w) has flow rate=(\d+); tunnels? leads? to valves? (.+)$");

    private readonly Dictionary<string, Valve> valves;

    private readonly List<Valve> nonZeroValves;

    private readonly Dictionary<ValveRoute, int> routeLengths;

    /// <inheritdoc />
    public Solution16Dijkstra(Input input) : base(input)
    {
        valves = new Dictionary<string, Valve>(Input.Lines.Length);
        nonZeroValves = new List<Valve>();

        foreach (string line in Input.Lines)
        {
            GroupCollection groups = Pattern.Match(line).Groups;

            string name = groups[1].Value;
            int flowRate = int.Parse(groups[2].Value);
            string[] connectedValves = groups[3].Value.Split(", ");

            Valve valve = new(name, flowRate, connectedValves);
            valves.Add(name, valve);

            if (flowRate > 0)
            {
                nonZeroValves.Add(valve);
            }
        }

        routeLengths = GetRouteLengths(valves, nonZeroValves);
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        ImmutableHashSet<Valve>.Builder closedValvesBuilder = ImmutableHashSet.CreateBuilder<Valve>();
        foreach (Valve valve in nonZeroValves)
        {
            closedValvesBuilder.Add(valve);
        }
        
        return ComputeMaximumFlow(new State(
            valves[StartingValve], 
            1,
            0,
            closedValvesBuilder.ToImmutable()))
            .ToString();
    }

    private int ComputeMaximumFlow(State state)
    {
        int bestFlow = state.EventualFlow;

        foreach (Valve nextValve in state.ClosedValves)
        {
            int nextMinute = state.Minute + routeLengths[new ValveRoute(state.Valve.Name, nextValve.Name)] + 1;

            if (nextMinute > Minutes)
            {
                continue;
            }

            State nextState = new(
                nextValve,
                nextMinute,
                state.EventualFlow + (Minutes - nextMinute + 1) * nextValve.FlowRate,
                state.ClosedValves.Remove(nextValve));
            bestFlow = Math.Max(bestFlow, ComputeMaximumFlow(nextState));
        }

        return bestFlow;
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }

    private static Dictionary<ValveRoute, int> GetRouteLengths(Dictionary<string, Valve> valves, List<Valve> nonZeroValves)
    {
        Dictionary<string, DijkstraNode> nodes = new(nonZeroValves.Count + 1);
        foreach (Valve valve in valves.Values)
        {
            nodes.Add(valve.Name, new DijkstraNode(valve));
        }

        Dictionary<ValveRoute, int> routeLengths = new(nonZeroValves.Count * nonZeroValves.Count);

        // Consider routes from AA
        foreach (Valve valve in nonZeroValves)
        {
            ValveRoute route = new(StartingValve, valve.Name);
            routeLengths.Add(route, GetRouteLength(nodes, route));
        }

        for (int i = 0; i < nonZeroValves.Count; i++)
        {
            for (int j = i + 1; j < nonZeroValves.Count; j++)
            {
                ValveRoute route = new(nonZeroValves[i].Name, nonZeroValves[j].Name);
                routeLengths.Add(route, GetRouteLength(nodes, route));
            }
        }

        List<KeyValuePair<ValveRoute, int>> mirroredRouteLengths = new();

        foreach (KeyValuePair<ValveRoute,int> length in routeLengths)
        {
            mirroredRouteLengths.Add(new KeyValuePair<ValveRoute, int>(
                new ValveRoute(length.Key.To, length.Key.From),
                length.Value));
        }

        foreach (KeyValuePair<ValveRoute, int> length in mirroredRouteLengths)
        {
            routeLengths.Add(length.Key, length.Value);
        }

        return routeLengths;
    }

    private static int GetRouteLength(Dictionary<string, DijkstraNode> nodes, ValveRoute route)
    {
        foreach (KeyValuePair<string,DijkstraNode> node in nodes)
        {
            node.Value.TentativeDistance = int.MaxValue;
        }
        nodes[route.From].TentativeDistance = 0;
        
        HashSet<DijkstraNode> unvisitedNodes = new(nodes.Select(node => node.Value));

        while (unvisitedNodes.Count > 0)
        {
            DijkstraNode node = unvisitedNodes.MinBy(node => node.TentativeDistance)!;

            if (node.Valve.Name == route.To)
            {
                return node.TentativeDistance;
            }
            
            unvisitedNodes.Remove(node);

            foreach (string connectedValve in node.Valve.ConnectedValves)
            {
                DijkstraNode connectedNode = nodes[connectedValve];
                connectedNode.TentativeDistance = Math.Min(connectedNode.TentativeDistance, node.TentativeDistance + 1);
            }
        }

        throw new Exception($"Could not find a path for the route: {route}");
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

    private record State(Valve Valve, int Minute, int EventualFlow, ImmutableHashSet<Valve> ClosedValves)
    {
        public override string ToString()
        {
            return $"Valve = {Valve.Name}, Minute = {Minute}, Eventual flow = {EventualFlow}, Closed valves = {string.Join(", ", ClosedValves)}";
        }
    }

    private readonly record struct ValveRoute(string From, string To);

    private class DijkstraNode
    {
        public readonly Valve Valve;

        public int TentativeDistance;

        public DijkstraNode(Valve valve)
        {
            Valve = valve;
            TentativeDistance = 0;
        }
    }
}