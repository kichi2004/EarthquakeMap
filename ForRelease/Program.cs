using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ForRelease
{
    class Program
    {
        private static void Main()
        {


            var ignoreFiles = new[] {
                "EarthquakeMap.exe",
                "EarthquakeMap.exe.config",
                "Koruri-Regular.ttf",
                "readme.txt",
                "kyoshin_points"
            };
            var nozip = new[] {
                "ForRelease.exe",
                "Software.zip",
            };
            string dir = null;
            try {
                dir = Directory.GetCurrentDirectory();
            } catch {
                Console.Read();
                return;
            }
            Console.WriteLine("current: " + dir);
            if (!Directory.Exists("lib")) Directory.CreateDirectory(dir + "\\lib");
            foreach (var file in Directory.EnumerateFiles(dir)) {
                var pass = file;
                if (file.EndsWith(".pdb") || file.EndsWith("xml")) {
                    File.Delete(file);
                    Console.WriteLine(file + " deleted.");
                    continue;
                }
                Console.Write(pass.Split('\\').Last());

                if (!ignoreFiles.Contains(file.Split('\\').Last()) &&
                    !nozip.Contains(file.Split('\\').Last())) {
                    var move = $@"{dir}\lib\{file.Split('\\').Last()}";
                    if (File.Exists(move)) File.Delete(move);
                    File.Move(pass, move);
                    Console.WriteLine(" moved to lib\\" + move.Split('\\').Last());
                } else
                    Console.WriteLine(" ignored");
            }
            Process.Start(@"C:\Program Files\7-Zip\7z.exe", $"a Software.zip Images lib {string.Join(" ", ignoreFiles)}");
        }
    }
}

