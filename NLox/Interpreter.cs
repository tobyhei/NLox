using System;
using System.Collections.Generic;
using NLox.Misc;

namespace NLox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Unit>
    {
        private Environment environment = new Environment();

        public void interpret(IEnumerable<Stmt> stmts)
        {
            try
            {
                foreach (var stmt in stmts)
                {
                    execute(stmt);
                }
            }
            catch (RuntimeError error)
            {
                Lox.runtimeError(error);
            }
        }

        private string stringify(object @object)
        {
            if (@object == null) return "nil";

            // Hack. Work around Java adding ".0" to integer-valued doubles.
            if (@object is double)
            {
                string text = @object.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return @object.ToString();
        }

        public object visitBinaryExpr(Expr.Binary expr)
        {
            object left = evaluate(expr.left);
            object right = evaluate(expr.right);

            object evalDouble(Func<double, double, object> func)
            {
                if (!(left is double ld && right is double rd))
                    throw new RuntimeError(expr.@operator, "Operands must be numbers.");
                return func(ld, rd);
            }

            switch (expr.@operator.type)
            {
                case TokenType.BANG_EQUAL: return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL: return isEqual(left, right);
                case TokenType.GREATER:
                    return evalDouble((l, r) => l > r);
                case TokenType.GREATER_EQUAL:
                    return evalDouble((l, r) => l >= r);
                case TokenType.LESS:
                    return evalDouble((l, r) => l < r);
                case TokenType.LESS_EQUAL:
                    return evalDouble((l, r) => l <= r);
                case TokenType.MINUS:
                    return evalDouble((l, r) => l - r);
                case TokenType.SLASH:
                    return evalDouble((l, r) => l / r);
                case TokenType.STAR:
                    return evalDouble((l, r) => l * r);
                case TokenType.PLUS when left is double dl && right is double dr:
                    return dl + dr;
                case TokenType.PLUS when left is string sl && right is string sr:
                    return sl + sr;
                case TokenType.PLUS: throw new RuntimeError(
                    expr.@operator, "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        public object visitGroupingExpr(Expr.Grouping expr) => evaluate(expr);

        public object visitLiteralExpr(Expr.Literal expr) => expr.value;

        public object visitUnaryExpr(Expr.Unary expr)
        {
            object right = evaluate(expr.right);

            switch (expr.@operator.type)
            {
                case TokenType.BANG:
                    return !isTruthy(right);
                case TokenType.MINUS:
                checkNumberOperand(expr.@operator, right);
                return -(double)right;
            }

            // Unreachable.
            return null;
        }

        private object evaluate(Expr expr) => expr.accept(this);

        private Unit execute(Stmt stmt) => stmt.accept(this);

        private static bool isTruthy(object @object)
        {
            if (@object == null) return false;
            if (@object is bool b) return b;
            return true;
        }

        private bool isEqual(object a, object b)
        {
            switch (a)
            {
                case null: return b == null;
                default:
                    return a.Equals(b);
            }
        }

        private void checkNumberOperand(Token @operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        Unit Stmt.IVisitor<Unit>.visitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return Unit.value;
        }

        Unit Stmt.IVisitor<Unit>.visitPrintStmt(Stmt.Print stmt)
        {
            var value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return Unit.value;
        }

        public Unit visitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = evaluate(stmt.initializer);
            }

            environment.define(stmt.name.lexeme, value);
            return Unit.value;
        }

        public object visitVariableExpr(Expr.Variable expr)
            => environment.get(expr.name);

        public object visitAssignExpr(Expr.Assign expr)
        {
            object value = evaluate(expr.value);

            environment.assign(expr.name, value);
            return value;
        }

        public Unit visitBlockStmt(Stmt.Block stmt)
        {
            executeBlock(stmt.statements, new Environment(environment));
            return Unit.value;
        }

        private void executeBlock(IEnumerable<Stmt> statements, Environment environment)
        {
            var previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (var statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }
    }
}
