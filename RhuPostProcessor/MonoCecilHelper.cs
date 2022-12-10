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
			return obj.GetType() == typeof(int)
				? Instruction.Create(OpCodes.Ldc_I4,(int)obj)
				: obj.GetType() == typeof(uint)
				? Instruction.Create(OpCodes.Ldc_I4_S, (uint)obj)
				: obj.GetType() == typeof(bool)
				? (bool)obj ? Instruction.Create(OpCodes.Ldc_I4_1) : Instruction.Create(OpCodes.Ldc_I4_0)
				: obj.GetType() == typeof(char)
				? Instruction.Create(OpCodes.Ldc_I4_S,(uint)obj)
				: obj.GetType() == typeof(string)
				? Instruction.Create(OpCodes.Ldstr,(string)obj)
				: obj.GetType() == typeof(float)
				? Instruction.Create(OpCodes.Ldc_R4,(float)obj)
				: obj.GetType() == typeof(double)
				? Instruction.Create(OpCodes.Ldc_R8, (double)obj)
				: obj.GetType() == typeof(long)
				? Instruction.Create(OpCodes.Ldc_I8,(long)obj)
				: obj.GetType() == typeof(ulong)
				? Instruction.Create(OpCodes.Ldc_I8, (ulong)obj)
				: obj.GetType() == typeof(byte)
				? Instruction.Create(OpCodes.Ldc_I4, (int)obj)
				: obj.GetType() == typeof(sbyte)
				? Instruction.Create(OpCodes.Ldc_I4_S, (uint)obj)
				: obj.GetType() == typeof(short)
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
			return self == null
				?               throw new ArgumentNullException(nameof(self))
				: !self.HasMethods ? (IEnumerable<MethodDefinition>)Array.Empty<MethodDefinition>() : self.Methods.Where((MethodDefinition method) => method.IsConstructor);
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
			catch {
				Console.WriteLine("Exception getting constructor for type: " + type.FullName);
				throw;
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
				throw new ArgumentException(nameof(arguments.Length));
			}
			if (self.GenericParameters.Count != arguments.Length) {
				throw new ArgumentException(nameof(arguments.Length));
			}
			var genericInstanceType = new GenericInstanceMethod(self);
			foreach (var item in arguments) {
				genericInstanceType.GenericArguments.Add(item);
			}
			return genericInstanceType;
		}
		public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments) {
			if (arguments.Length == 0) {
				throw new ArgumentException(nameof(arguments.Length));
			}
			if (self.GenericParameters.Count != arguments.Length) {
				throw new ArgumentException(nameof(arguments.Length));
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
