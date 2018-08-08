using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UpgradeNugetPackages
{
    class Program
    {
        static void DoReplace(string filename, IEnumerable<(string, string)> replacements)
        {
            var text = File.ReadAllText(filename);

            var madeChange = false;
            foreach (var (oldStr, newStr) in replacements)
            {
                if (!text.Contains(oldStr)) continue;
                
                text = text.Replace(oldStr, newStr);
                madeChange = true;
            }

            if (madeChange)
                File.WriteAllText(filename, text, encoding: new System.Text.UTF8Encoding(true));
            
        }
        
        static void PrintHelp()
        {
            Console.WriteLine("Usage: dotnet UpgradeNugetPackage.dll <path> <packagename> <oldVersion> <newVersion>");
        }
        
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                PrintHelp();
                return;
            }
            
            var path = args[0];
            var packageName = args[1];
            var oldVersion = args[2];
            var newVersion = args[3];

            if (oldVersion.Count(x => x == '.') < 3 || newVersion.Count(x => x == '.') < 3)
            {
                Console.WriteLine("Please provide version numbers with 4 parts, e.g. 1.0.30.0");
                return;
            }
                

            var oldVersionSemVer = new string(oldVersion.Reverse().SkipWhile(x => x != '.').Skip(1).Reverse().ToArray());
            var newVersionSemVer = new string(newVersion.Reverse().SkipWhile(x => x != '.').Skip(1).Reverse().ToArray());
            Console.WriteLine(oldVersionSemVer);
            Console.WriteLine(newVersionSemVer);

            var oldPackageStr = $"\"{packageName}\" version=\"{oldVersionSemVer}\"";
            var newPackageStr = oldPackageStr.Replace(oldVersionSemVer, newVersionSemVer);
            var packageReplacments = new[]
            {
                (oldPackageStr, newPackageStr)
            };
            var packagesFiles = Directory.GetFiles(path, "packages.config", SearchOption.AllDirectories);
            foreach (var file in packagesFiles)
                DoReplace(file, packageReplacments);

            var oldReference = $"\"{packageName}, Version={oldVersion}, Culture=neutral, PublicKeyToken=null\"";
            var newReference = oldReference.Replace(oldVersion, newVersion);
            var oldHintPath =  $@"packages\{packageName}.{oldVersionSemVer}";
            var newHintPath = oldHintPath.Replace(oldVersionSemVer, newVersionSemVer);
            var csProjReplacements = new[]
            {
                (oldReference, newReference),
                (oldHintPath, newHintPath)
            };
            
            var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            foreach (var file in csprojFiles)
                DoReplace(file, csProjReplacements);
        }
    }
}