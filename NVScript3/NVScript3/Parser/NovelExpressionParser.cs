using NVScript3.NVScript3.Exceptions;
using NVScript3.NVScript3.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NVScript3.NVScript3.Parser
{
    public partial class NovelParser
    {
        class NovelTextOperator
        {
            public int Priority { get; set; }

            public string Name { get; set; }

            public NovelTextOperator(int priority, string name)
            {
                Priority = priority;
                Name = name;
            }
        }

        private List<NovelExpressionOperator> Operators = new List<NovelExpressionOperator>
        (
            new NovelExpressionOperator[] {
                //Negation operator
                new NovelNegationOperator(),

                //Scalar operators
                new NovelPowerOperator(),
                new NovelMultiplyOperator(),
                new NovelDivideOperator(),
                new NovelAddOperator(),
                new NovelSubtractOperator(),
                new NovelDivideRestOperator(),

                //Comparison operators
                new NovelEqualsOperator(),
                new NovelDifferentThanOperator(),
                new NovelBiggerEqualOperator(),
                new NovelSmallerEqualOperator(),
                new NovelBiggerOperator(),
                new NovelSmallerOperator(),

                //Logic operators
                new NovelLogicAndOperator(),
                new NovelLogicOrOperator(),

                //Comma operator
                new NovelCommaOperator(),

                //Support operators
                new NovelOpenCurvedParenthesesOperator(),
                new NovelCloseCurvedParenthesesOperator(),
                new NovelAssignOperator(),
            }
        );

        private enum ParenthesesType
        {
            Normal,
            Function
        }

        private Stack<ParenthesesType> parenthesesType = new Stack<ParenthesesType>();
        private Stack<NovelExpressionOperator> OperatorStack = new Stack<NovelExpressionOperator>();

        public List<NovelInstruction> ParseExpression(string text)
        {
            //Trim the expression
            text = text.Trim();

            //Create empty expression object
            NovelExpression novelExpression = new NovelExpression();

            //Declare instructions
            List<NovelInstruction> instructions = new List<NovelInstruction>();

            //For RPN algorithm purpose
            string output = "";
            string buffer = "";

            for (int i = 0; i < text.Length; i++)
            {
                //Check if the i is pointing at operator name
                var operatorIndex = IsOperator(text, i);

                //operatorIndex = -1 means that its regular character
                if (operatorIndex >= 0)
                {             
                    //Build expression here based on whats in buffer
                    if(!buffer.Equals(""))
                    {
                        if (IsDeclaredVariable(buffer))
                        {
                            buffer = buffer.Substring(3);

                            if(!Variables.ContainsVariable(buffer))
                            {
                                instructions.Add(new NovelExpandStack(1));
                                Variables.AddVaraible(buffer);
                            }
                            //TODO functions name
                            else
                                throw new NovelException("Variable or function name " + buffer + " already exists.", ParsedFile, ParsedLine);
                        }

                        //Handle the operands
                        novelExpression = HandleOperand(novelExpression, buffer);

                        //Push variable to the output
                        output += buffer;
                        buffer = "";
                    }

                    //Get the new operator
                    var op = Operators[operatorIndex];

                    //Parentheses close operator
                    if(op is NovelCloseCurvedParenthesesOperator)
                    {
                        //Pop all operators till (
                        while(OperatorStack.Count() > 0)
                        {
                            var opFromStack = OperatorStack.Pop();
                            if (opFromStack is NovelOpenCurvedParenthesesOperator)
                            {
                                var parentheses = parenthesesType.Pop();

                                if(parentheses == ParenthesesType.Function)
                                    novelExpression.Term.Add(new NovelFunctionEndOperator());

                                break;
                            }
                            else
                            {
                                if (!(opFromStack is NovelOpenCurvedParenthesesOperator ||
                                       opFromStack is NovelCloseCurvedParenthesesOperator))
                                    novelExpression.Term.Add(opFromStack);
                            }
                        }
                    }
                    //If every other operator
                    else
                    {
                        //Iteratore over OperatorStack
                        while (OperatorStack.Count() > 0)
                        {
                            //If priority of new operator is lower or equal pop stack operators
                            //
                            if (op.GetPriority() <= OperatorStack.First().GetPriority()
                                && !(op is NovelOpenCurvedParenthesesOperator))
                            {
                                var opFromStack = OperatorStack.Pop();

                                if (!(opFromStack is NovelOpenCurvedParenthesesOperator ||
                                       opFromStack is NovelCloseCurvedParenthesesOperator))
                                    novelExpression.Term.Add(opFromStack);
                            }
                            else
                                break;
                        }
                    }

                    //Now put the operator on the stack if not close curved
                    if( !(op is NovelCloseCurvedParenthesesOperator) )
                    {
                        //If previous term part was function call
                        if( novelExpression.Term.Count >= 1 &&
                            op is NovelOpenCurvedParenthesesOperator && 
                            novelExpression.Term[novelExpression.Term.Count-1] is NovelExpressionFunctionCall)
                        {
                            parenthesesType.Push(ParenthesesType.Function);
                            novelExpression.Term.Add(new NovelFunctionStartOperator());
                        }
                        //If normal paretheses
                        else if(op is NovelOpenCurvedParenthesesOperator)
                        {
                            parenthesesType.Push(ParenthesesType.Normal);
                        }

                        OperatorStack.Push(op);
                    }

                    //Move pointer by a length of operator
                    i += op.GetName().Length - 1;
                }
                //If its not an operator
                else
                {
                    if (text[i].Equals(' '))
                        continue;
                    else if (text[i].Equals('\t'))
                        continue;
                    else if (text[i].Equals('\r'))
                        continue;
                    else if (text[i].Equals('\n'))
                        continue;

                    //If its a quote
                    if (text[i].Equals('\"'))
                    {
                        int len = text.IndexOf('\"', i + 1) - i + 1;
                        buffer += text.Substring(i, len);
                        i += len - 1;
                    }
                    //If its not add single character
                    else
                        buffer += text[i];
                }    
            }

            //Empty buffer first
            if (!buffer.Equals(""))
                novelExpression = HandleOperand(novelExpression, buffer);
            
            //Empty operator stack
            while (OperatorStack.Count() > 0)
            {
                var opFromStack = OperatorStack.Pop();
                if (!(opFromStack is NovelOpenCurvedParenthesesOperator ||
                       opFromStack is NovelCloseCurvedParenthesesOperator))
                    novelExpression.Term.Add(opFromStack);
            }

            Variables.AddScope();
            int reservedVariables = Variables.GetStackVariables();
            while (novelExpression.Term.Count > 0)
            {
                novelExpression = UnfoldOperations(novelExpression, ref instructions);
                novelExpression = UnfoldArguments(novelExpression, ref instructions);
                //For single variables or literals
                if (novelExpression.Term.Count == 1)
                {
                    var newExpression = new NovelExpression();
                    newExpression.Term.Add(novelExpression.Term[0]);
                    instructions.Add(newExpression);
                    novelExpression.Term.Clear();
                }
            }
            
            //How many of temporary variables expression has
            reservedVariables = Variables.GetStackVariables() - reservedVariables;

            //Shrink stack to remove them
            if(reservedVariables > 0)
                instructions.Add(new NovelExpandStack(-reservedVariables));

            Variables.RemoveScope();

            foreach (var instruction in instructions)
            {
                string buff = "";
                buff += " " + instruction;
                Console.WriteLine(buff);
            }
            Console.WriteLine("");

            return instructions;
        }

        private NovelExpression HandleOperand(NovelExpression expression, string operand)
        {
            int index = -1;
            if (IsVariable(operand))
            {
                expression.Term.Add(new NovelExpressionVariable(operand, Variables.GetStackIndex(operand)));
            }
            else if (IsFunction(operand))
            {
                //TODO update offset???
                expression.Term.Add(new NovelExpressionFunctionCall(operand, 0, false));
            }
            else if((index = IsDelegateFunction(operand)) >= 0)
            {
                expression.Term.Add(new NovelExpressionFunctionCall(operand, index, true));
            }
            else if (IsLiteral(operand))
            {
                expression.Term.Add(new NovelExpressionLiteral(GetLiteral(operand)));
            }
            else if (IsString(operand))
            {
                expression.Term.Add(new NovelExpressionLiteral(operand.Substring(1, operand.Count() - 2)));
            }
            else
            {
                throw new NovelException("Undefined object " + operand + ".", ParsedFile, ParsedLine);
            }
            return expression;
        }

        private NovelExpression UnfoldOperations(NovelExpression expression, ref List<NovelInstruction> instructions)
        {
            //Get the term
            var term = expression.Term;

            //Look for 
            for(int i = 0; i < term.Count; i++)
            {
                var part = term[i];
                //Look for operator different 
                //comma
                //fnc start
                //fnc end
                //fnc call
                if(part is NovelExpressionOperator)
                {
                    if( !(part is NovelExpressionFunctionCall) 
                        && !(part is NovelCommaOperator) 
                        && !(part is NovelFunctionStartOperator)
                        && !(part is NovelFunctionEndOperator) )
                    {
                        //If its one argument operator
                        if (part is NovelNegationOperator &&
                            term[i - 1] is NovelExpressionOperand)
                        {
                            var operand = term[i - 1] as NovelExpressionOperand;
                            var lastStackOperator = Variables.GetStackVariables();
                            var temporaryVariable = new NovelExpressionVariable("temp_" + lastStackOperator, lastStackOperator);
                            Variables.AddVaraible(temporaryVariable.Name);

                            instructions.Add(new NovelExpandStack(1));
                            var newExpression = new NovelExpression();
                            newExpression.Term.Add(temporaryVariable);
                            newExpression.Term.Add(operand);
                            newExpression.Term.Add(part);
                            newExpression.Term.Add(new NovelAssignOperator());
                            instructions.Add(newExpression);
                            //Replace last operation with temporary variable
                            expression.Term.RemoveAt(i - 1);
                            expression.Term.RemoveAt(i - 1);
                            //Don't put temporary variable to expression if its empty
                            if(expression.Term.Count > 0)
                                expression.Term.Insert(i - 1, temporaryVariable);

                            i -= 1;
                        }
                        //Get the two operands if its not negation
                        else
                        {
                            //If its the end of expression
                            if( part is NovelAssignOperator && 
                                expression.Term.Count == 3 && 
                                term[i - 1] is NovelExpressionOperand &&
                                term[i - 2] is NovelExpressionOperand)
                            {
                                var newExpression = new NovelExpression();
                                newExpression.Term.Add(term[i - 2]);
                                newExpression.Term.Add(term[i - 1]);
                                newExpression.Term.Add(part);
                                instructions.Add(newExpression);
                                expression.Term.Clear();
                            }
                            //If both previous parts in term are operands
                            else if (
                                term[i - 1] is NovelExpressionOperand &&
                                term[i - 2] is NovelExpressionOperand)
                            {
                                var operandLeft = term[i - 2] as NovelExpressionOperand;
                                var operandRight = term[i - 1] as NovelExpressionOperand;

                                var lastStackOperator = Variables.GetStackVariables();
                                var temporaryVariable = new NovelExpressionVariable("temp_" + lastStackOperator, lastStackOperator);
                                Variables.AddVaraible(temporaryVariable.Name);

                                instructions.Add(new NovelExpandStack(1));
                                //Create new operation of two operands
                                //and assign the result to temporary variable
                                var newExpression = new NovelExpression();
                                newExpression.Term.Add(temporaryVariable);
                                newExpression.Term.Add(operandLeft);
                                newExpression.Term.Add(operandRight);
                                newExpression.Term.Add(part);
                                newExpression.Term.Add(new NovelAssignOperator());
                                instructions.Add(newExpression);
                                //Replace last operation with temporary variable
                                expression.Term.RemoveAt(i - 2);
                                expression.Term.RemoveAt(i - 2);
                                expression.Term.RemoveAt(i - 2);
                                if (expression.Term.Count > 0)
                                    expression.Term.Insert(i - 2, temporaryVariable);

                                i -= 2;
                            }
                        }
                    }
                }
            }
            return expression;
        }

        private NovelExpression UnfoldArguments(NovelExpression expression, ref List<NovelInstruction> instructions)
        {
            //Get the term
            var term = expression.Term;
            for(int i = 0; i < term.Count; i++)
            {
                int functionEndIndex = -1;
                int functionElements = 2;
                //Look for function start operator
                if(term[i] is NovelFunctionStartOperator && term[i - 1] is NovelExpressionFunctionCall)
                {
                    //Check if inside function are only comma characters
                    bool onlyCommas = true;
                    for(int j = i + 1; j < term.Count; j++)
                    {
                        var test = term[j];
                        functionElements++;

                        //Read until novel function end
                        if (term[j] is NovelFunctionEndOperator)
                        {
                            functionEndIndex = j;
                            break;
                        }

                        //If its an operator
                        if ( term[j] is NovelExpressionOperator )
                        {
                            //If its different than comma
                            if( !(term[j] is NovelCommaOperator) )
                            {
                                onlyCommas = false;
                                break;
                            }
                        }
                    }

                    if(onlyCommas)
                    {
                        //Fill function with arguments
                        var function = term[i - 1] as NovelExpressionFunctionCall;
                        for(int j = i; j < functionEndIndex; j++)
                        {
                            if (term[j] is NovelExpressionOperand)
                                function.Arguments.Add(term[j] as NovelExpressionOperand);
                        }
                        //TODO
                        //here i have number of arguments in function
                        //so i can check if its the right number
                        int newIndex = ParsedScript.DelegateFunctionList.
                            ContainsFunction(function.Name, function.Arguments.Count);

                        //If found new delegated function
                        if (newIndex > -1)
                        {
                            function.Offset = newIndex;
                        }
                        else
                            throw new NovelException("Could not find delegated method " 
                                + function.Name + " with " + function.Arguments.Count() +
                                " arguments.", "", 0);
                            
                        //Now create new expression
                        var lastStackOperator = Variables.GetStackVariables();
                        var temporaryVariable = new NovelExpressionVariable("temp_" + lastStackOperator, lastStackOperator);
                        Variables.AddVaraible(temporaryVariable.Name);

                        //instructions.Add(new NovelExpandStack(1));
                        var newExpression = new NovelExpression();
                        //newExpression.Term.Add(temporaryVariable);
                        newExpression.Term.Add(function);
                        //newExpression.Term.Add(new NovelAssignOperator());
                        instructions.Add(newExpression);

                        //Remove from expression 
                        for (int j = 0; j < functionElements; j++)
                            expression.Term.RemoveAt(i - 1);

                        if (expression.Term.Count > 0)
                            expression.Term.Insert(i - 1, temporaryVariable);

                        i -= 1;
                    }
                }
            }
            return expression;
        }
        
        private bool IsString(string name)
        {
            if (name.IndexOf('\"') == 0 && name.IndexOf('\"', 1) == name.Length - 1)
                return true;

            return false;
        }

        private int IsDelegateFunction(string name)
        {
            if (ParsedScript.DelegateFunctionList != null)
                return ParsedScript.DelegateFunctionList.ContainsFunction(name);
            else
                return -1;
        }

        private int UpdateDelegateFunction(string name, int argsCount)
        {
            if (ParsedScript.DelegateFunctionList != null)
                return ParsedScript.DelegateFunctionList.ContainsFunction(name, argsCount);
            else
                return -1;
        }

        private bool IsLiteral(string name)
        {
            decimal temp = 0;
            return decimal.TryParse(name, out temp);
        }

        private bool IsVariable(string name)
        {
            return Variables.ContainsVariable(name);
        }

        private bool IsDeclaredVariable(string name)
        {
            if (name.IndexOf("var") == 0)
                return true;

            return false;
        }

        private bool IsFunction(string name)
        {
            foreach(var function in ParsedScript.FunctionList)
            {
                if (function.Name.Equals(name))
                    return true;
            }
            return false;
        }

        private int IsOperator(string text, int startIndex)
        {
            for(int i = 0; i < Operators.Count; i++)
            {
                if (text.IndexOf(Operators[i].GetName(), startIndex) - startIndex == 0)
                    return i;
            }
            return -1;
        }

        private object GetLiteral(string text)
        {
            int temp = 0;
            if (int.TryParse(text, out temp))
                return temp;

            double temp2 = 0.0;
            if (double.TryParse(text, out temp2))
                return temp2;

            throw new NovelException("Object " + text + " has not supported type.", ParsedFile, ParsedLine);
        }
    }
}
