using System.Collections.Generic;

namespace NLox {
    public abstract class Stmt
    {
        public abstract TR accept<TR>(IVisitor<TR> visitor);

        public interface IVisitor<TR>
        {
            TR visitBlockStmt(Block stmt);
            TR visitExpressionStmt(Expression stmt);
            TR visitIfStmt(If stmt);
            TR visitPrintStmt(Print stmt);
            TR visitVarStmt(Var stmt);
            TR visitWhileStmt(While stmt);
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

        public class If : Stmt
        {
            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;

            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitIfStmt(this);
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

        public class While : Stmt
        {
            public readonly Expr condition;
            public readonly Stmt body;

            public While(Expr condition, Stmt body)
            {
                this.condition = condition;
                this.body = body;
            }

            public override TR accept<TR>(IVisitor<TR> visitor)
            {
                return visitor.visitWhileStmt(this);
            }
        }
    }
}
