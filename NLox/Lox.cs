using System;
using System.Globalization;
using System.IO;

namespace NLox
{
    internal class Lox
    {
        private static bool hadError;

        private static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nlox [script]");
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Run(Convert.ToString(bytes, CultureInfo.InvariantCulture));
        }

        private static void RunPrompt()
        {
            while (true)
            {
                var line = Console.ReadLine();
                Console.WriteLine("> ");
                Run(line);
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        public static void error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] error" + where + ": " + message);
            hadError = true;
        }
    }
}