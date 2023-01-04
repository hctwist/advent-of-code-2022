using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/20
/// </summary>
[Solution(20)]
[SolutionInput("Input20.test.txt")]
[SolutionInput("Input20.txt", Benchmark = true)]
public class Solution20 : Solution
{
    private readonly List<long> file;

    public Solution20(Input input) : base(input)
    {
        file = Input.Lines.Select(long.Parse).ToList();
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
         return Decrypt(file, 1).ToString();
    }
    
    /// <inheritdoc />
    protected override string? Problem2()
    {
        return Decrypt(file.Select(n => n * 811_589_153), 10).ToString();
    }

    private static long Decrypt(IEnumerable<long> file, int count)
    {
        LinkedList<long> nodes = new(file);
        List<LinkedListNode<long>> originallyOrderedNodes = new(nodes.Count);
        LinkedListNode<long>? cursor = nodes.First!;
        while (cursor != null)
        {
            originallyOrderedNodes.Add(cursor);
            cursor = cursor.Next;
        }

        for (int i = 0; i < count; i++)
        {
            foreach (LinkedListNode<long> node in originallyOrderedNodes)
            {
                Move(nodes, node, node.Value);
            }
        }

        LinkedListNode<long> zeroNode = nodes.Find(0) ?? throw new Exception("Cannot find a zero node");
        long coordinates = Find(nodes, zeroNode, 1_000).Value + Find(nodes, zeroNode, 2_000).Value + Find(nodes, zeroNode, 3_000).Value;
        return coordinates;
    }

    private static void Move<T>(LinkedList<T> nodes, LinkedListNode<T> node, long offset)
    {
        // As we are moving the node, we don't count it as part of repeating cycles
        offset %= (nodes.Count - 1);

        if (offset == 0)
        {
            return;
        }

        LinkedListNode<T> cursor = node;

        if (offset < 0)
        {
            for (int i = 0; i < -offset; i++)
            {
                cursor = cursor.Previous ?? nodes.Last!;
            }

            nodes.Remove(node);
            if (cursor == nodes.First)
            {
                nodes.AddLast(node);
            }
            else
            {
                nodes.AddBefore(cursor, node);
            }
        }
        else
        {
            for (int i = 0; i < offset; i++)
            {
                cursor = cursor.Next ?? nodes.First!;
            }

            nodes.Remove(node);
            nodes.AddAfter(cursor, node);
        }
    }

    private static LinkedListNode<T> Find<T>(LinkedList<T> nodes, LinkedListNode<T> node, int offset)
    {
        offset %= nodes.Count;

        LinkedListNode<T> cursor = node;

        if (offset < 0)
        {
            for (int i = 0; i < -offset; i++)
            {
                cursor = cursor.Previous ?? nodes.Last!;
            }
        }
        else
        {
            for (int i = 0; i < offset; i++)
            {
                cursor = cursor.Next ?? nodes.First!;
            }
        }

        return cursor;
    }
}