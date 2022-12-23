using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/13
/// </summary>
[Solution(13)]
[SolutionInput("Input13.test.txt", Enabled = false)]
[SolutionInput("Input13.txt", Benchmark = true, Problem1Solution = "5350", Problem2Solution = "19570")]
public class Solution13 : Solution
{
    public Solution13(Input input) : base(input)
    {
        
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        List<(string, string)> packetPairs = new((Input.Lines.Length + 1) / 3);

        for (int i = 0; i < Input.Lines.Length; i += 3)
        {
            packetPairs.Add((Input.Lines[i], Input.Lines[i + 1]));
        }
        
        int sum = 0;
        
        for (int i = 0; i < packetPairs.Count; i++)
        {
            int n = i + 1;
            
            (string, string) pair = packetPairs[i];
            if (ComparePackets(pair.Item1, pair.Item2) < 0)
            {
                sum += n;
            }
        }
        
        return sum.ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        List<string> packets = new((Input.Lines.Length + 1) / 3 + 2);

        for (int i = 0; i < Input.Lines.Length; i += 3)
        {
            packets.Add(Input.Lines[i]);
            packets.Add(Input.Lines[i + 1]);
        }

        string dividerPacket1 = "[[2]]";
        string dividerPacket2 = "[[6]]";
        
        packets.Add(dividerPacket1);
        packets.Add(dividerPacket2);
        
        packets.Sort(Comparer<string>.Create((left, right) => ComparePackets(left, right)));

        int decoderKey = 1;

        for (int i = 0; i < packets.Count; i++)
        {
            string packet = packets[i];
            if (packet == dividerPacket1 || packet == dividerPacket2)
            {
                decoderKey *= i + 1;
            }
        }

        return decoderKey.ToString();
    }

    private static int ComparePackets(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        while (left.Length > 0 && right.Length > 0)
        {
            int leftRead;
            int rightRead;
            int comparison;

            // Two array packets
            if (left[0] == '[' && right[0] == '[')
            {
                leftRead = ReadArrayPacket(left, out ReadOnlySpan<char> leftArrayPacket);
                rightRead = ReadArrayPacket(right, out ReadOnlySpan<char> rightArrayPacket);
                comparison = ComparePackets(leftArrayPacket[1..^1], rightArrayPacket[1..^1]);
            }
            // Left is an array packet, right must be an integer packet
            else if (left[0] == '[')
            {
                leftRead = ReadArrayPacket(left, out ReadOnlySpan<char> leftArrayPacket);
                rightRead = ReadIntegerPacket(right, out ReadOnlySpan<char> rightIntegerPacket);
                comparison = ComparePackets(leftArrayPacket[1..^1], rightIntegerPacket);
            }
            // Right is an array packet, left must be an integer packet
            else if (right[0] == '[')
            {
                leftRead = ReadIntegerPacket(left, out ReadOnlySpan<char> leftIntegerPacket);
                rightRead = ReadArrayPacket(right, out ReadOnlySpan<char> rightArrayPacket);
                comparison = ComparePackets(leftIntegerPacket, rightArrayPacket[1..^1]);
            }
            // They're both integers
            else
            {
                leftRead = ReadIntegerPacket(left, out ReadOnlySpan<char> leftIntegerPacket);
                rightRead = ReadIntegerPacket(right, out ReadOnlySpan<char> rightIntegerPacket);
                comparison = int.Parse(leftIntegerPacket).CompareTo(int.Parse(rightIntegerPacket));
            }

            if (comparison != 0)
            {
                return comparison;
            }

            if (leftRead >= left.Length || rightRead >= right.Length)
            {
                break;
            }

            left = left[(leftRead + 1)..];
            right = right[(rightRead + 1)..];
        }

        return left.Length - right.Length;
    }

    private static int ReadIntegerPacket(ReadOnlySpan<char> packet, out ReadOnlySpan<char> integerPacket)
    {
        if (!char.IsDigit(packet[0]))
        {
            throw new ArgumentException($"Cannot process a non-integer packet: {packet}");
        }

        for (int i = 0; i < packet.Length; i++)
        {
            if (!char.IsDigit(packet[i]))
            {
                integerPacket = packet[..i];
                return i;
            }
        }

        integerPacket = packet;
        return packet.Length;
    }

    private static int ReadArrayPacket(ReadOnlySpan<char> packet, out ReadOnlySpan<char> arrayPacket)
    {
        if (packet[0] != '[')
        {
            throw new ArgumentException($"Cannot process a non-array packet: {packet}");
        }
        
        int count = 0;
        
        for (int i = 0; i < packet.Length; i++)
        {
            if (packet[i] == '[')
            {
                count++;
            }
            else if (packet[i] == ']')
            {
                count--;
            }

            if (count == 0)
            {
                arrayPacket = packet[..(i + 1)];
                return i + 1;
            }
        }

        throw new ArgumentException($"Could not find a matching bracket for array packet: {packet}");
    }
}