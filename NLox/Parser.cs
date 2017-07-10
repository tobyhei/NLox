﻿using System.Collections.Generic;

namespace NLox
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> parse()
        {
            var statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        private Stmt declaration()
        {
            try
            {
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch (ParseError)
            {
                synchronize();
                return null;
            }
        }

        private Stmt varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (match(TokenType.EQUAL))
            {
                initializer = expression();
            }

            consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt statement()
        {
            if (match(TokenType.FOR)) return forStatement();
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.LEFT_BRACE)) return new Stmt.Block(block());

            return expressionStatement();
        }

        private Stmt forStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (match(TokenType.SEMICOLON)) initializer = null;
            else if (match(TokenType.VAR)) initializer = varDeclaration();
            else initializer = expressionStatement();

            Expr condition = null;
            if (!check(TokenType.SEMICOLON)) condition = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!check(TokenType.RIGHT_PAREN)) increment = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after 'for' clauses.");

            Stmt body = statement();

            if (increment != null)
                body = new Stmt.Block(new List<Stmt>{ body, new Stmt.Expression(increment) });

            if (condition == null)
                condition = new Expr.Literal(TokenType.TRUE);
            body = new Stmt.While(condition, body);

            if (initializer != null) body = new Stmt.Block(new List<Stmt>{initializer, body});

            return body;
        }

        private Stmt whileStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect ( after while.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ) after condition.");

            Stmt body = statement();
            return new Stmt.While(condition, body);
        }

        private Stmt ifStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = statement();
            Stmt elseBranch = match(TokenType.ELSE) ? statement() : null;

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr expression()
        {
            return assignment();
        }

        private Expr assignment()
        {
            Expr expr = or();

            if (match(TokenType.EQUAL))
            {
                Token equals = previous();
                Expr value = assignment();

                if (expr is Expr.Variable ev) {
                    Token name = ev.name;
                    return new Expr.Assign(name, value);
                }

                error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr or()
        {
            Expr expr = and();

            while (match(TokenType.OR))
            {
                Token @operator = previous();
                Expr right = and();
                expr = new Expr.Logical(expr, @operator, right);
            }

            return expr;
        }

        private Expr and()
        {
            Expr expr = equality();

            while (match(TokenType.AND))
            {
                Token @operator = previous();
                Expr right = equality();
                expr = new Expr.Logical(expr, @operator, right);
            }

            return expr;
        }

        private Expr equality()
        {
            Expr expr = comparison();

            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token @operator = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private bool match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        private bool check(TokenType tokenType)
        {
            if (isAtEnd()) return false;
            return peek().type == tokenType;
        }

        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        private bool isAtEnd() => peek().type == TokenType.EOF;

        private Token peek() => tokens[current];

        private Token previous() => tokens[current - 1];

        private Expr comparison()
        {
            Expr expr = term();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token @operator = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr term()
        {
            Expr expr = factor();

            while (match(TokenType.MINUS, TokenType.PLUS))
            {
                Token @operator = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr factor()
        {
            Expr expr = unary();

            while (match(TokenType.SLASH, TokenType.STAR))
            {
                Token @operator = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr unary()
        {
            if (match(TokenType.BANG, TokenType.MINUS))
            {
                Token @operator = previous();
                Expr right = unary();
                return new Expr.Unary(@operator, right);
            }

            return primary();
        }

        private Expr primary()
        {
            if (match(TokenType.FALSE)) return new Expr.Literal(false);
            if (match(TokenType.TRUE)) return new Expr.Literal(true);
            if (match(TokenType.NIL)) return new Expr.Literal(null);

            if (match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().literal);
            }

            if (match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(previous());
            }

            if (match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw error(peek(), "Expect expression.");
        }

        private Token consume(TokenType type, string message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }

        private List<Stmt> block()
        {
            var statements = new List<Stmt>();

            while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private ParseError error(Token token, string message)
        {
            Lox.error(token, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().type == TokenType.SEMICOLON) return;

                switch (peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                advance();
            }
        }
    }
}
