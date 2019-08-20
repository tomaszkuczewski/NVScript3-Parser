using NVScript3.NVScript3.Exceptions;
using NVScript3.NVScript3.Execute.Memory;
using System;
using System.Linq;

namespace NVScript3.NVScript3.Instructions
{
    //Negation operator
    //new NovelTextOperator(102, "!"),

    //Scalar operators
    //new NovelTextOperator(101, "^"),
    //new NovelTextOperator(100, "*"),
    //new NovelTextOperator(100, "/"),
    //new NovelTextOperator(99, "+"),
    //new NovelTextOperator(99, "-"),
    //new NovelTextOperator(99, "%"),

    ////Comparison operators
    //new NovelTextOperator(98, "=="),
    //new NovelTextOperator(98, "!="),
    //new NovelTextOperator(98, ">="),
    //new NovelTextOperator(98, "<="),
    //new NovelTextOperator(98, ">"),
    //new NovelTextOperator(98, "<"),

    ////Logic operators
    //new NovelTextOperator(97, "&&"),
    //new NovelTextOperator(97, "||"),

    //new NovelTextOperator(96, ","),

    ////Support operators
    //new NovelTextOperator(2, "("),
    //new NovelTextOperator(1, ")"),
    //new NovelTextOperator(0, "="),

    public abstract class NovelExpressionOperator : NovelTerm
    {
        public Type [] ArithmeticTypes = new Type[] { typeof(int), typeof(float), typeof(double) };

        public abstract string GetName();

        public abstract int GetPriority();

        public abstract NovelVariable Operate(NovelVariable var1, 
            NovelVariable var2);

        protected bool IsString(NovelVariable v)
        {
            if (v.VariableType == typeof(string))
                return true;

            return false;
        }

        protected bool ValidateArithmetics(NovelVariable v1, NovelVariable v2)
        {
            if (IsNumeric(v1) && IsNumeric(v2))
                return true;

            return false;
        }

        protected bool ValidateLogical(NovelVariable v1, NovelVariable v2)
        {
            if (v1.VariableType == typeof(bool) &&
                v2.VariableType == typeof(bool))
                return true;

            return false;
        }

        protected bool IsNumeric(NovelVariable v)
        {
            if (v.VariableType == typeof(int) ||
                v.VariableType == typeof(float) ||
                v.VariableType == typeof(double))
                return true;

            return false;
        }

        protected Type GetReturnType(NovelVariable v1, NovelVariable v2)
        {
            var type1 = GetMaxArithmeticType(v1);
            var type2 = GetMaxArithmeticType(v2);
            var result = (type1 > type2 ? type1 : type2);
            if (result >= 0)
                return ArithmeticTypes[result];

            return typeof(int);
        }

        protected int GetMaxArithmeticType(NovelVariable v)
        {
            for(int i = 0; i < ArithmeticTypes.Count(); i++)
            {
                if (v.VariableType == ArithmeticTypes[i])
                    return i;
            }
            return -1;
        }

        protected bool GetBool(NovelVariable v)
        {
            return Convert.ToBoolean(v.Value);
        }

        protected decimal GetDecimal(NovelVariable v)
        {
            return Convert.ToDecimal(v.Value);
        }

        public override string ToString()
        {
            return GetName();
        }
    }

    public abstract class NovelArithmeticOperator : NovelExpressionOperator
    {

    }

    public abstract class NovelLogicOperator : NovelExpressionOperator
    {

    }

    class NovelFunctionStartOperator : NovelExpressionOperator
    {
        public override string GetName() { return "args{"; }

        public override int GetPriority() { return 102; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }

    class NovelFunctionEndOperator : NovelExpressionOperator
    {
        public override string GetName() { return "}args"; }

        public override int GetPriority() { return 102; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }


    class NovelNegationOperator : NovelLogicOperator
    {
        public override string GetName() { return "!"; }

        public override int GetPriority() { return 102; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }

    class NovelPowerOperator : NovelArithmeticOperator
    {
        public override string GetName() { return "^"; }

        public override int GetPriority() { return 101; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            decimal output = dec1;
            for (int i = 0; i < dec2 - 1; i++)
                output *= dec1;

            var result = Convert.ChangeType(output, GetReturnType(var1, var2));
            return new NovelVariable(result);
        }
    }

    class NovelMultiplyOperator : NovelArithmeticOperator
    {
        public override string GetName() { return "*"; }

        public override int GetPriority() { return 100; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);
            var result = Convert.ChangeType(dec1 * dec2, GetReturnType(var1, var2));
            return new NovelVariable(result);
        }
    }

    class NovelDivideOperator : NovelArithmeticOperator
    {
        public override string GetName() { return "/"; }

        public override int GetPriority() { return 100; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);
            var result = Convert.ChangeType(dec1 / dec2, GetReturnType(var1, var2));
            return new NovelVariable(result);
        }
    }

    class NovelAddOperator : NovelArithmeticOperator
    {
        public override string GetName() { return "+"; }

        public override int GetPriority() { return 99; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if(IsString(var1) || IsString(var2))
            {
                string v1, v2;
                if (IsString(var1))
                    v1 = (string)var1.Value;
                else
                    v1 = "" + GetDecimal(var1);

                if (IsString(var2))
                    v2 = (string)var2.Value;
                else
                    v2 = "" + GetDecimal(var2);

                return new NovelVariable(v1 + v2);
            }
            else if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);
            var result = Convert.ChangeType(dec1 + dec2, GetReturnType(var1, var2));
            return new NovelVariable(result);
        }
    }

    class NovelSubtractOperator : NovelArithmeticOperator
    {
        public override string GetName() { return "-"; }

        public override int GetPriority() { return 99; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);
            var result = Convert.ChangeType(dec1 - dec2, GetReturnType(var1, var2));
            return new NovelVariable(result);
        }
    }

    class NovelDivideRestOperator : NovelArithmeticOperator
    {
        public override string GetName() { return "%"; }

        public override int GetPriority() { return 99; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);
            var result = Convert.ChangeType(dec1 % dec2, GetReturnType(var1, var2));
            return new NovelVariable(result);
        }
    }

    class NovelEqualsOperator : NovelLogicOperator
    {
        public override string GetName() { return "=="; }

        public override int GetPriority() { return 98; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            var result = Convert.ChangeType(dec1.Equals(dec2), typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelDifferentThanOperator : NovelLogicOperator
    {
        public override string GetName() { return "!="; }

        public override int GetPriority() { return 98; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            var result = Convert.ChangeType(!dec1.Equals(dec2), typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelBiggerEqualOperator : NovelLogicOperator
    {
        public override string GetName() { return ">="; }

        public override int GetPriority() { return 98; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            var result = Convert.ChangeType(dec1 >= dec2, typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelSmallerEqualOperator : NovelLogicOperator
    {
        public override string GetName() { return "<="; }

        public override int GetPriority() { return 98; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            var result = Convert.ChangeType(dec1 <= dec2, typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelBiggerOperator : NovelLogicOperator
    {
        public override string GetName() { return ">"; }

        public override int GetPriority() { return 98; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            var result = Convert.ChangeType(dec1 > dec2, typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelSmallerOperator : NovelLogicOperator
    {
        public override string GetName() { return "<"; }

        public override int GetPriority() { return 98; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateArithmetics(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetDecimal(var1);
            var dec2 = GetDecimal(var2);

            var result = Convert.ChangeType(dec1 < dec2, typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelLogicAndOperator : NovelLogicOperator
    {
        public override string GetName() { return "&&"; }

        public override int GetPriority() { return 97; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateLogical(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetBool(var1);
            var dec2 = GetBool(var2);

            var result = Convert.ChangeType(dec1 && dec2, typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelLogicOrOperator : NovelLogicOperator
    {
        public override string GetName() { return "||"; }

        public override int GetPriority() { return 97; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            if (!ValidateLogical(var1, var2))
                throw new NovelException("Not available operation (" + GetName() + ")", "temp", 0);

            var dec1 = GetBool(var1);
            var dec2 = GetBool(var2);

            var result = Convert.ChangeType(dec1 || dec2, typeof(bool));
            return new NovelVariable(result);
        }
    }

    class NovelCommaOperator : NovelExpressionOperator
    {
        public override string GetName() { return ","; }

        public override int GetPriority() { return 96; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }

    class NovelOpenCurvedParenthesesOperator : NovelExpressionOperator
    {
        public override string GetName() { return "("; }

        public override int GetPriority() { return 2; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }

    class NovelCloseCurvedParenthesesOperator : NovelExpressionOperator
    {
        public override string GetName() { return ")"; }

        public override int GetPriority() { return 1; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }

    class NovelAssignOperator : NovelExpressionOperator
    {
        public override string GetName() { return "="; }

        public override int GetPriority() { return 0; }

        public override NovelVariable Operate(NovelVariable var1, NovelVariable var2)
        {
            return null;
        }
    }
}
