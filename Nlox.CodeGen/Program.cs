using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nlox.CodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length  != 1) throw new ArgumentException("requires 1 argument for output directory");

            var outputDir = args[0];

            defineAst(outputDir, "Expr", new List<string> { 
                "Binary   : Expr left, Token @operator, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Unary    : Token @operator, Expr right"
            });
        }

        public static void defineAst(
            string outputDir, string baseName, List<string> types)
        {
            var path = outputDir + Path.DirectorySeparatorChar + baseName + ".cs";

            using (var fileWriter = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(fileWriter))
            {
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine("namespace NLox {");
                writer.WriteLine($"    public abstract class {baseName}");
                writer.WriteLine("    {");

                writer.WriteLine("        public abstract TR accept<TR>(IVisitor<TR> visitor);");
                writer.WriteLine("");

                defineVisitor(writer, baseName, types);

                // The AST classes.
                foreach (var type in types)
                {
                    var className = type.Split(':')[0].Trim();
                    var fields = type.Split(':')[1].Trim();
                    defineType(writer, baseName, className, fields);
                }

                // close file
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }

        private static void defineType(
            StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine("");
            writer.WriteLine($"        public class {className} : {baseName}");
            writer.WriteLine("        {");

            // Fields.
            var fields = fieldList.Split(new[] { ", " }, StringSplitOptions.None);

            foreach (var field in fields)
            {
                writer.WriteLine($"            public readonly {field};");
            }

            writer.WriteLine();

            // Constructor.
            writer.WriteLine($"            public {className}({fieldList})");
            writer.WriteLine("            {");

            // Store parameters in fields.
            foreach (var field in fields)
            {
                string name = field.Split(' ')[1];
                writer.WriteLine($"                this.{name} = {name};");
            }

            writer.WriteLine("            }");

            // Visitor pattern.
            writer.WriteLine();
            writer.WriteLine("            public override TR accept<TR>(IVisitor<TR> visitor)");
            writer.WriteLine("            {");
            writer.WriteLine("                return visitor.visit" +
                className + baseName + "(this);");
            writer.WriteLine("            }");

            writer.WriteLine("        }");
        }

        private static void defineVisitor(
            StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("        public interface IVisitor<TR>");
            writer.WriteLine("        {");

            foreach (var type in types)
            {
                var typeName = type.Split(':')[0].Trim();
                writer.WriteLine(
                    "            " +
                    $"TR visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("        }");
        }
    }
}