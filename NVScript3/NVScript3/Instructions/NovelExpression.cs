using System.Collections.Generic;

namespace NVScript3.NVScript3.Instructions
{
    public interface NovelTerm
    {
    }

    public class NovelExpressionOperand : NovelTerm
    {
    }



    public class NovelExpressionLiteral : NovelExpressionOperand
    {
        public object Literal { get; set; }

        public NovelExpressionLiteral(object literal)
        {
            Literal = literal;
        }

        public override string ToString()
        {
            return "" + Literal;
        }
    }

    public class NovelExpressionVariable : NovelExpressionOperand
    {
        public string Name { get; set; }

        public int StackOffset { get; set; }

        public NovelExpressionVariable(string name, int stackOffset)
        {
            Name = name;
            StackOffset = stackOffset;
        }
        public override string ToString()
        {
            return Name;
        }
    }


    public class NovelExpressionFunctionCall : NovelExpressionOperand
    {
        public string Name;
        public int Offset;

        public List<NovelExpressionOperand> Arguments { get; set; }

        public bool IsDelegated { get; set; }

        public NovelExpressionFunctionCall(string name, int offset, bool delegated)
        {
            Name = name;
            Offset = offset;
            Arguments = new List<NovelExpressionOperand>();
            IsDelegated = delegated;
        }

        public override string ToString()
        {
            string buffer = "(";
            foreach (var arg in Arguments)
                buffer += arg + ",";
            buffer += ")";
            return Name + buffer;
        }
    }

    public class NovelExpression : NovelInstruction
    {
        public List<NovelTerm> Term { get; set; }

        public NovelExpression()
        {
            Term = new List<NovelTerm>();
        }

        public override string ToString()
        {
            string buffer = "";
            foreach(var part in Term)
                buffer += " " + part;

            return buffer;
        }
    }
}
