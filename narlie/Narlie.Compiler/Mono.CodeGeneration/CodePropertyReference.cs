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
	public class CodePropertyReference: CodeValueReference
	{
		CodeExpression target;
		PropertyInfo property;
		
		public CodePropertyReference (CodeExpression target, PropertyInfo property)
		{
			this.target = target;
			this.property = property;		
		}
		
		public override void Generate (ILGenerator gen)
		{
			if (!property.CanRead) throw new InvalidOperationException ("Property " + property.DeclaringType + "." + property.Name + " does not have a getter.");
			CodeGenerationHelper.GenerateMethodCall (gen, target, property.GetGetMethod());
		}
		
		public override void GenerateSet (ILGenerator gen, CodeExpression value)
		{
			if (!property.CanWrite) throw new InvalidOperationException ("Property " + property.DeclaringType + "." + property.Name + " does not have a setter.");
			CodeGenerationHelper.GenerateMethodCall (gen, target, property.GetSetMethod(), value);
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			if ((object)target != null) {
				target.PrintCode (cp);
				cp.Write (".");
				cp.Write (property.Name);
			} else {
				cp.Write (property.DeclaringType + "." + property.Name);
			}
		}
		
		public override Type GetResultType ()
		{
			return property.PropertyType;
		}
	}
}
