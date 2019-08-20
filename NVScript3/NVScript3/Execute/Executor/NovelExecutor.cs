using NVScript3.NVScript3.Exceptions;
using NVScript3.NVScript3.Instructions;
using NVScript3.NVScript3.Execute.Memory;

using System.Collections.Generic;
using System.Linq;

namespace NVScript3.NVScript3.Execute.Executor
{
    public class NovelExecutor
    {

        public NovelMemoryStack Stack = new NovelMemoryStack();

        public NovelScript Script { get; set; }

        public int InstructionPointer { get; set; }

        public int StackPointer { get; set; }

        public bool IsExecuting { get; set; }

        public NovelExecutor(NovelScript script)
        {
            IsExecuting = false;
            Script = script;
            InstructionPointer = 0;
        }

        public void Reset()
        {
            Stack.Reset();
            InstructionPointer = 0;
            StackPointer = 0;
        }

        public void CallScriptFunction(string name, object [] args)
        {
            int argsCount = 0;
            if (args != null)
                argsCount = args.Length;

            var function = Script.FunctionList.Where(x => x.Name.Equals(name) &&
            x.ParameterNameList.Count.Equals(argsCount)).FirstOrDefault();

            if (function != null)
            {
                //Save pointer states
                var lastInstructionPtr = new NovelVariable(InstructionPointer);
                var lastStackPtr = new NovelVariable(StackPointer);

                var stackSize = Stack.GetStackSize();
                //First expand the stack
                Stack.ExpandStack(3);
                //Push the variables on stack
                Stack.SetVariableAt(stackSize, 1, lastInstructionPtr);
                Stack.SetVariableAt(stackSize, 2, lastStackPtr);
                //Set new stack poitner
                StackPointer = Stack.GetStackSize();
                //Set new instruction pointer
                InstructionPointer = function.Offset;
                //Expand by the number of arguments
                Stack.ExpandStack(function.ParameterNameList.Count);
                //Fill arguments
                for (int i = 0; i < argsCount; i++)
                    Stack.SetVariableAt(StackPointer, i, new NovelVariable(args[i]));
            }
            else
                throw new NovelException("Could not find function " + name + " with given signature", Script.Name, 0);
        }

        public void ExecuteNext()
        {
            if (InstructionPointer < Script.Instructions.Count())
                ExecuteInstruction(Script.Instructions[InstructionPointer]);
            else
                IsExecuting = false;
        }

        public void ContinueExecution()
        {
            IsExecuting = true;

            while (IsExecuting == true)
                ExecuteInstruction(Script.Instructions[InstructionPointer]);
        }

        private void ExecuteInstruction(NovelInstruction instruction)
        {
            if(instruction is NovelExpandStack)
            {
                Stack.ExpandStack((instruction as NovelExpandStack).ExpandSize);
            }
            else if(instruction is NovelJump)
            {
                //IP fix
                InstructionPointer += (instruction as NovelJump).JumpIndex - 1;
            }
            else if(instruction is NovelExpression)
            {
                //IP fix
                ExecuteExpression(instruction as NovelExpression);
            }
            else if(instruction is NovelCondition)
            {
                //IP fix
                ExecuteCondition(instruction as NovelCondition);
            }
            else if(instruction is NovelReturn)
            {
                ExecuteReturn(instruction as NovelReturn);
            }

            InstructionPointer++;
        }

        private void ExecuteCondition(NovelCondition condition)
        {
            var operandValue = OperandToValue(condition.ConditionOperand);
            if(operandValue.VariableType == typeof(bool))
            {
                var boolValue = (bool) operandValue.Value;
                //If condition is true
                if(boolValue)
                {
                    //Clean the stack
                    if (condition.StackInstruction != null)
                        Stack.ExpandStack(condition.StackInstruction.ExpandSize);
                }
                //If false
                else
                {
                    //Move by the offset of instruction on false
                    InstructionPointer += condition.InstructionOnFalse;
                    //Clean the stack
                    Stack.ExpandStack(condition.StackInstruction.ExpandSize);
                    //Because IP is incremented each tick of loop it needs to be subtracted by 1
                    InstructionPointer--;
                }
            }
            else
            {
                //TODO error
            }
        }

        private void ExecuteReturn(NovelReturn returnValue)
        {
            var operandResult = OperandToValue(returnValue.ReturnOperand);
            //Set return type
            Stack.SetVariableAt(StackPointer, -3, new NovelVariable(operandResult.Value));

            var lastInstructionPtr = (int) Stack.GetVariableAt(StackPointer, -2).Value;
            var lastStackPtr = (int)Stack.GetVariableAt(StackPointer, -1).Value;

            //Clear the stack until stack pointer
            while (StackPointer != Stack.GetStackSize())
                Stack.ExpandStack(-1);

            //Remove instruction pointer and stack pointer
            Stack.ExpandStack(-2);

            if (Stack.GetStackSize() == 1)
                IsExecuting = false;

            //Set last pointers
            InstructionPointer = lastInstructionPtr;
            StackPointer = lastStackPtr;
        }

        private void ExecuteExpression(NovelExpression expression)
        {
            if(expression.Term.Count == 5)
            {
                var result = expression.Term[0] as NovelExpressionOperand;
                var operand1 = expression.Term[1] as NovelExpressionOperand;
                var operand2 = expression.Term[2] as NovelExpressionOperand;
                var oper = expression.Term[3] as NovelExpressionOperator;
                var assignOper = expression.Term[4] as NovelExpressionOperator;
                //Assuming that assign operator is =
                if(assignOper is NovelAssignOperator)
                {
                    var var1 = OperandToValue(operand1);
                    var var2 = OperandToValue(operand2);

                    //TODO EXECUTION TIME EXCEPTIONS
                    if (var1 == null)
                        throw new NovelException("Object " + var1.ToString() + " is undefined", "", 0);

                    if (var2 == null)
                        throw new NovelException("Object " + var2.ToString() + " is undefined", "", 0);

                    var varRes = oper.Operate(var1, var2);
                    Stack.SetVariableAt(StackPointer,
                        (result as NovelExpressionVariable).StackOffset,
                        varRes);
                }
            }
            else if(expression.Term.Count == 3)
            {
                var result = expression.Term[0] as NovelExpressionOperand;
                var operand1 = expression.Term[1] as NovelExpressionOperand;
                var assignOper = expression.Term[2] as NovelExpressionOperator;
                //Assuming that assign operator is =
                if (assignOper is NovelAssignOperator)
                {
                    var var1 = OperandToValue(operand1);
                    //TODO EXECUTION TIME EXCEPTIONS
                    if (var1 == null)
                        throw new NovelException("Object " + var1.ToString() + " is undefined", "", 0);

                    Stack.SetVariableAt(StackPointer,
                        (result as NovelExpressionVariable).StackOffset,
                        var1);
                }
            }
            else if(expression.Term.Count == 1)
            {
                if(expression.Term[0] is NovelExpressionFunctionCall)
                {
                    var exprFunc = expression.Term[0] as NovelExpressionFunctionCall;
                    var argList = new List<object>();
                    foreach(var arg in exprFunc.Arguments)
                        argList.Add(OperandToValue(arg).Value);

                    if(!exprFunc.IsDelegated)
                    {
                        CallScriptFunction(exprFunc.Name, argList.ToArray());
                        InstructionPointer--;
                    }
                    else
                    {
                        Stack.ExpandStack(1);
                        var result = Script.DelegateFunctionList.Call(exprFunc.Offset, argList.ToArray());

                        //Possible for Actions that returns null
                        if (result == null)
                        {
                            result = new int();
                            result = 0;
                        }

                        Stack.SetVariableAtLast(new NovelVariable(result));
                    }
                }
            }
        }

        private NovelVariable OperandToValue(NovelExpressionOperand operand)
        {
            if (operand is NovelExpressionLiteral)
                return new NovelVariable((operand as NovelExpressionLiteral).Literal);
            else if (operand is NovelExpressionVariable)
                return Stack.GetVariableAt(StackPointer, (operand as NovelExpressionVariable).StackOffset);

            return null;
        }

    }
}
