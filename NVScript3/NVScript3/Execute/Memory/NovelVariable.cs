using System;

namespace NVScript3.NVScript3.Execute.Memory
{
    public class NovelVariable
    {
        public object Value { get; set; }

        public Type VariableType { get; set; }

        public NovelVariable(object variable)
        {
            Value = variable;
            VariableType = variable.GetType();
        }

        public override string ToString()
        {
            return ""+Value;
        }

    }
}
