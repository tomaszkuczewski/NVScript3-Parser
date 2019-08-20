# NVScript3 Parser
# Summary
(Not finished) That is the parser that allows to compile and execute a language
that is very similiar to JavaScript. Language uses only stack so the state of the program
can be easly serialized and recovered later.

REMINDER: It was not meant to be released for public due to lack of time to REFACTOR but its used as a code sample
because it took A LOT OF TIME.

- Supports functions
- Supports conditions
- Supports while loop
- Supports callbacks to C#
- Supports error checking
- Uses Reverse Polish notation with support of calling functions

# Example code in NV

```javascript
/*
	Block comment
*/

//Line comment

function main()
{
	var a = 5;
	
	if( a == 5 )
	{
		Print("Hello world"); //C# callback
		a = add(input);
	}
}

function add(input)
{
	return input + 1;
}
```

# Example code in C#
```c#
	var text = File.ReadAllText("test.txt");
	//Adding c# callbacks
	NovelFunctionPool delegates = new NovelFunctionPool();
	delegates.AddFunction(new Action<string>(Print));
	//Parsing to the script object
	var script = new NovelParser().ParseText("test.txt", text, delegates);
	//Using default executor
	var executor = new NovelExecutor(script);
	executor.Reset();
	executor.CallScriptFunction("main", null);
	executor.ContinueExecution();
```