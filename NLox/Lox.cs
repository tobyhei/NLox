using System;
using System.Globalization;
using System.IO;

namespace NLox
{
    internal class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError;
        private static bool hadRuntimeError;

        private static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nlox [script]");
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                runPrompt();
            }
        }

        private static void runFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            run(Convert.ToString(bytes, CultureInfo.InvariantCulture));
        }

        private static void runPrompt()
        {
            while (true)
            {
                var line = Console.ReadLine();
                Console.WriteLine("> ");
                run(line);
            }
        }

        private static void run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            Expr expression = parser.parse();

            // Stop if there was a syntax error.
            if (hadError) Environment.Exit(65);
            if (hadRuntimeError) Environment.Exit(70);

            interpreter.interpret(expression);
        }

        public static void error(int line, string message)
        {
            report(line, "", message);
        }

        public static void error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                report(token.line, " at end", message);
            }
            else
            {
                report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void runtimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.Message} \n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] error" + where + ": " + message);
            hadError = true;
        }
    }
}