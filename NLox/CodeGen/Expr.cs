using System.Collections.Generic;

namespace NLox {
    public abstract class Expr
    {
        public abstract TR accept<TR>(IVisitor<TR> visitor);

        public interface IVisitor<TR>
        {
            TR visitAssignExpr(Assign expr);
            TR visitBinaryExpr(Binary expr);
            TR visitGroupingExpr(Grouping expr);
            TR visitLiteralExpr(Literal expr);
            TR visitUnaryExpr(Unary expr);
            TR visitVariableExpr(Variable expr);
        }

        public class Assign : Expr
        {
            public readonly Token name;
            public readonly Expr value;

            public Assign(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitAssignExpr(this);
            }
        }

        public class Binary : Expr
        {
            public readonly Expr left;
            public readonly Token @operator;
            public readonly Expr right;

            public Binary(Expr left, Token @operator, Expr right)
            {
                this.left = left;
                this.@operator = @operator;
                this.right = right;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitBinaryExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public readonly Expr expression;

            public Grouping(Expr expression)
            {
                this.expression = expression;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitGroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public readonly object value;

            public Literal(object value)
            {
                this.value = value;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitLiteralExpr(this);
            }
        }

        public class Unary : Expr
        {
            public readonly Token @operator;
            public readonly Expr right;

            public Unary(Token @operator, Expr right)
            {
                this.@operator = @operator;
                this.right = right;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitUnaryExpr(this);
            }
        }

        public class Variable : Expr
        {
            public readonly Token name;

            public Variable(Token name)
            {
                this.name = name;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitVariableExpr(this);
            }
        }
    }
}
