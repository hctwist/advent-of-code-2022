using System.Data.SqlTypes;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using AdventOfCode.Framework;

namespace AdventOfCode2022.Solutions;

/// <summary>
/// https://adventofcode.com/2022/day/7
/// </summary>
[Solution(7)]
[SolutionInput("Input7.test.txt", Enabled = false)]
[SolutionInput("Input7.txt", Benchmark = true, Problem1Solution = "1581595", Problem2Solution = "1544176")]
public class Solution7 : Solution
{
    private static readonly Regex CDPattern = new(@"^\$ cd (.+)$");
    private static readonly Regex LSPattern = new(@"^\$ ls$");
    private static readonly Regex FilePattern = new(@"^(\d+) (.+)$");
    private static readonly Regex DirectoryPattern = new(@"^dir (.+)$");

    private readonly List<ICommand> commands;

    public Solution7(Input input) : base(input)
    {
        commands = new List<ICommand>();

        foreach (string line in Input.Lines)
        {
            Match cdMatch = CDPattern.Match(line);
            if (cdMatch.Success)
            {
                commands.Add(new CDCommand(cdMatch.Groups[1].Value));
                continue;
            }

            Match lsMatch = LSPattern.Match(line);
            if (lsMatch.Success)
            {
                // No-op
                continue;
            }

            Match fileMatch = FilePattern.Match(line);
            if (fileMatch.Success)
            {
                commands.Add(
                    new DiscoverFileCommand(
                        new File(
                            fileMatch.Groups[2].Value,
                            int.Parse(fileMatch.Groups[1].Value))));
                continue;
            }

            Match directoryMatch = DirectoryPattern.Match(line);
            if (directoryMatch.Success)
            {
                commands.Add(new DiscoverDirectoryCommand(directoryMatch.Groups[1].Value));
                continue;
            }
            
            throw new Exception($"Didn't recognise command '{line}'");
        }
    }

    /// <inheritdoc />
    protected override string? Problem1()
    {
        FileExplorer explorer = new();
        foreach (ICommand command in commands)
        {
            command.Execute(explorer);
        }

        DirectorySizeWalker walker = new();
        List<int> sizes = walker.Walk(explorer.Root);

        return sizes.Where(size => size < 100000).Sum().ToString();
    }

    /// <inheritdoc />
    protected override string? Problem2()
    {
        FileExplorer explorer = new();
        foreach (ICommand command in commands)
        {
            command.Execute(explorer);
        }

        DirectorySizeWalker walker = new();
        List<int> sizes = walker.Walk(explorer.Root);

        const int DiskSize = 70_000_000;
        const int UpdateSize = 30_000_000;

        int freeSpace = DiskSize - sizes.Max();
        int spaceRequired = UpdateSize - freeSpace;

        int candidate = int.MaxValue;

        foreach (int size in sizes)
        {
            if (size > spaceRequired & size < candidate)
            {
                candidate = size;
            }
        }

        return candidate.ToString();
    }

    public class FileExplorer
    {
        public readonly Directory Root;

        public Directory CurrentDirectory;

        public FileExplorer()
        {
            Root = new Directory(string.Empty, null);
            CurrentDirectory = Root;
        }
    }

    public class Directory
    {
        public readonly string Name;

        public readonly Directory? Parent;

        public readonly List<File> Files;

        public readonly List<Directory> Subdirectories;

        public Directory(string name, Directory? parent)
        {
            Name = name;
            Parent = parent;

            Subdirectories = new List<Directory>();
            Files = new List<File>();
        }
    }

    public record File(string Name, int Size);

    public interface ICommand
    {
        void Execute(FileExplorer explorer);
    }

    public class CDCommand : ICommand
    {
        private readonly string name;

        public CDCommand(string name)
        {
            this.name = name;
        }

        /// <inheritdoc />
        public void Execute(FileExplorer explorer)
        {
            switch (name)
            {
                case "/":
                    explorer.CurrentDirectory = explorer.Root;
                    return;
                case "..":
                    if (explorer.CurrentDirectory.Parent is null)
                    {
                        throw new Exception($"Could not move up from {explorer.CurrentDirectory.Name}");
                    }
                    explorer.CurrentDirectory = explorer.CurrentDirectory.Parent;
                    return;
                default:
                    foreach (Directory subdirectory in explorer.CurrentDirectory.Subdirectories)
                    {
                        if (subdirectory.Name == name)
                        {
                            explorer.CurrentDirectory = subdirectory;
                            return;
                        }
                    }
                    throw new Exception($"Could not move into {name} from {explorer.CurrentDirectory.Name}");
            }
        }
    }

    public class DiscoverDirectoryCommand : ICommand
    {
        private readonly string name;

        public DiscoverDirectoryCommand(string name)
        {
            this.name = name;
        }

        /// <inheritdoc />
        public void Execute(FileExplorer explorer)
        {
            Directory newDirectory = new(name, explorer.CurrentDirectory);
            explorer.CurrentDirectory.Subdirectories.Add(newDirectory);
        }
    }

    public class DiscoverFileCommand : ICommand
    {
        private readonly File file;

        public DiscoverFileCommand(File file)
        {
            this.file = file;
        }

        /// <inheritdoc />
        public void Execute(FileExplorer explorer)
        {
            explorer.CurrentDirectory.Files.Add(file);
        }
    }

    public class DirectorySizeWalker
    {
        private readonly List<int> sizes;

        public DirectorySizeWalker()
        {
            sizes = new List<int>();
        }
        
        public List<int> Walk(Directory directory)
        {
            sizes.Clear();
            WalkInternal(directory);
            return sizes;
        }

        private int WalkInternal(Directory directory)
        {
            int size = 0;

            foreach (File file in directory.Files)
            {
                size += file.Size;
            }

            foreach (Directory subdirectory in directory.Subdirectories)
            {
                size += WalkInternal(subdirectory);
            }
            
            sizes.Add(size);

            return size;
        }
    }
}