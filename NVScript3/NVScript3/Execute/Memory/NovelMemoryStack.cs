
using System.Collections.Generic;
using System.Linq;

namespace NVScript3.NVScript3.Execute.Memory
{
    public class NovelMemoryStack
    {
        private List<NovelVariable> Stack = new List<NovelVariable>();

        public NovelMemoryStack()
        {

        }

        public void Reset()
        {
            Stack.Clear();
        }

        public void ExpandStack(int size)
        {
            if (size > 0)
                for (int i = 0; i < size; i++)
                    Stack.Add(null);

            else if (size < 0)
                for (int i = 0; i < -size; i++)
                    Stack.RemoveAt(Stack.Count - 1);
        }

        public int GetStackSize()
        {
            return Stack.Count();
        }

        public void SetVariableAtLast(NovelVariable variable)
        {
            Stack[Stack.Count - 1] = variable;
        }

        public void SetVariableAt(int StackPointer, int offset, NovelVariable variable)
        {
            Stack[StackPointer + offset] = variable;
        }

        public NovelVariable GetVariableAt(int StackPointer, int offset)
        {
            return Stack[StackPointer + offset];
        }


    }
}
