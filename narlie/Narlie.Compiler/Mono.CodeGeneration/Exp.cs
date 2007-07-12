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
	public class Exp
	{
		Exp () {}
		
		public static CodeExpression Literal (object ob) { return new CodeLiteral (ob); }
		public static CodeExpression Literal (string ob) { return new CodeLiteral (ob, typeof(string)); }
		
		public static CodeExpression New (Type type, params CodeExpression[] pars)
		{
			return new CodeNew (type, pars);
		}
		
		public static CodeExpression NewArray (Type type, CodeExpression size)
		{
			return new CodeNewArray (type, size);
		}
		
		public static CodeExpression NewArray (Type type, int n)
		{
			return new CodeNewArray (type, Exp.Literal (n));
		}
		
		public static CodeExpression And (CodeExpression e1, CodeExpression e2)
		{
			return new CodeAnd (e1, e2);
		} 
		
		public static CodeExpression And (CodeExpression e1, CodeExpression e2, CodeExpression e3)
		{
			return new CodeAnd (new CodeAnd (e1, e2), e3);
		} 
		
		public static CodeExpression Or (CodeExpression e1, CodeExpression e2)
		{
			return new CodeOr (e1, e2);
		}
		 
		public static CodeValueReference Inc (CodeValueReference e)
		{
			return new CodeIncrement (e);
		} 
		
		public static CodeExpression Call (CodeExpression target, string name, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (target, name, parameters);
		}
		
		public static CodeExpression Call (CodeExpression target, MethodInfo method, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (target, method, parameters);
		}
		
		public static CodeExpression Call (CodeExpression target, CodeMethod method, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (target, method, parameters);
		}
		
		public static CodeExpression Call (Type type, string name, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (type, name, parameters);
		}
		
		public static CodeExpression Call (MethodInfo method, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (method, parameters);
		}
		
		public static CodeExpression Call (CodeMethod method, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (method, parameters);
		}
		
		public static CodeExpression NullValue (Type type)
		{
			return new CodeLiteral (null, type);
		}
		
		public static CodeExpression When (CodeExpression condition, CodeExpression trueResult, CodeExpression falseResult)
		{
			return new CodeWhen (condition, trueResult, falseResult);
		}
		
		public static CodeExpression MemberGet (Type type, string name)
		{
			MemberInfo[] mems = type.GetMember (name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
			if (mems.Length == 0) throw new InvalidOperationException ("Static field or property '" + name + "' not found in " + type); 
			return MemberGet (mems[0]);
		} 
		
		public static CodeExpression MemberGet (MemberInfo member)
		{
			if (member is FieldInfo) {
				FieldInfo field = (FieldInfo) member;
				return new CodeFieldReference (null, field);
			}
			else if (member is PropertyInfo) {
				PropertyInfo prop = (PropertyInfo) member;
				return new CodePropertyReference (null, prop);
			}
			else
				throw new InvalidOperationException (member.Name + " is not either a field or a property");
		} 
		
		public static bool CanGenerateLiteral (Type type)
		{
			return CodeLiteral.CanGenerate (type);
		}
	}
}
