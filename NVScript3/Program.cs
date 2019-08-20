using NVScript3.NVScript3.Execute;
using NVScript3.NVScript3.Execute.Executor;
using NVScript3.NVScript3.Parser;
using System;
using System.IO;

namespace NVScript3
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText("kitchen.txt");

            NovelFunctionPool delegates = new NovelFunctionPool();

            delegates.AddFunction(new Func<int, string, int>(C1) );
            delegates.AddFunction(new Func<int, int>(C2));
            delegates.AddFunction(new Func<int, int>(C3));
            delegates.AddFunction(new Func<int, int>(C4));
            delegates.AddFunction(new Func<int, int>(C5));

            var script = new NovelParser().ParseText("kitchen.txt", text, delegates);
            var executor = new NovelExecutor(script);

            executor.Reset();
            executor.CallScriptFunction("abc", null);
            executor.ContinueExecution();
        }

        public static int C1(int a, string text)
        {
            return a - 1; 
        }
        public static int C2(int a)
        {
            return a - 1;
        }
        public static int C3(int a)
        {
            return a - 1;
        }
        public static int C4(int a)
        {
            return a - 1;
        }
        public static int C5(int a)
        {
            return a - 1;
        }
    }
}
