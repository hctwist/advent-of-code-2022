using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/16
/// </summary>
[Solution(16, Enabled = false)]
[SolutionInput("Input16.test.txt")]
[SolutionInput("Input16.txt", Benchmark = true)]
public class Solution16 : Solution
{
    private static readonly Regex Pattern = new(@"^Valve (\w\w) has flow rate=(\d+); tunnels? leads? to valves? (.+)$");

    private readonly Dictionary<string, Node> nodes;

    /// <inheritdoc />
    public Solution16(Input input) : base(input)
    {
        nodes = new Dictionary<string, Node>(Input.Lines.Length);

        foreach (string line in Input.Lines)
        {
            GroupCollection groups = Pattern.Match(line).Groups;

            string valve = groups[1].Value;
            int flowRate = int.Parse(groups[2].Value);
            string[] connectedValves = groups[3].Value.Split(", ");

            Node node = new(valve, flowRate, connectedValves);
            nodes.Add(valve, node);
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        nodes["AA"].Stage(new State(0, new HashSet<string>()));
        nodes["AA"].Commit();
        
        for (int i = 0; i < 30; i++)
        {
            Console.WriteLine($"Minute {i + 1}");
            Console.WriteLine($"Max states = {nodes.Values.Max(node => node.States.Count)}");
            foreach (Node node in nodes.Values)
            {
                ProcessNode(node, i + 1);
            }

            foreach (Node node in nodes.Values)
            {
                node.Commit();
            }
        }

        return nodes.Values.SelectMany(node => node.States).Max(state => state.EventualFlow).ToString();
    }

    private void ProcessNode(Node node, int minute)
    {
        foreach (State state in node.States)
        {
            // If the valve is not already open, we can open it
            if (!state.OpenValves.Contains(node.Valve))
            {
                node.Stage(state.UpdatedWith((30 - minute) * node.FlowRate, node.Valve));
            }
            
            // We can also move on
            foreach (string connectedValve in node.ConnectedValves)
            {
                nodes[connectedValve].Stage(state);
            }
        }
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }

    private class Node
    {
        public readonly string Valve;

        public readonly int FlowRate;

        public readonly string[] ConnectedValves;

        public readonly List<State> States;

        private readonly List<State> pendingStates;

        public Node(string valve, int flowRate, string[] connectedValves)
        {
            Valve = valve;
            FlowRate = flowRate;
            ConnectedValves = connectedValves;

            States = new List<State>();
            pendingStates = new List<State>();
        }

        public void Stage(State state)
        {
            pendingStates.Add(state);
        }

        public void Commit()
        {
            States.Clear();

            foreach (State pendingState in pendingStates)
            {
                AddStateAndResolveSuperiority(pendingState);
            }

            pendingStates.Clear();
        }

        private void AddStateAndResolveSuperiority(State state)
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                if (state.IsEqualOrSuperior(States[i]))
                {
                    States.RemoveAt(i);
                }
                else if (States[i].IsEqualOrSuperior(state))
                { 
                    return;
                }
            }
            
            States.Add(state);
        }
    }

    private record State(int EventualFlow, IReadOnlySet<string> OpenValves)
    {
        public State UpdatedWith(int addedFlow, string newlyOpenedValve)
        {
            int newEventualFlow = EventualFlow + addedFlow;
            
            HashSet<string> newOpenValves = new(OpenValves);
            newOpenValves.Add(newlyOpenedValve);

            return new State(newEventualFlow, newOpenValves);
        }

        public bool IsEqualOrSuperior(State other)
        {
            return EventualFlow >= other.EventualFlow && OpenValves.IsSubsetOf(other.OpenValves);
        }

        public override string ToString()
        {
            return $"Eventual flow = {EventualFlow}, Open valves = {string.Join(", ", OpenValves)}";
        }
    }
}