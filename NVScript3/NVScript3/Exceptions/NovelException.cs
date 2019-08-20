using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NVScript3.NVScript3.Exceptions
{
    /// <summary>
    /// Exception for NVScript
    /// </summary>
    public class NovelException : Exception
    {
        /// <summary>
        /// Contains the string that shows the place of error eg. "          ^"
        /// </summary>
        public string MarkString { get; set; }

        /// <summary>
        /// Line content with possible error
        /// </summary>
        public string LineContent { get; set; }

        /// <summary>
        /// If the exception should be treated as warning
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// Constructor of NVException based on base class Exception
        /// </summary>
        /// <param name="message">Message of exception</param>
        /// <param name="file">File in which exception occured</param>
        /// <param name="line">Line in which exception occured</param>
        /// <param name="character">Possible index in string where is the error</param>
        public NovelException(string message, string file, int lineIndex/*, string text, int character*/) :
            base( /*(warning == true ? "[WARNING] " : "[ERROR] ") +*/ message + " |File: " + file + "|Line: " + lineIndex + "|")
        {
            //Default variables values
            //IsWarning = warning;
            //MarkString = "";
            //LineContent = "";

            ////If line content isn't null then set it
            //if (text != null)
            //{
            //    //Split whole text to lines
            //    var lines = text.Split('\n');

            //    //Get the right line and remove \r character
            //    LineContent = "\n" + lines[lineIndex - 1].Replace("\r", "").Replace("\t", " ");
            //}

            ////If error is on certain index then generate mark string that will show the place of error under the error string
            //if (character > -1)
            //{
            //    MarkString += "\n";

            //    for (int i = 0; i <= character - 1; i++)
            //        MarkString += " ";

            //    MarkString += "^";
            //}
        }
        
        ///// <summary>
        ///// Merges message of exception and marking (if available) to show where is possible error
        ///// </summary>
        ///// <returns>Return full string of exception message and marking</returns>
        //public string GetFullMessage()
        //{
        //    return Message + LineContent + MarkString;
        //}
    }
}
