using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static SharpMC.Generator.Prismarine.CodeGen;
using static SharpMC.Generator.Tools.Helpers;

// ReSharper disable UseObjectOrCollectionInitializer

namespace SharpMC.Generator.Prismarine.Data
{
    internal static class DataPriWriter
    {
        public static void WriteBlocks(Block[] blocks, string target)
        {
            const string fieldType = "Block";
            var f = new List<OneField>();
            foreach (var block in blocks)
            {
                var fieldName = ToTitleCase(block.Name);
                var v = $" = new {fieldType} {{ Id = {block.Id}, " +
                        $"DisplayName = \"{block.DisplayName}\", Name = \"{block.Name}\", " +
                        $"MinStateId = {block.MinStateId}, MaxStateId = {block.MaxStateId}, " +
                        $"DefaultState = {block.DefaultState}, Material = \"{block.Material}\" " +
                        "}";
                f.Add(new OneField
                {
                    Name = fieldName, TypeName = $"readonly {fieldType}", Constant = v
                });
            }
            f = f.OrderBy(x => x.Name).ToList();
            var allNames = f.Select(e => e.Name);
            f.Insert(0, new OneField
            {
                Name = "All", TypeName = $"readonly {fieldType}[]",
                Constant = $" = {{ {string.Join(", ", allNames)} }}"
            });
            var item = new OneUnit
            {
                Class = "KnownBlocks",
                Namespace = $"{nameof(SharpMC)}.Blocks",
                Fields = f
            };
            Console.WriteLine($" * {item.Class}");
            Write(item, target);
        }

        private static void Write(OneUnit item, string target)
        {
            var nsp = item.Namespace;
            var nspDir = nsp.Replace('.', Path.DirectorySeparatorChar);
            var className = item.Class;
            var outPath = Path.Combine(target, nspDir, $"{className}.cs");
            var outDir = Path.GetDirectoryName(outPath);
            Directory.CreateDirectory(outDir);
            var lines = new List<string>();
            lines.Add("using System;");
            lines.Add(string.Empty);
            lines.Add($"namespace {nsp}");
            lines.Add("{");
            var ext = new List<string>();
            var extStr = ext.Count == 0 ? string.Empty : $" : {string.Join(", ", ext)}";
            lines.Add($"{Sp}public class {className}{extStr}");
            lines.Add($"{Sp}{{");
            foreach (var field in item.Fields)
            {
                var fName = field.Name;
                var fType = field.TypeName;
                var fInit = field.Constant;
                lines.Add($"{Sp}{Sp}public static {fType} {fName}{fInit};");
            }
            lines.Add($"{Sp}}}");
            lines.Add("}");
            File.WriteAllLines(outPath, lines, Encoding.UTF8);
        }
    }
}