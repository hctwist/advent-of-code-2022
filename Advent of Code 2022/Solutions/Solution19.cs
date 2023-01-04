using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/19
/// </summary>
[Solution(19, Enabled = false)]
[SolutionInput("Input19.test.txt")]
[SolutionInput("Input19.txt", Benchmark = true)]
public class Solution19 : Solution
{
    private const int Minutes = 24;
    
    private static readonly Regex BlueprintPattern = new(@"^Blueprint \d+: Each ore robot costs (\d+) ore. Each clay robot costs (\d+) ore. Each obsidian robot costs (\d+) ore and (\d+) clay. Each geode robot costs (\d+) ore and (\d+) obsidian.$");

    private readonly List<Blueprint> blueprints;

    /// <inheritdoc />
    public Solution19(Input input) : base(input)
    {
        blueprints = new List<Blueprint>(Input.Lines.Length);

        foreach (string line in Input.Lines)
        {
            Match match = BlueprintPattern.Match(line);

            CombinedResources oreRobotCost = new()
            {
                Ore = int.Parse(match.Groups[1].Value)
            };

            CombinedResources clayRobotCost = new()
            {
                Ore = int.Parse(match.Groups[2].Value)
            };

            CombinedResources obsidianRobotCost = new()
            {
                Ore = int.Parse(match.Groups[3].Value),
                Clay = int.Parse(match.Groups[4].Value)
            };

            CombinedResources geodeRobotCost = new()
            {
                Ore = int.Parse(match.Groups[5].Value),
                Obsidian = int.Parse(match.Groups[6].Value)
            };

            blueprints.Add(new Blueprint(oreRobotCost, clayRobotCost, obsidianRobotCost, geodeRobotCost));
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        int maximumGeodes = 0;

        foreach (Blueprint blueprint in blueprints)
        {
            State state = new(
                0,
                new CombinedResources(),
                Robot.OreRobot.Product);

            maximumGeodes = Math.Max(maximumGeodes, GetMaximumGeodesFrom(blueprint, state));
        }

        return maximumGeodes.ToString();
    }

    private int GetMaximumGeodesFrom(Blueprint blueprint, State state)
    {
        int maximumGeodes = (state.Resources + state.ResourceProduction * (Minutes - state.Minute)).Geodes;

        foreach (ResourceType type in Enum.GetValues<ResourceType>())
        {
            CombinedResources cost = blueprint.GetCostFor(type);
            if (!TryGetTimeToProduce(cost, state, out int minutesTaken))
            {
                // We can't produce a robot of this type
                continue;
            }

            State newState = state with
            {
                Minute = state.Minute + minutesTaken + 1,
                Resources = state.Resources + (minutesTaken + 1) * state.ResourceProduction - cost,
                ResourceProduction = state.ResourceProduction + Robot.GetFor(type).Product
            };

            maximumGeodes = Math.Max(maximumGeodes, GetMaximumGeodesFrom(blueprint, newState));
        }

        return maximumGeodes;
    }

    private static bool TryGetTimeToProduce(CombinedResources resources, State state, out int minutesTaken)
    {
        int minute = 0;

        CombinedResources resourcesAvailable = state.Resources;

        while (state.Minute + minute < Minutes)
        {
            if (resourcesAvailable >= resources)
            {
                minutesTaken = minute;
                return true;
            }

            resourcesAvailable += state.ResourceProduction;
            minute++;
        }

        minutesTaken = default;
        return false;
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        return null;
    }

    private class Blueprint
    {
        private readonly Dictionary<ResourceType, CombinedResources> costs;

        public Blueprint(CombinedResources oreRobotCost, CombinedResources clayRobotCost, CombinedResources obsidianRobotCost, CombinedResources geodeRobotCost)
        {
            costs = new Dictionary<ResourceType, CombinedResources>()
            {
                { ResourceType.Ore, oreRobotCost },
                { ResourceType.Clay, clayRobotCost },
                { ResourceType.Obsidian, obsidianRobotCost },
                { ResourceType.Geode, geodeRobotCost }
            };
        }

        public CombinedResources GetCostFor(ResourceType type)
        {
            return costs[type];
        }
    }

    private record CombinedResources(int Ore, int Clay, int Obsidian, int Geodes)
    {
        public CombinedResources() : this(0, 0, 0, 0)
        {
        }

        public static bool operator >=(CombinedResources resources1, CombinedResources resources2)
        {
            return resources1.Ore >= resources2.Ore &&
                resources1.Clay >= resources2.Clay &&
                resources1.Obsidian >= resources2.Obsidian &&
                resources1.Geodes >= resources2.Geodes;
        }

        public static bool operator <=(CombinedResources resources1, CombinedResources resources2)
        {
            return resources1.Ore <= resources2.Ore &&
                resources1.Clay <= resources2.Clay &&
                resources1.Obsidian <= resources2.Obsidian &&
                resources1.Geodes <= resources2.Geodes;
        }

        public static CombinedResources operator -(CombinedResources resources1, CombinedResources resources2)
        {
            return new CombinedResources(
                resources1.Ore - resources2.Ore,
                resources1.Clay - resources2.Clay,
                resources1.Obsidian - resources2.Obsidian,
                resources1.Geodes - resources2.Geodes);
        }

        public static CombinedResources operator +(CombinedResources resources1, CombinedResources resources2)
        {
            return new CombinedResources(
                resources1.Ore + resources2.Ore,
                resources1.Clay + resources2.Clay,
                resources1.Obsidian + resources2.Obsidian,
                resources1.Geodes + resources2.Geodes);
        }

        public static CombinedResources operator *(CombinedResources resources, int x)
        {
            return new CombinedResources(
                resources.Ore * x,
                resources.Clay * x,
                resources.Obsidian * x,
                resources.Geodes * x);
        }
        
        public static CombinedResources operator *(int x, CombinedResources resources)
        {
            return resources * x;
        }
    }

    private record State(
        int Minute,
        CombinedResources Resources,
        CombinedResources ResourceProduction);

    private record Robot(CombinedResources Product)
    {
        public static readonly Robot OreRobot = new(
            new CombinedResources()
            {
                Ore = 1
            });

        public static readonly Robot ClayRobot = new(
            new CombinedResources()
            {
                Clay = 1
            });

        public static readonly Robot ObsidianRobot = new(
            new CombinedResources()
            {
                Obsidian = 1
            });

        public static readonly Robot GeodeRobot = new(
            new CombinedResources()
            {
                Geodes = 1
            });

        public static Robot GetFor(ResourceType type)
        {
            return type switch
            {
                ResourceType.Ore => OreRobot,
                ResourceType.Clay => ClayRobot,
                ResourceType.Obsidian => ObsidianRobot,
                ResourceType.Geode => GeodeRobot,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }

    private enum ResourceType
    {
        Ore,
        Clay,
        Obsidian,
        Geode
    }
}