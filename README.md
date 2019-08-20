# NVScript3 Parser
# Summary
(Not finished) That is the parser that allows to compile and execute a language
that is very similiar to JavaScript. Language uses only stack so the state of the program
can be easly serialized and recovered later.

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