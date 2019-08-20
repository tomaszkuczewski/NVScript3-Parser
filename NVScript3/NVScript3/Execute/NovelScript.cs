using NVScript3.NVScript3.Instructions;
using System.Collections.Generic;

namespace NVScript3.NVScript3.Execute
{

    public class NovelScript
    {

        public List<NovelFunctionInformationEntry> FunctionList { get; set; }

        public List<NovelInstruction> Instructions { get; set; }

        public NovelFunctionPool DelegateFunctionList { get; set; }

        public string Name { get; set; }

        public NovelScript(string name)
        {
            Name = name;
            Instructions = new List<NovelInstruction>();
            FunctionList = new List<NovelFunctionInformationEntry>();
            DelegateFunctionList = null;
        }
    }

}
