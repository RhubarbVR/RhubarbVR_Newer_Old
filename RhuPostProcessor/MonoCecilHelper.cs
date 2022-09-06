using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RhuPostProcessor
{
	public static class MonoCecilHelper
	{
		public static Instruction SetOperand(this Instruction ins, object obj) {
			ins.Operand = obj;
			return ins;
		}

		public static Instruction GetInstructionForEvaluationStack(this object obj) {
			if(obj.GetType() == typeof(CustomAttributeArgument)) {
				obj = ((CustomAttributeArgument)obj).Value;
			}
			if (obj.GetType() == typeof(int)) {
				return Instruction.Create(OpCodes.Ldc_I4,(int)obj);
			}
			if (obj.GetType() == typeof(uint)) {
				return Instruction.Create(OpCodes.Ldc_I4_S, (uint)obj);
			}
			if (obj.GetType() == typeof(bool)) {
				return (bool)obj ? Instruction.Create(OpCodes.Ldc_I4_1) : Instruction.Create(OpCodes.Ldc_I4_0);
			}
			if (obj.GetType() == typeof(char)) {
				return Instruction.Create(OpCodes.Ldc_I4_S,(uint)obj);
			}
			if (obj.GetType() == typeof(string)) {
				return Instruction.Create(OpCodes.Ldstr,(string)obj);
			}
			if (obj.GetType() == typeof(float)) {
				return Instruction.Create(OpCodes.Ldc_R4,(float)obj);
			}
			if (obj.GetType() == typeof(double)) {
				return Instruction.Create(OpCodes.Ldc_R8, (double)obj);
			}
			if (obj.GetType() == typeof(long)) {
				return Instruction.Create(OpCodes.Ldc_I8,(long)obj);
			}
			if (obj.GetType() == typeof(ulong)) {
				return Instruction.Create(OpCodes.Ldc_I8, (ulong)obj);
			}
			if (obj.GetType() == typeof(byte)) {
				return Instruction.Create(OpCodes.Ldc_I4, (int)obj);
			}
			if (obj.GetType() == typeof(sbyte)) {
				return Instruction.Create(OpCodes.Ldc_I4_S, (uint)obj);
			}
			return obj.GetType() == typeof(short)
				? Instruction.Create(OpCodes.Ldc_I4_S, (uint)obj)
				: obj.GetType() == typeof(ushort)
				? Instruction.Create(OpCodes.Ldc_I4, (int)obj)
				:         throw new Exception($"Type {obj.GetType().Name} is not set up to be added to evaluationStack");
		}
		public static MethodReference MakeHostInstanceGeneric(this MethodReference self, TypeReference genericType) {
			var methodReference = new MethodReference(self.Name, self.ReturnType, genericType) {
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis,
				CallingConvention = self.CallingConvention
			};
			foreach (var parameter in self.Parameters) {
				methodReference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
			}
			foreach (var genericParameter in self.GenericParameters) {
				methodReference.GenericParameters.Add(new GenericParameter(genericParameter.Name, methodReference));
			}
			return methodReference;
		}

		public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition self) {
			if (self == null) {
				throw new ArgumentNullException("self");
			}
			return !self.HasMethods ? (IEnumerable<MethodDefinition>)Array.Empty<MethodDefinition>() : self.Methods.Where((MethodDefinition method) => method.IsConstructor);
		}

		public static MethodReference GetConstructor(this TypeReference type, ModuleDefinition module, out bool isMethodCall,bool AllowPrams = false) {
			try {
				if (type.IsGenericParameter) {
					MethodReference result = module.ImportReference(typeof(Activator)).Resolve().Methods.First((MethodDefinition m) => m.Name == "CreateInstance" && m.HasGenericParameters && !m.HasParameters).MakeGenericMethodType(type);
					isMethodCall = true;
					return result;
				}
				if (type.IsGenericInstance) {
					var genericInstanceType = (GenericInstanceType)type;
					var method = genericInstanceType.Resolve().GetConstructors().First((MethodDefinition ctr) => !(ctr.HasParameters && !AllowPrams) && !ctr.IsStatic)
						.MakeHostInstanceGeneric(genericInstanceType);
					method = module.ImportReference(method);
					isMethodCall = false;
					return method;
				}
				var method2 = type.Resolve().GetConstructors().First((MethodDefinition ctr) => !(ctr.HasParameters && !AllowPrams) && !ctr.IsStatic);
				var result2 = module.ImportReference(method2);
				isMethodCall = false;
				return result2;
			}
			catch (Exception ex) {
				Console.WriteLine("Exception getting constructor for type: " + type.FullName);
				throw ex;
			}
		}

		public static GenericInstanceMethod MakeGenericMethodType(this MethodDefinition self, IEnumerable<TypeReference> arguments) {
			var genericInstanceType = new GenericInstanceMethod(self);
			foreach (var item in arguments) {
				genericInstanceType.GenericArguments.Add(item);
			}
			return genericInstanceType;
		}
		public static GenericInstanceMethod MakeGenericMethodType(this MethodDefinition self, params TypeReference[] arguments) {
			if (arguments.Length == 0) {
				throw new ArgumentException();
			}
			if (self.GenericParameters.Count != arguments.Length) {
				throw new ArgumentException();
			}
			var genericInstanceType = new GenericInstanceMethod(self);
			foreach (var item in arguments) {
				genericInstanceType.GenericArguments.Add(item);
			}
			return genericInstanceType;
		}
		public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments) {
			if (arguments.Length == 0) {
				throw new ArgumentException();
			}
			if (self.GenericParameters.Count != arguments.Length) {
				throw new ArgumentException();
			}
			var genericInstanceType = new GenericInstanceType(self);
			foreach (var item in arguments) {
				genericInstanceType.GenericArguments.Add(item);
			}
			return genericInstanceType;
		}
		public static IEnumerable<MethodDefinition> AllMethods(this TypeReference self) {
			var resolvedSelf = self.Resolve();
			foreach (var item in resolvedSelf.Methods) {
				yield return item;
			}
			if(resolvedSelf.BaseType != null) {
				foreach (var item in resolvedSelf.BaseType.AllMethods()) {
					yield return item;
				}
			}
		}

		public static IEnumerable<PropertyDefinition> AllProperties(this TypeReference self) {
			var resolvedSelf = self.Resolve();
			foreach (var item in resolvedSelf.Properties) {
				yield return item;
			}
			if (resolvedSelf.BaseType != null) {
				foreach (var item in resolvedSelf.BaseType.AllProperties()) {
					yield return item;
				}
			}
		}
		public static FieldReference GetGenericFieldReference(this FieldDefinition field) {
			if (!field.DeclaringType.HasGenericParameters) {
				return field;
			}
			var genericInstanceType = new GenericInstanceType(field.DeclaringType);
			foreach (var genericParameter in field.DeclaringType.GenericParameters) {
				genericInstanceType.GenericArguments.Add(genericParameter);
			}
			return new FieldReference(field.Name, field.FieldType, genericInstanceType);
		}

		public static IEnumerable<EventDefinition> AlleEvents(this TypeReference self) {
			var resolvedSelf = self.Resolve();
			foreach (var item in resolvedSelf.Events) {
				yield return item;
			}
			if (resolvedSelf.BaseType != null) {
				foreach (var item in resolvedSelf.BaseType.AlleEvents()) {
					yield return item;
				}
			}
		}
	}
}
