namespace NVScript3.NVScript3.Instructions
{
    public class NovelExpandStack : NovelInstruction
    {
        public int ExpandSize { get; set; }

        public NovelExpandStack(int size)
        {
            ExpandSize = size;
        }

        public override string ToString()
        {
            return "Expand " + ExpandSize;
        }
    }
}
