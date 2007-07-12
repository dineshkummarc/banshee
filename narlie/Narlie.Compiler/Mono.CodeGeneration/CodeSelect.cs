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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeSelect: CodeStatement
	{
		ArrayList conditions = new ArrayList ();
		ArrayList blocks = new ArrayList ();
		bool hasDefaultCase;
		
		public CodeSelect ()
		{
		}
		
		public void AddCase (CodeExpression condition, CodeBlock block)
		{
			if (object.ReferenceEquals (condition, null))
				throw new ArgumentNullException ("condition");
			if (hasDefaultCase)
				throw new InvalidOperationException ("The default case must be the last case in the select block.");
				
			conditions.Add (condition);
			blocks.Add (block);
		}
		
		public void AddDefaultCase (CodeBlock block)
		{
			if (hasDefaultCase)
				throw new InvalidOperationException ("Only one default case is allowed in a select block.");

			hasDefaultCase = true;
			conditions.Add (null);
			blocks.Add (block);
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (blocks.Count == 0) return;
			
			CodeExpression cond = (CodeExpression)conditions[0];
			if (object.ReferenceEquals (cond, null)) {
				((CodeBlock) blocks[0]).Generate (gen);
				return;
			}
			
			CodeIf initialIf = new CodeIf (cond);
			initialIf.TrueBlock = (CodeBlock) blocks[0];
			CodeIf prevCif = initialIf;
			
			for (int n=1; n<blocks.Count; n++) {
				cond = (CodeExpression)conditions[n];
				if (!object.ReferenceEquals (cond, null)) {
					CodeIf cif = new CodeIf (cond);
					cif.TrueBlock = (CodeBlock) blocks[n];
					CodeBlock cb = new CodeBlock ();
					cb.Add (cif);
					prevCif.FalseBlock = cb;
					prevCif = cif;
				} else {
					prevCif.FalseBlock = (CodeBlock) blocks[n];
				}
			}

			initialIf.Generate (gen);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			for (int n=0; n<blocks.Count; n++) {
				CodeExpression cond = (CodeExpression)conditions[n];
				if (!object.ReferenceEquals (cond, null)) {
					if (n == 0)
						cp.Write ("if (");
					else
						cp.Write ("else if (");
					
					((CodeExpression)conditions[n]).PrintCode (cp);
					cp.Write (") {");
					cp.EndLine ();
					cp.Indent ();
					((CodeBlock) blocks[n]).PrintCode (cp);
					cp.Unindent ();
					cp.BeginLine ().Write ("}");
					if (n < blocks.Count - 1) {
						cp.EndLine ();
						cp.BeginLine ();
					}
				} else {
					cp.Write ("else {");
					cp.EndLine ();
					cp.Indent ();
					((CodeBlock) blocks[n]).PrintCode (cp);
					cp.Unindent ();
					cp.BeginLine ().Write ("}");
				}
			}
		}
	}
}
