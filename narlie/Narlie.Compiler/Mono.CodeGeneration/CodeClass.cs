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
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeClass
	{
		TypeBuilder typeBuilder;
		ArrayList methods = new ArrayList ();
		ArrayList fields = new ArrayList ();
		Type baseType;
		Type[] interfaces;
		CodeMethod ctor;
		CodeMethod cctor;
		CodeBuilder instanceInit;
		CodeBuilder classInit;
		int varId;
		
		public CodeClass (ModuleBuilder mb, string name)
		: this (mb, name, TypeAttributes.Public, typeof(object))
		{
		}
		
		public CodeClass (ModuleBuilder mb, string name, Type baseType, params Type[] interfaces)
		: this (mb, name, TypeAttributes.Public, baseType, interfaces)
		{
		}
		
		public CodeClass (ModuleBuilder mb, string name, TypeAttributes attr, Type baseType, params Type[] interfaces)
		{
			typeBuilder = mb.DefineType (name, attr, baseType, interfaces);
			this.baseType = baseType;
			this.interfaces = interfaces;
		}
		
		public CodeMethod CreateMethod (string name, Type returnType, params Type[] parameterTypes)
		{
			CodeMethod met = new CodeMethod (this, GetMethodName (name), MethodAttributes.Public, returnType, parameterTypes);
			methods.Add (met);
			return met;
		}
		
		public CodeMethod CreateVirtualMethod (string name, Type returnType, params Type[] parameterTypes)
		{
			CodeMethod met = new CodeMethod (this, GetMethodName (name), MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameterTypes);
			methods.Add (met);
			return met;
		}
		
		public CodeMethod CreateStaticMethod (string name, Type returnType, params Type[] parameterTypes)
		{
			CodeMethod met = new CodeMethod (this, GetMethodName (name), MethodAttributes.Public | MethodAttributes.Static, returnType, parameterTypes);
			methods.Add (met);
			return met;
		}
		
		public CodeMethod CreateMethod (string name, MethodAttributes attributes, Type returnType, params Type[] parameterTypes)
		{
			CodeMethod met = new CodeMethod (this, GetMethodName (name), attributes, returnType, parameterTypes);
			methods.Add (met);
			return met;
		}
		
		public CodeMethod GetDefaultConstructor ()
		{
			if (ctor != null) return ctor;
			ctor = CreateConstructor (Type.EmptyTypes);
			return ctor;
		}
		
		public CodeMethod CreateConstructor (params Type[] parameters)
		{
			if (ctor != null) return ctor;
			ctor = CodeMethod.DefineConstructor (this, MethodAttributes.Public, parameters);
			methods.Add (ctor);
			CodeBuilder cb = GetInstanceInitBuilder ();
			ctor.CodeBuilder.CurrentBlock.Add (cb.CurrentBlock);
			return ctor;
		}
		
		public CodeMethod GetStaticConstructor ()
		{
			if (cctor != null) return cctor;
			cctor = CodeMethod.DefineConstructor (this, MethodAttributes.Public | MethodAttributes.Static, Type.EmptyTypes);
			methods.Add (cctor);
			CodeBuilder cb = GetClassInitBuilder ();
			cctor.CodeBuilder.CurrentBlock.Add (cb.CurrentBlock);
			return cctor;
		}
		
		public CodeMethod ImplementMethod (Type baseType, string methodName)
		{
			MethodInfo basem = baseType.GetMethod (methodName);
			return ImplementMethod (baseType, basem);
		}
		
		public CodeMethod ImplementMethod (MethodInfo basem)
		{
			return ImplementMethod (basem.DeclaringType, basem);
		}
		
		public CodeMethod ImplementMethod (Type baseType, MethodInfo basem)
		{
			ParameterInfo[] pinfos = basem.GetParameters ();
			Type[] pars = new Type[pinfos.Length];
			for (int n=0; n<pinfos.Length; n++)
				pars[n] = pinfos[n].ParameterType;

			CodeMethod met = CodeMethod.DefineMethod (this, basem.Name, MethodAttributes.Public | MethodAttributes.Virtual, basem.ReturnType, pars);
			typeBuilder.DefineMethodOverride (met.MethodInfo, basem);
			methods.Add (met);
			return met;
		}
		
		public CodeFieldReference DefineField (string name, Type type)
		{
			return DefineField (GetFieldName (name), type, FieldAttributes.Public, null);
		}
		
		public CodeFieldReference DefineField (string name, Type type, CodeExpression initialValue)
		{
			return DefineField (GetFieldName (name), type, FieldAttributes.Public, initialValue);
		}
		
		public CodeFieldReference DefineStaticField (CodeExpression initialValue)
		{
			return DefineField (GetFieldName (null), initialValue.GetResultType(), FieldAttributes.Public | FieldAttributes.Static, initialValue);
		}
		
		public CodeFieldReference DefineStaticField (string name, Type type)
		{
			return DefineField (GetFieldName (name), type, FieldAttributes.Public | FieldAttributes.Static, null);
		}
		
		public CodeFieldReference DefineStaticField (string name, Type type, CodeExpression initialValue)
		{
			return DefineField (GetFieldName (name), type, FieldAttributes.Public | FieldAttributes.Static, initialValue);
		}
		
		public CodeFieldReference DefineField (string name, Type type, FieldAttributes attrs)
		{
			return DefineField (GetFieldName (name), type, attrs, null);
		}
		
		public CodeFieldReference DefineField (string name, Type type, FieldAttributes attrs, CodeExpression initialValue)
		{
			FieldBuilder fb = typeBuilder.DefineField (GetFieldName (name), type, attrs);
			fields.Add (fb);
			CodeFieldReference fr;
			if ((attrs & FieldAttributes.Static) != 0)
				fr = new CodeFieldReference (fb);
			else
				fr = new CodeFieldReference (new CodeArgumentReference (TypeBuilder, 0, "this"), fb);
			
			if (!object.ReferenceEquals (initialValue, null)) {
				CodeBuilder cb = (attrs & FieldAttributes.Static) == 0 ? GetInstanceInitBuilder () : GetClassInitBuilder (); 
				cb.Assign (fr, initialValue);
			}
			return fr;
		}
		
		public TypeBuilder TypeBuilder
		{
			get { return typeBuilder; }
		} 
		
		private CodeBuilder GetInstanceInitBuilder ()
		{
			if (instanceInit != null) return instanceInit;
			instanceInit = new CodeBuilder (this);
			GetDefaultConstructor ();	// Ensure that the constructor is generated
			return instanceInit;
		}
		
		private CodeBuilder GetClassInitBuilder ()
		{
			if (classInit != null) return classInit;
			classInit = new CodeBuilder (this);
			GetStaticConstructor ();	// Ensure that the constructor is generated
			return classInit;
		}
		
		private string GetFieldName (string name)
		{
			if (name == null) return "__field_" + (varId++);
			else return name;
		}
		
		private string GetMethodName (string name)
		{
			if (name == null) return "__Method_" + (varId++);
			else return name;
		}
		
		public string PrintCode ()
		{
			StringWriter sw = new StringWriter ();
			CodeWriter cw = new CodeWriter (sw);
			PrintCode (cw);
			return sw.ToString ();
		}
		
		public void PrintCode (CodeWriter cw)
		{
/*			if ((typeBuilder.Attributes & TypeAttributes.Abstract) != 0) cw.Write ("abstract ");
			if ((typeBuilder.Attributes & TypeAttributes.NestedAssembly) != 0) cw.Write ("internal ");
			if ((typeBuilder.Attributes & TypeAttributes.NestedPrivate) != 0) cw.Write ("private ");
*/			if ((typeBuilder.Attributes & TypeAttributes.Public) != 0) cw.Write ("public ");
			cw.Write ("class ").Write (typeBuilder.Name);
			
			bool dots = false;
			if (baseType != null && baseType != typeof(object)) {
				cw.Write (" : " + baseType);
				dots = true;
			}
			
			if (interfaces != null && interfaces.Length > 0) {
				if (!dots) cw.Write (" : ");
				else cw.Write (", ");
				for (int n=0; n<interfaces.Length; n++) {
					if (n > 0) cw.Write (", ");
					cw.Write (interfaces[n].ToString ());
				}
			}
			
			cw.EndLine ().WriteLineInd ("{");
			
			foreach (FieldInfo f in fields) {
				cw.BeginLine ();
				if ((f.Attributes & FieldAttributes.Static) != 0)
					cw.Write ("static ");
				cw.Write (f.FieldType.Name + " ");
				cw.Write (f.Name + ";");
				cw.EndLine ();
				cw.WriteLine (""); 
			}
			
			for (int n=0; n<methods.Count; n++) {
				CodeMethod met = methods[n] as CodeMethod;
				if (n > 0) cw.WriteLine ("");
				met.PrintCode (cw);
			}

			cw.WriteLineUnind ("}");
		}
		
		public Type CreateType ()
		{
			if (ctor == null)
				ctor = GetDefaultConstructor ();

			foreach (CodeMethod met in methods)
				met.Generate ();
				
			Type t = typeBuilder.CreateType ();
			
			foreach (CodeMethod met in methods)
				met.UpdateMethodBase (t);
				
			return t;
		}
	}
}

