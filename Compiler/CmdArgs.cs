using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
    public struct AllocatorArgs
    {
        public string New;
        public string Drop;
    }

    public struct CmdArgs
    {
        public string ProjectPath;
        public string OutputPath;
        public AllocatorArgs Allocator;
        public string EntryPoint;
        public bool NoDeprecated;
        public bool NoWarnings;

        public static bool Parse(string[] cmd, out CmdArgs args)
        {
            args = new CmdArgs();
            if (cmd.Length < 1)
            {
                Console.WriteLine("Please specify a U project to compile.");
                return false;
            }

            args.ProjectPath = cmd[0];

            if (cmd.Length == 1)
                return true;

            int i = 1;
            while (i < cmd.Length)
            {
                switch (cmd[i++].ToLower().AsSpan())
                {
                    case "--alloc":
                    case "-a":
                        if (i >= cmd.Length - 1)
                        {
                            if (cmd[i].Equals("null", StringComparison.OrdinalIgnoreCase))
                                break;
                            Console.WriteLine("Invalid format for '--alloc' flag. Format is '--alloc [allocFunc] [deallocFunc]' or '--alloc null'.");
                            return false;
                        }

                        args.Allocator = new AllocatorArgs { New = cmd[i++], Drop = cmd[i] };
                        break;
                    case "--entry":
                    case "-e":
                        if (i >= cmd.Length)
                        {
                            Console.WriteLine("Invalid format for '--entry' flag. Format is '--entry [entryFunc]'.");
                            return false;
                        }
                        args.EntryPoint = cmd[i];
                        break;
                    case "--nodeprecated":
                    case "-d":
                        args.NoDeprecated = true;
                        break;
                    case "--nowarn":
                    case "-w":
                        args.NoWarnings = true;
                        break;
                    case "--output":
                    case "-o":
                        if (i >= cmd.Length)
                        {
                            Console.WriteLine("Invalid format for '--output' flag. Format is '--output [outputPath]'.");
                            return false;
                        }
                        args.OutputPath = cmd[i];
                        break;

                    default:
                        Console.WriteLine($"Unknown argument '{cmd[--i]}'.");
                        return false;
                }
                i++;
            }

            args.OutputPath ??= Path.Combine(Path.GetDirectoryName(args.ProjectPath), "bin");

            return true;
        }
    }
}
