using System.Collections.Generic;

namespace NLox {
    public abstract class Stmt
    {
        public abstract TR accept<TR>(IVisitor<TR> visitor);

        public interface IVisitor<TR>
        {
            TR visitBlockStmt(Block stmt);
            TR visitExpressionStmt(Expression stmt);
            TR visitPrintStmt(Print stmt);
            TR visitVarStmt(Var stmt);
        }

        public class Block : Stmt
        {
            public readonly List<Stmt> statements;

            public Block(List<Stmt> statements)
            {
                this.statements = statements;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitBlockStmt(this);
            }
        }

        public class Expression : Stmt
        {
            public readonly Expr expression;

            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitExpressionStmt(this);
            }
        }

        public class Print : Stmt
        {
            public readonly Expr expression;

            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitPrintStmt(this);
            }
        }

        public class Var : Stmt
        {
            public readonly Token name;
            public readonly Expr initializer;

            public Var(Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitVarStmt(this);
            }
        }
    }
}
