using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVScript3.Utils
{
    public class StringUtils
    {
        public static String GetBetween(String text, String start, String end)
        {
            int p1 = text.IndexOf(start) + start.Length;
            int p2 = text.IndexOf(end, p1);

            if (end == "") return (text.Substring(p1));
            else return text.Substring(p1, p2 - p1);
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string GetBackwardsUntil(string text, int startIndex, char[] characters)
        {
            string result = "";
            for(int i = startIndex; i >= 0; i--)
            {
                for(int c = 0; c < characters.Length; c++)
                {
                    if (characters[c] == text[i])
                    {
                        return Reverse(result);
                    }
                }
                result += text[i];
            }
            return Reverse(result);
        }

        public static string RefactorWhiteSpaces(string text)
        {
            text = text.Replace("\r\n", " ");
            text = text.Replace("\t", " ");
            text = text.Trim();
            return text;
        }

        public static int FindCount(string text, int startIndex, int length, char character)
        {
            int count = 0;
            for(int i = startIndex; i < startIndex + length; i++)
            {
                if (text[i] == character)
                    count++;
            }
            return count;
        }

        public static string RemoveSpaces(string text)
        {
            //Remove any kind of white spaces between characters
            text = text.Replace(" ", "");
            text = text.Replace("\t", "");
            return text;
        }

        public static bool IsEmpty(string name)
        {
            if (name.Length == 0)
                return true;

            return false;
        }

        //public static bool IsEnclosed(string text, int index, char character)
        //{
        //    int endIndex = text.IndexOf

        //    return false;
        //}
    }
}
