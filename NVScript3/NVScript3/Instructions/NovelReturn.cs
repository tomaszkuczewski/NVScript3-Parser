namespace NVScript3.NVScript3.Instructions
{
    public class NovelReturn : NovelInstruction
    {
        public NovelExpressionOperand ReturnOperand{ get; set; }

        public NovelReturn(NovelExpressionOperand operand)
        {
            ReturnOperand = operand;
        }
    }
}
