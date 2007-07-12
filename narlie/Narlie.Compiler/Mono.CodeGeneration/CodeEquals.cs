//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (C) Lluis Sanchez Gual, 2004
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeEquals: CodeConditionExpression
	{
		CodeExpression exp1;
		CodeExpression exp2;
		Type t1;
		Type t2;
		MethodInfo eqm;
		
		public CodeEquals (CodeExpression exp1, CodeExpression exp2)
		{
			this.exp1 = exp1;
			this.exp2 = exp2;
			
			t1 = exp1.GetResultType ();
			t2 = exp2.GetResultType ();
			
			eqm = t1.GetMethod ("op_Equality", new Type[] { t1, t2 });
			if (eqm == null) eqm = t2.GetMethod ("op_Equality", new Type[] { t1, t2 });
			
			if (eqm == null && (!t1.IsAssignableFrom (t2) || (t1.IsValueType && !t1.IsPrimitive && !t1.IsEnum)))
				throw new InvalidOperationException ("Operator == cannot be applied to operands of type '" + t1 + "' and '" + t2 + "'");
			
			// Don't use the equality operator if the second operand is null
			if (eqm != null && (exp2 is CodeLiteral) && ((CodeLiteral)exp2).Value == null && !t1.IsValueType && !t2.IsValueType)
				eqm = null;
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (t1.IsPrimitive || t1.IsEnum || eqm == null) {
				exp1.Generate (gen);
				exp2.Generate (gen);
				gen.Emit (OpCodes.Ceq);
			}
			else
				CodeGenerationHelper.GenerateMethodCall (gen, null, eqm, exp1, exp2);				
		}
		
		public override void GenerateForBranch (ILGenerator gen, Label label, bool branchCase)
		{
			if (t1.IsPrimitive || t1.IsEnum || eqm == null)
			{
				exp1.Generate (gen);
				exp2.Generate (gen);
				if (branchCase)
					gen.Emit (OpCodes.Beq, label);
				else
					gen.Emit (OpCodes.Bne_Un, label);
			}
			else
			{
				CodeGenerationHelper.GenerateMethodCall (gen, null, eqm, exp1, exp2);				
				if (branchCase)
					gen.Emit (OpCodes.Brtrue, label);
				else
					gen.Emit (OpCodes.Brfalse, label);
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			exp1.PrintCode (cp);
			cp.Write (" == ");
			exp2.PrintCode (cp);
		}
		
		public override Type GetResultType ()
		{
			return typeof (bool);
		}
	}
}