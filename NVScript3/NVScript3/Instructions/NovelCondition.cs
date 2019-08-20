namespace NVScript3.NVScript3.Instructions
{
    public class NovelCondition : NovelInstruction
    {
        public NovelExpressionOperand ConditionOperand { get; set; }

        public NovelExpandStack StackInstruction { get; set; }

        public int InstructionOnFalse { get; set; }

        public NovelCondition()
        {
            StackInstruction = null;
            ConditionOperand = new NovelExpressionOperand();
            InstructionOnFalse = -1;
        }
    }
}
