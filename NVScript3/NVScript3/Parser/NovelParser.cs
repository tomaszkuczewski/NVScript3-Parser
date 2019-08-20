using NVScript3.NVScript3.Execute;
using NVScript3.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVScript3.NVScript3.Parser
{
    public class NovelParserPhrase
    {
        public int CodeLine { get; set; }
    }

    public class NovelParserBlock : NovelParserPhrase
    {
        public NovelParserBlock()
        {
            Content = new List<NovelParserPhrase>();
            Header = "";
            CharacterCount = 0;
            CodeLine = 0;
        }
        public string Header { get; set; }
        public List<NovelParserPhrase> Content { get; set; }
        public int CharacterCount { get; set; }
        public static string CleanHeader(string header)
        {
            header = header.Replace("\t", "");
            header = header.Replace("\r\n", "");
            header = header.Replace("{", "");
            return header;
        }
    }

    public class NovelParserText : NovelParserPhrase
    {
        public NovelParserText(string text, int line)
        {
            Instruction = text;
            CodeLine = line;
        }

        public string Instruction { get; set; }
    }

    public partial class NovelParser
    {
        private int CharacterPointer { get; set; }

        private string ParsedText { get; set; }

        private string ParsedFile { get; set; }

        private int ParsedLine { get; set; }

        public NovelScript ParseText(string fileName, string scriptCode, NovelFunctionPool funcPool = null)
        {

            //Setup variables
            CharacterPointer = 0;

            //Setting up parsed text
            ParsedText = scriptCode;

            //Setting up parsed file name
            ParsedFile = fileName;

            //Setting up parsedline
            ParsedLine = -1;

            //Main block of grouped code
            var mainBlock = new NovelParserBlock();
            
            //For reading purposes
            int endIndex = 0;
            NovelParserBlock block = null;

            //Read all the blocks in file
            while( (block = ReadBlock(scriptCode, endIndex, out endIndex)) != null )
            {
                mainBlock.Content.Add(block);
            }

            return ParseScript(fileName, mainBlock, funcPool);
        }

    }
}
