using NVScript3.NVScript3.Exceptions;
using NVScript3.NVScript3.Execute;
using NVScript3.NVScript3.Instructions;
using NVScript3.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NVScript3.NVScript3.Parser
{
    public partial class NovelParser
    {
        public class VariableScope
        {
            private List<List<string>> Variables { get; set; }

            public VariableScope()
            {
                Variables = new List<List<string>>();
            }

            public void AddScope()
            {
                Variables.Add(new List<string>());
            }

            public void AddVaraible(string name)
            {
                Variables[Variables.Count - 1].Add(name);
            }

            public int GetStackIndex(string name)
            {
                int index = 0;
                for(int level = 0; level < Variables.Count; level++)
                {
                    for(int variable = 0; variable < Variables[level].Count; variable++)
                    {
                        if (Variables[level][variable].Equals(name))
                            return index;
                        else
                            index++;
                    }
                }
                return 0;
            }

            public int GetScopeStackVariablesCount()
            {
                if (Variables.Count > 0)
                    return Variables.Last().Count;

                return 0;
            }

            public List<string> GetScopeStackVariables()
            {
                if (Variables.Count > 0)
                    return Variables.Last();

                return null;
            }

            public int GetStackVariables()
            {
                int vars = 0;
                foreach (var level in Variables)
                    foreach (var variable in level)
                        vars++;

                return vars;
            }

            public void RemoveScope()
            {
                Variables.RemoveAt(Variables.Count - 1);
            }

            public bool ContainsVariable(string name)
            {
                foreach (var level in Variables)
                    foreach (var variable in level)
                        if (variable.Equals(name))
                            return true;

                return false;
            }

        }

        public class ConditionScope
        {
            public enum ConditionType
            {
                Normal,
                Loop      
            }

            public class ConditionScopeInformation
            {
                public int NestLevel = -1;
                public int FirstInstructionIndex = -1;
                public int ConditionInstructionIndex = -1;
                public int ConditionLevel = -1;
                public int InstructionOnFalse = -1;
                public ConditionType Type;
            }

            public Dictionary<int, List<ConditionScopeInformation>> Conditions { get; set; }

            public ConditionScope()
            {
                Conditions = new Dictionary<int, List<ConditionScopeInformation>>();
            }

            public bool IsNestLevelEmpty(int nestLevel)
            {
                if (!Conditions.ContainsKey(nestLevel))
                    return true;

                if (Conditions[nestLevel].Count == 0)
                    return true;

                return false;
            }

            public int GetLastConditionLevel(int nestLevel)
            {
                if (!Conditions.ContainsKey(nestLevel))
                    return -1;

                if (Conditions[nestLevel].Count == 0)
                    return -1;

                return Conditions[nestLevel].Last().ConditionLevel;
            }

            public void ClearScope(int nestLevel)
            {
                Conditions[nestLevel].Clear();
            }

            public List<ConditionScopeInformation> GetNestedInformation(int nestLevel)
            {
                if (!Conditions.ContainsKey(nestLevel))
                    Conditions.Add(nestLevel, new List<ConditionScopeInformation>());

                return Conditions[nestLevel];
            }

            public void AddCondition(int nestLevel, int firstInstruction, int conditionInstruction, int conditionLevel, ConditionType type = ConditionType.Normal)
            {
                if (!Conditions.ContainsKey(nestLevel))
                    Conditions.Add(nestLevel, new List<ConditionScopeInformation>());

                var conditionInfo = new ConditionScopeInformation();
                conditionInfo.NestLevel = nestLevel;
                conditionInfo.FirstInstructionIndex = firstInstruction;
                conditionInfo.ConditionInstructionIndex = conditionInstruction;
                conditionInfo.ConditionLevel = conditionLevel;
                conditionInfo.Type = type;
                Conditions[nestLevel].Add(conditionInfo);
            }
        }


        private VariableScope Variables = new VariableScope();
        private ConditionScope Conditions = new ConditionScope();

        enum ParentBlockType
        {
            StartBlock,
            Function,
            Condition
        }

        private bool ContainsUserReturn = false;

        private NovelScript ParsedScript = null;

        private NovelScript ParseScript(string name, NovelParserBlock block, NovelFunctionPool funcPool = null)
        {
            //Setup script
            ParsedScript = new NovelScript(name);
            ParsedScript.DelegateFunctionList = funcPool;

            try
            {
                ParsedScript = ParseScript(ParsedScript, block, ParentBlockType.StartBlock, 0);
            }
            catch (NovelException exception)
            {
                //TODO remove
                //Print the exception
                Console.WriteLine(exception.Message);
            }

            return ParsedScript;
        }
        
        private NovelScript ParseScript(NovelScript script, NovelParserBlock block, ParentBlockType parentType, int nestLevel)
        {
            if (parentType == ParentBlockType.Function)
                ContainsUserReturn = false;

            //Prepeare variable scope
            Variables.AddScope();


            //For each content in block
            for (int i = 0; i < block.Content.Count(); i++)
            {
                //Get the phrase
                var phrase = block.Content[i];

                //Setting up the line for debbugging purposes
                ParsedLine = phrase.CodeLine;

                //If its another block
                if (phrase is NovelParserBlock)
                {
                    var localBlock = phrase as NovelParserBlock;
                    var header = localBlock.Header;

                    //If header is the function signature
                    if (IsFunctionSignature(header))
                    {
                        //If the function signatures are nested
                        if (parentType != ParentBlockType.StartBlock)
                            throw new NovelException("Function signatures cannot be nested", ParsedFile, ParsedLine);

                        //Get the function object
                        var function = ParseFunctionSignature(header, script.Instructions.Count);

                        //Add function to list
                        script.FunctionList.Add(function);

                        Variables.AddScope();
                        //Add arguments to stack
                        for(int j = 0; j < function.ParameterNameList.Count; j++)
                            Variables.AddVaraible(function.ParameterNameList[j]);

                        //Parse the local block
                        script = ParseScript(script, localBlock, ParentBlockType.Function, nestLevel + 1);
                        Variables.RemoveScope();
                    }   
                    //If header indicates that this is condition start
                    else if(IsCondition(header))
                    {
                        //Check if condition is inside function block or another condition block
                        if(parentType != ParentBlockType.Function &&
                            parentType != ParentBlockType.Condition)
                            throw new NovelException("Condition has to be in function", ParsedFile, ParsedLine);

                        //Getting the condition level
                        int conditionLevel = -1;

                        List<NovelInstruction> instructions = new List<NovelInstruction>();

                        //Get the novel condition
                        var condition = ParseCondition(header, out conditionLevel, ref instructions);

                        //TODO check boolean type compatibility
                        //Check if the last term element is logical type
                        //var terms = condition.Expression.Term;
                        //var lastTerm = terms[terms.Count - 1];

                        //if( !(lastTerm is NovelLogicOperator) )
                        //    throw new NovelException("Condition does not contain logic expression", ParsedFile, ParsedLine);

                        //Getting last type of condition (if, else if, else)
                        int lastLevel = Conditions.GetLastConditionLevel(nestLevel);

                        //Check if the upcoming condition is higher than 
                        //those in condition scope, or if those are else if
                        if(conditionLevel > lastLevel || (conditionLevel == 1 && lastLevel == 1))
                        {
                            //If lastLevel is "If" or "Else if" then
                            if (lastLevel == 0 || lastLevel == 1)
                                script.Instructions.Add(new NovelJump());
                
                            int firstInstruction = script.Instructions.Count;

                            foreach (var inst in instructions)
                                script.Instructions.Add(inst);

                            Conditions.AddCondition(nestLevel, firstInstruction, 
                                script.Instructions.Count, conditionLevel, 
                                ConditionScope.ConditionType.Normal);

                            script.Instructions.Add(condition);
                        }
                        else
                        {
                            throw new NovelException("Invalid type of conditional instruction in this context", ParsedFile, ParsedLine);
                        }

                        ParsedScript = ParseScript(script, localBlock, ParentBlockType.Condition, nestLevel + 1);
                    }
                    //If header indicates that this is a loop
                    else if(IsWhileLoop(header))
                    {
                        //Check if condition is inside function block or another condition block
                        if (parentType != ParentBlockType.Function &&
                            parentType != ParentBlockType.Condition)
                            throw new NovelException("Condition has to be in function", ParsedFile, ParsedLine);

                        List<NovelInstruction> instructions = new List<NovelInstruction>();
                        var condition = ParseWhileLoop(header, ref instructions);

                        //Get the index of where the local variables for condition start
                        int firstInstructionOfCndIndex = script.Instructions.Count;

                        //Add all instructions
                        foreach (var instruction in instructions)
                            script.Instructions.Add(instruction);

                        //Add stuff to condition scope
                        Conditions.AddCondition(nestLevel, firstInstructionOfCndIndex, 
                            script.Instructions.Count, 1, 
                            ConditionScope.ConditionType.Loop);

                        //Add condition at the end
                        script.Instructions.Add(condition);
                        ParsedScript = ParseScript(script, localBlock, ParentBlockType.Condition, nestLevel + 1);
                    }
                }
                //Or if its a plain text
                else if (phrase is NovelParserText)
                {
                    //If condition scope contains
                    if (Conditions.GetLastConditionLevel(nestLevel) > -1)
                        UpdateConditions(nestLevel);

                    var localText = phrase as NovelParserText;
                    var instruction = localText.Instruction;

                    //If is assignment expression
                    //if (IsAssigment(instruction))
                    //{
                    //    //Parse assignment (supports initialization)
                    //    var instructions = ParseAssignment(instruction);
                    //    foreach (var inst in instructions)
                    //        script.Instructions.Add(inst);
                    //}
                    if (IsReturn(instruction))
                    {
                        var instructions = ParseReturn(instruction);
                        foreach (var inst in instructions)
                            script.Instructions.Add(inst);

                        ContainsUserReturn = true;
                    }
                    //If its other kind of expression
                    else
                    {
                        //Parse expression (supports initialization)
                        var instructions = ParseExpression(instruction);
                        foreach (var inst in instructions)
                            script.Instructions.Add(inst);
                    }
                }

                //TODO find a better way to this
                //Now we have to handle returning values
                //If the last instruction of this block isn't the return value then return default
            }

            //If the end of the block then end condition
            UpdateConditions(nestLevel);

            //Return value should cause local variables to unstack
            if (parentType == ParentBlockType.Function)
            {
                //If contains returns in function but last instruction is not a return
                if (ContainsUserReturn == true && !(script.Instructions[script.Instructions.Count() - 1] is NovelReturn))
                    throw new NovelException("Function does not return a value.", ParsedFile, ParsedLine);
                //If last instruction isn't return
                else if(!(script.Instructions[script.Instructions.Count() - 1] is NovelReturn))
                    script.Instructions.Add(new NovelReturn(new NovelExpressionLiteral((int) 0)));
            }
            //No return so manual unstacking
            else
            {
                //Unstack local variables
                var stackVars = Variables.GetScopeStackVariablesCount();
                if (stackVars > 0)
                {
                    foreach (var v in Variables.GetScopeStackVariables())
                        Console.WriteLine(v);

                    script.Instructions.Add(new NovelExpandStack(-stackVars));
                }
            }

            //Remove scope before return
            Variables.RemoveScope();

            return script;
        }

        public void UpdateConditions(int nestLevel)
        {
            //Get the condition scope
            var conditionScope = Conditions.GetNestedInformation(nestLevel);

            //Get the next condition index (and after first "for" loop
            //it takes value of index of instruction at the end 
            //of conditional branch  (eg. index of where "else" is ending)
            int nextCondition = -1;

            //Instruction index of last block
            int afterConditionIndex = -1;

            //For each saved conditional instruction in nest level
            for (int i = 0; i < conditionScope.Count; i++)
            {
                //Reset the value for each condition in the scope
                nextCondition = -1;

                int indexOfCondition = conditionScope[i].ConditionInstructionIndex;

                //Check if there is (eg. else after if)
                if (i + 1 < conditionScope.Count)
                    nextCondition = conditionScope[i + 1].FirstInstructionIndex - indexOfCondition;

                //If not then set next jump to where conditional branch ends
                if (nextCondition == -1)
                    nextCondition = ParsedScript.Instructions.Count - indexOfCondition;

                afterConditionIndex = nextCondition + indexOfCondition;

                //Update the NovelCondition in instruction list
                //Get novelcondition instruction from script
                var condition = ParsedScript.Instructions
                    [indexOfCondition] as NovelCondition;

                //When its looped condition
                //then offset has to be added + 1 so it reaches after the jump
                int localOffset = 0;

                if (conditionScope[i].Type == ConditionScope.ConditionType.Loop)
                    localOffset += 1;
                //Update of local variable

                condition.InstructionOnFalse = nextCondition + localOffset;

                //Set information field to help the next step
                conditionScope[i].InstructionOnFalse = nextCondition;

                //Actuall update
                ParsedScript.Instructions[indexOfCondition] = condition;
            }

            //For each saved conditional instruction in nest level
            foreach(var condition in conditionScope)
            {
                //Update NovelJump in instruction list
                //at the end of "if"s and "else ifs"
                if (condition.ConditionLevel == 0 ||
                   condition.ConditionLevel == 1)
                {
                    //Index of jumpto
                    var jumpToIndex = condition.ConditionInstructionIndex + condition.InstructionOnFalse - 1;

                    //Update for loop
                    if(condition.Type == ConditionScope.ConditionType.Loop)
                    {
                        //Add the novel jump and the end of the scope
                        var jumpInst = new NovelJump();
                        jumpInst.JumpIndex = condition.FirstInstructionIndex - jumpToIndex - 1;
                        ParsedScript.Instructions.Add(jumpInst);
                    }
                    else if(condition.Type == ConditionScope.ConditionType.Normal)
                    {
                        //Get the jumpto instruction
                        var jumpto = ParsedScript.Instructions[jumpToIndex] as NovelJump;
                        if (jumpto != null)
                        {
                            //Set up the jump after end of this conditional block
                            jumpto.JumpIndex = afterConditionIndex - jumpToIndex;
                            //Update jump instruction
                            ParsedScript.Instructions[condition.ConditionInstructionIndex +
                                condition.InstructionOnFalse - 1] = jumpto;
                        }
                    }
                }
            }

            Conditions.ClearScope(nestLevel);
        }

        public bool IsWhileLoop(string text)
        {
            if (text.IndexOf("while") == 0)
            {
                return true;
            }
            return false;
        }

        public NovelCondition ParseWhileLoop(string text, ref List<NovelInstruction> instructions)
        {
            //Get the text inside parentheses
            var conditionText = text.Substring("while".Length).Trim();
            //Use expression system for loops
            var novelCondition = new NovelCondition();
            //Parse expression
            var expr = ParseExpression(conditionText);
            //If last is stack expansion
            if(expr.Last() is NovelExpandStack)
            {
                //Add all the instructions except the last one
                //which is stack expansion, coz SE is handled by 
                //condition instruction itself
                for (int i = 0; i < expr.Count - 1; i++)
                    instructions.Add(expr[i]);
                //Take stack expand instruction
                novelCondition.StackInstruction = expr.Last() as NovelExpandStack;
                //Take last expression
                var lastExpression = expr[expr.Count - 2] as NovelExpression;
                //Get the RESULT variable from expression
                novelCondition.ConditionOperand = lastExpression.Term[0] as NovelExpressionOperand;
            }
            else
            {
                //Take last expression
                var lastExpression = expr.Last() as NovelExpression;
                //Get the RESULT variable from expression
                novelCondition.ConditionOperand = lastExpression.Term[0] as NovelExpressionOperand;
            }

            return novelCondition;
        }

        public bool IsCondition(string text)
        {
            if( text.IndexOf("if") == 0 || 
                text.IndexOf("else if") == 0 ||
                text.IndexOf("else") == 0)
            {
                return true;
            }
            return false;
        }

        public NovelCondition ParseCondition(string text, out int conditionLevel, ref List<NovelInstruction> instructions)
        {
            conditionLevel = -1;
            var conditionStart = new string [] { "if", "else if", "else" };
            for(int i = 0; i < conditionStart.Length; i++)
            {
                if(text.IndexOf(conditionStart[i]) == 0)
                {
                    conditionLevel = i;
                    break;
                }
            }

            var conditionText = text.Substring(
                conditionStart[conditionLevel].Length).Trim();

            var novelCondition = new NovelCondition();

            if(conditionLevel == 0 || conditionLevel == 1)
            {
                var unfold = ParseExpression(conditionText);

                if(unfold.Last() is NovelExpandStack)
                {
                    for (int i = 0; i < unfold.Count - 1; i++)
                        instructions.Add(unfold[i]);

                    novelCondition.StackInstruction = unfold.Last() as NovelExpandStack;
                    var lastExpression = unfold[unfold.Count - 2] as NovelExpression;
                    novelCondition.ConditionOperand = lastExpression.Term[0] as NovelExpressionOperand;
                }
                else
                {
                    var lastExpression = unfold.Last() as NovelExpression;
                    novelCondition.ConditionOperand = lastExpression.Term[0] as NovelExpressionOperand;
                }
            }
            else if(conditionLevel == 2)
            {
                var expression = new NovelExpression();
                expression.Term.Add(new NovelExpressionLiteral(true));
                novelCondition.ConditionOperand = new NovelExpressionLiteral(true);
            }
            return novelCondition;
        }

        private bool IsAssigment(string text)
        {
            if (text.Split('=').Length >= 2)
                return true;

            return false;
        }

        private List<NovelInstruction> ParseAssignment(string text)
        {
            //Splitting the text by assignment operator
            var textByAssignOp = text.Split('=');

            //Getting the left value
            var leftValue = textByAssignOp[0].Trim();

            //Check if variable is declared
            bool containsDeclaration = false;

            //If contains the var to declare variable
            if(leftValue.IndexOf("var ") == 0 || leftValue.IndexOf("var\t") == 0)
            {
                leftValue = leftValue.Substring(3).Trim();
                containsDeclaration = true;
            }

            //Validate the name of variable
            if (!ValidateName(leftValue))
                throw new NovelException("Invalid variable name " + leftValue + ".", ParsedFile, ParsedLine);

            //If contains declaration
            if(containsDeclaration)
            {
                if (Variables.ContainsVariable(leftValue))
                    throw new NovelException("Variable " + leftValue + " already exists.", ParsedFile, ParsedLine);
                else
                    Variables.AddVaraible(leftValue);
            }
            //if does not contains declaration
            else
            {
                if(!Variables.ContainsVariable(leftValue))
                    throw new NovelException("Variable " + leftValue + " does not exists in this scope.", ParsedFile, ParsedLine);
            }

            //Getting the rvalue (right side)
            //var rightValue = StringUtils.RemoveSpaces(textByAssignOp[1]).Trim();
            var rightValue = textByAssignOp[1].Trim();

            var expressionString = leftValue + "=" + rightValue;
            var instructions = ParseExpression(expressionString);

            instructions.Insert(0, new NovelExpandStack(1));
            return instructions;
        }

        private bool IsFunctionSignature(string text)
        {
            //If keyword function is on 0 index
            if (text.IndexOf("function ") == 0 || text.IndexOf("function\t") == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse the function signature
        /// </summary>
        private NovelFunctionInformationEntry ParseFunctionSignature(string text, int offset)
        {
            //Remove function keyword
            text = text.Substring(8);

            //Remove spaces before the name
            text = text.TrimStart();

            //If there is no parentheses
            if (text.IndexOf("(") == -1 || text.IndexOf(")") == -1)
                throw new NovelException("Invalid function signature no parenthesis. ", ParsedFile, ParsedLine);

            //Get function name
            var functionName = text.Substring(0, text.IndexOf("("));

            //Validate function name
            if (!ValidateName(functionName))
                throw new NovelException("Invalid function name. ", ParsedFile, ParsedLine);

            //Remove function name from TextLeft string
            text = text.Substring(text.IndexOf("("));

            //Get function parameters
            var signatureParameters = StringUtils.GetBetween(text, "(", ")").TrimStart();

            //Remove white spaces from parameters
            signatureParameters = StringUtils.RemoveSpaces(signatureParameters);

            //Split parameters per ,
            var parameters = signatureParameters.Split(',');

            //List of parameter names
            List<string> parameterNameList = new List<string>();

            //Foreach parameters
            for (int i = 0; i < parameters.Count(); ++i)
            {
                //Validate the name
                if (ValidateName(parameters[i]))
                    parameterNameList.Add(parameters[i]);
                //If the first parameter is empty string then it means there is no parameters in function
                else if (StringUtils.IsEmpty(parameters[i]) && parameters.Count() == 1)
                    continue;
                //If name is wrong then put an error
                else
                    throw new NovelException("Invalid characters in function parameters ", ParsedFile, ParsedLine);
            }

            //Remove everything between ()
            text = text.Substring(text.IndexOf(")") + 1);

            //Assign the parsed function
            return new NovelFunctionInformationEntry(functionName, parameterNameList, offset);
        }

        private bool IsReturn(string text)
        {
            if (text.Trim().IndexOf("return ") == 0 ||
                text.Trim().IndexOf("return\t") == 0)
                return true;

            return false;
        }

        private List<NovelInstruction> ParseReturn(string text)
        {
            //First remove return from string
            text = text.Remove(0, 6);

            //Parse the expression
            var unfold = ParseExpression(text);

            var instructions = new List<NovelInstruction>();
            //If the expression has temporary variables then the last
            //instruction is ExpandStack
            if(unfold.Last() is NovelExpandStack)
            {
                //Copy all instructions except expandstack
                for (int i = 0; i < unfold.Count - 1; i++)
                    instructions.Add(unfold[i]);
                //Get the last instruction
                var expandStack = unfold.Last() as NovelExpandStack;
                //Get the last expression (before the expandstack)
                var lastExpression = unfold[unfold.Count - 2] as NovelExpression;
                //Get the first part of term as it is Variable
                var returnVariable = lastExpression.Term[0] as NovelExpressionVariable;
                //Add return instruction
                instructions.Add(new NovelReturn(returnVariable));
            }
            else
            {
                //Get the last expression
                var lastExpression = unfold.Last() as NovelExpression;
                //Get the first part of term as it operand
                var returnVariable = lastExpression.Term[0] as NovelExpressionOperand;
                instructions.Add(new NovelReturn(returnVariable));
            }

            return instructions;
        }

        private bool ValidateName(string name)
        {
            if (StringUtils.IsEmpty(name))
                return false;

            var result = name.ToArray().All(c => char.IsLetterOrDigit(c) || c.Equals("_"));
            return result;
        }

    }
}
