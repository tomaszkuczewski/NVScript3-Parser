namespace NVScript3.NVScript3.Instructions
{
    public class NovelJump : NovelInstruction
    {
        public int JumpIndex { get; set; }
        public NovelJump()
        {
            JumpIndex = -1;
        }
    }
}
