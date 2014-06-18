using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{

	public enum OpCode
	{
		// Meta-opcodes
		Nop,		// Does not perform any operation.
		Invalid,	// Crashes the executor with an unrecoverable NotImplementedException.
		Debug,		// Does not perform any operation. Used to help debugging.

		// Stack ops and assignment
		Pop,		// Discards the topmost n elements from the v-stack. 
		Load,		// Loads a value from the specified symbol, and pushes it on the v-stack.
		Literal,	// Pushes a literal (constant value) on the stack. 
		Symbol,		// Loads a symbol on the stack
		SymStorN,	// Performs a store to a symbol, without needing the symbol on the v-stack and without extracting the operand from the v-stack.
		Store,		// Performs a single value assignment [including table fields]
		Assign,		// Performs complex assignment supporting tuples [including table fields]
		Closure,	// Creates a closure on the top of the v-stack, using the symbols for upvalues and num-val for entry point of the function.

		// Stack-frame ops and calls
		Enter,		// Enters a new stack frame
		Leave,		// Leaves a stack frame
		Exit,		// Leaves every stack frame up and including the topmost function frame, plus it exits the topmost closure
		BeginFn,	// Adjusts for start of function, taking in parameters and allocating locals
		Args,		// Takes the arguments passed to a function and sets the appropriate symbols in the local scope
		Call,		// Calls the function specified on the specified element from the top of the v-stack. If the function is a Moon# function, it pushes its numeric value on the v-stack, then pushes the current PC onto the x-stack, enters the function closure and jumps to the function first instruction. If the function is a CLR function, it pops the function value from the v-stack, then invokes the function synchronously and finally pushes the result on the v-stack.
		Ret,		// Pops the top n values of the v-stack. Then pops an X value from the v-stack. Then pops X values from the v-stack. Afterwards, it pushes the top n values popped in the first step, pops the top of the x-stack and jumps to that location.
		TailChk,	// Checks if the return value from a Ret or Clr call is a tail call request, in case it repeats the call 

		// Jumps
		Jump,		// Jumps to the specified PC
		Jf,			// Pops the top of the v-stack and jumps to the specified location if it's false
		JNil,		// Jumps if the top of the stack is nil
		JFor,		// Peeks at the top, top-1 and top-2 values of the v-stack which it assumes to be numbers. Then if top-1 is less than zero, checks if top is <= top-2, otherwise it checks that top is >= top-2. Then if the condition is false, it jumps.
		JtOrPop,	// Peeks at the topmost value of the v-stack as a boolean. If true, it performs a jump, otherwise it removes the topmost value from the v-stack.
		JfOrPop,	// Peeks at the topmost value of the v-stack as a boolean. If false, it performs a jump, otherwise it removes the topmost value from the v-stack.

		// Operators
		Concat,		// Concatenation of the two topmost operands on the v-stack
		LessEq,		// Compare <= of the two topmost operands on the v-stack
		Less,		// Compare < of the two topmost operands on the v-stack
		Eq,			// Compare == of the two topmost operands on the v-stack
		Add,		// Addition of the two topmost operands on the v-stack
		Sub,		// Subtraction of the two topmost operands on the v-stack
		Mul,		// Multiplication of the two topmost operands on the v-stack
		Div,		// Division of the two topmost operands on the v-stack
		Mod,		// Modulus of the two topmost operands on the v-stack
		Not,		// Logical inversion of the topmost operand on the v-stack
		Len,		// Size operator of the topmost operand on the v-stack
		Neg,		// Negation (unary minus) operator of the topmost operand on the v-stack
		Power,		// Power of the two topmost operands on the v-stack

		// Type conversions and manipulations
		MkTuple,	// Creates a tuple from the topmost n values
		Incr,		// Performs an add operation, without extracting the operands from the v-stack and assuming the operands are numbers.
		ToNum,		// Converts the top of the stack to a number
		ToBool,		// Converts the top of the stack to a boolean
		ExpTuple,	// Expands a tuple on the stack

		// Tables
		Index,		// Performs an index operation, pushing the indexed value on the stack.
		IndexRef,	// Performs an index operation, pushing a writable indexded value on the stack.
		IndexRefN,	// Performs an index operation, pushing a writable indexed value on the stack, does not pop the table.
		Method,		// Performs an index operation and pushes the table after the indexed value on the stack
		NewTable,	// Creates a new table on the stack

		// Iterators
		IterPrep,   // Prepares an iterator for execution 
		IterUpd,	// Updates the var part of an iterator
	}
}
