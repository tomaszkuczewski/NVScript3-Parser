using System.Collections.Generic;

namespace NVScript3.NVScript3.Execute
{
    public class NovelFunctionInformationEntry
    {
        public string Name;

        public List<string> ParameterNameList { get; set; }

        public int Offset { get; set; }

        public NovelFunctionInformationEntry(string name, List<string> parameters, int offset)
        {
            Name = name;
            ParameterNameList = parameters;
            Offset = offset;
        }
    }
}
