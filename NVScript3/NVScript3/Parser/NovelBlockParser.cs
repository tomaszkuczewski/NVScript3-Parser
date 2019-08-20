using NVScript3.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVScript3.NVScript3.Parser
{
    public partial class NovelParser
    {

        private NovelParserBlock ReadBlock(string text, int startIndex, out int endIndex)
        {
            //Name of block header
            string blockHeader = "";

            //Create block
            NovelParserBlock block = new NovelParserBlock();

            //Setting up the end index
            endIndex = -1;

            //Found the block
            bool isBlockFound = false;

            var line = "";
            for (int i = startIndex; i < text.Length; i++)
            {
                if (text[i].Equals('{') && !isBlockFound == true)
                {
                    //Read the block header going back from index i to }, SOF, ; 
                    blockHeader = StringUtils.GetBackwardsUntil(text, i - 1, new char[] { '}', ';', '{' });

                    //Clean block header
                    block.Header = NovelParserBlock.CleanHeader(blockHeader).Trim();

                    //Getting the block size
                    var tempEnd = -1;

                    //Setting up the character number including {} in this block
                    block.CharacterCount = ReadBlockContent(text, i, out tempEnd).Length + 2;

                    //Set the block line in code
                    block.CodeLine = StringUtils.FindCount(text, 0, text.LastIndexOf(')', i), '\n') + 1;

                    //Set the flag
                    isBlockFound = true;
                }

                //Main block found
                else if (isBlockFound)
                {
                    //Add line to list
                    if (text[i] == ';')
                    {
                        block.Content.Add(new NovelParserText(StringUtils.RefactorWhiteSpaces(line), StringUtils.FindCount(text, 0, i, '\n') + 1));
                        //Reset line
                        line = "";
                    }
                    //If end of block then return it
                    else if (text[i] == '}')
                    {
                        endIndex = i;
                        return block;
                    }
                    //Add character to line buffer
                    else if (text[i] != '{')
                    {
                        line += text[i];
                    }
                    //Internal block found
                    else if (text[i] == '{')
                    {
                        //Read the internal block if found
                        int tempVal = -1;
                        var internalBlock = ReadBlock(text, i, out tempVal);

                        //Add internal block to main block
                        block.Content.Add(internalBlock);

                        //Move pointer by number of characters in internal block
                        i += internalBlock.CharacterCount;

                        //Reset line
                        line = "";

                    }
                }
            }
            return null;
        }

        private string ReadBlockContent(string text, int startIndex, out int endIndex)
        {
            //To track number of opened parentheses
            int openedParentheses = 1;

            //To escape from {
            startIndex++;

            endIndex = -1;

            if (startIndex >= text.Length)
                return null;

            //For each
            for (int i = startIndex; i < text.Length; i++)
            {
                //If opening found then increment value
                if (text[i] == '{')
                    openedParentheses++;

                //if closing parentheses are found
                if (text[i] == '}')
                {
                    //Decrement value
                    openedParentheses--;

                    //If value is equal to one means that right closing parenteses has been found
                    if (openedParentheses == 0)
                    {
                        endIndex = i;
                        return text.Substring(startIndex, i - startIndex);
                    }
                }
            }
            return null;
        }
    }
}
