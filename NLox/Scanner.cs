using System;
using System.Collections.Generic;

namespace NLox
{
    public class Scanner
    {
        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            {"and", TokenType.AND},
            {"class", TokenType.CLASS},
            {"else", TokenType.ELSE},
            {"false", TokenType.FALSE},
            {"for", TokenType.FOR},
            {"fun", TokenType.FUN},
            {"if", TokenType.IF},
            {"nil", TokenType.NIL},
            {"or", TokenType.OR},
            {"print", TokenType.PRINT},
            {"return", TokenType.RETURN},
            {"super", TokenType.SUPER},
            {"this", TokenType.THIS},
            {"true", TokenType.TRUE},
            {"var", TokenType.VAR},
            {"while", TokenType.WHILE}
        };

        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!isAtEnd)
            {
                // We are at the beginning of the next lexeme.
                start = current;
                scanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private bool isAtEnd => current >= source.Length;

        private void scanToken()
        {
            char next = advance();
            switch (next)
            {
                case '(': addToken(TokenType.LEFT_PAREN); break;
                case ')': addToken(TokenType.RIGHT_PAREN); break;
                case '{': addToken(TokenType.LEFT_BRACE); break;
                case '}': addToken(TokenType.RIGHT_BRACE); break;
                case ',': addToken(TokenType.COMMA); break;
                case '.': addToken(TokenType.DOT); break;
                case '-': addToken(TokenType.MINUS); break;
                case '+': addToken(TokenType.PLUS); break;
                case ';': addToken(TokenType.SEMICOLON); break;
                case '*': addToken(TokenType.STAR); break;
                case '!': addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/' when match('/'):
                    // A comment goes until the end of the line.
                    while (peek() != '\n' && !isAtEnd) advance();
                    break;
                case '/': addToken(TokenType.SLASH); break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '\n':
                    line++;
                    break;
                case '\"': consumeString(); break;
                case '\'': consumeString(); break;
                case 'o':
                    if (peek() == 'r')
                    {
                        addToken(TokenType.OR);
                    }
                    break;
                case char c when isDigit(c): consumeNumber(); break;
                case char a when isAlpha(a): consumeIdentifier(); break;
                default: Lox.error(line, "Unexpected character."); break;
            }
        }

        private char advance()
        {
            current++;
            return source[current-1];
        }

        private void addToken(TokenType type) => addToken(type, null);

        private void addToken(TokenType type, object literal)
        {
            var text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool match(char expected)
        {
            if (isAtEnd) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char peek() => isAtEnd ? '\0' : source[current];

        private void consumeString()
        {
            while (peek() != '"' && !isAtEnd)
            {
                if (peek() == '\n') line++;
                advance();
            }

            // Unterminated string.
            if (isAtEnd)
            {
                Lox.error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            advance();

            // Trim the surrounding quotes.
            var value = source.Substring(start + 1, current - start - 2);
            addToken(TokenType.STRING, value);
        }

        private bool isDigit(char c) => c >= '0' && c <= '9';

        private void consumeNumber()
        {
            while (isDigit(peek())) advance();

            // Look for a fractional part.
            if (peek() == '.' && isDigit(peekNext()))
            {
                // Consume the "."
                advance();

                while (isDigit(peek())) advance();
            }

            addToken(TokenType.NUMBER, double.Parse(source.Substring(start, current-start)));
        }

        private char peekNext()
            => current + 1 >= source.Length ? '\0' : source[current + 1];

        private void consumeIdentifier()
        {
            while (isAlphaNumeric(peek())) advance();

            // See if the identifier is a reserved word.
            string text = source.Substring(start, current - start);

            var type = keywords.TryGetValue(text, out var type1) ? type1 : TokenType.IDENTIFIER;
            addToken(type);
        }

        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_';
        }

        private bool isAlphaNumeric(char c) => isAlpha(c) || isDigit(c);
    }
}