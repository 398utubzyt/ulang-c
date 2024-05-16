using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Ulang
{
    internal class Program
    {
        

        static void TranslateDirectory(string directory, List<SourceFile> sources)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
            {
                if (Directory.Exists(entry))
                {
                    TranslateDirectory(entry, sources);
                    continue;
                }

                if (SourceFile.Read(entry, out SourceFile src))
                {
                    src.Id = sources.Count;
                    sources.Add(src);
                    //Console.WriteLine(src.Path);
                    //using FileStream fs = new FileStream(src.Path, FileMode.Open, FileAccess.Read);
                    //using StreamReader sr = new StreamReader(fs);
                    //foreach (Token t in src.Tokens)
                    //    Console.WriteLine(t.ToString(sr));
                }
            }
        }

        static void Main(string[] cmd)
        {
            if (cmd?.Length == 0)
            {
                cmd = ["C:/Users/Admin/Documents/GitHub/ulang/Parser/tests/test.uproj"];
            }

            if (!CmdArgs.Parse(cmd, out CmdArgs args))
                return;

            if (!Directory.Exists(args.OutputPath))
                Directory.CreateDirectory(args.OutputPath);

            if (!ProjectFile.Read(args.ProjectPath, out ProjectFile proj))
            {
                Console.WriteLine($"Could not read project file '{args.ProjectPath}'.");
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<SourceFile> sources = new List<SourceFile>();
            TranslateDirectory(Path.GetDirectoryName(proj.Path), sources);
            Span<SourceFile> src = sources.ToArray().AsSpan();
            //Console.WriteLine();

            Compiler c = new Compiler(args);
            IRModule mod = new IRModule(proj);

            if (c.Compile(ref mod, src))
            {
                mod.Write(Path.Combine(args.OutputPath, $"{proj.Id}.uir"));
                Console.WriteLine("Compilation succeeded.");
            } else
                Console.WriteLine("There was a compilation error.");

            sw.Stop();
            Console.WriteLine($"Time elapsed: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");

            
            mod.Dispose();

            return;
        }
    }
}