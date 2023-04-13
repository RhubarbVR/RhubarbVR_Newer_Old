using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Managers;

using WebAssembly.Runtime;
using WebAssembly;
using WebAssembly.Runtime.Compilation;
using System.IO;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;
using RhuEngine.Components;
using RhuEngine.Linker;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using RNumerics;
using RhuEngine.WorldObjects;
using System.Reflection.Emit;

namespace RhuEngine.Wasm
{
	public sealed class WasmInstance : IDisposable
	{
		public abstract class WasmScript
		{
			protected WasmScript() {
			}
#pragma warning disable IDE1006 // Naming Styles
			public abstract UnmanagedMemory memory { get; }
#pragma warning restore IDE1006 // Naming Styles

		}

		private readonly ConcurrentStack<int> _deadRefs = new();
		private readonly List<object> _refs = new();


		private static long MakePrem(object target) {
			return target is int @int
				? BitConverter.ToInt64(BitConverter.GetBytes(@int).Concat(new byte[4]).ToArray())
				: target is float @float
				? BitConverter.ToInt64(BitConverter.GetBytes(@float).Concat(new byte[4]).ToArray())
				: target is long @long
				? @long
				: target is double @double
				? BitConverter.ToInt64(BitConverter.GetBytes(@double))
				: throw new Exception("Not supported Type");
		}

		private object ReadData(int readingAdress, Type type) {
			var adress = ReadInt64(readingAdress);
			if (type == typeof(int)) {
				return BitConverter.ToInt32(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(uint)) {
				return BitConverter.ToUInt32(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(bool)) {
				return BitConverter.ToBoolean(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(byte)) {
				return BitConverter.GetBytes(adress)[0];
			}
			else if (type == typeof(sbyte)) {
				return unchecked((sbyte)BitConverter.GetBytes(adress)[0]);
			}
			else if (type == typeof(short)) {
				return BitConverter.ToInt16(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(ushort)) {
				return BitConverter.ToUInt16(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(char)) {
				return BitConverter.ToChar(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(float)) {
				return BitConverter.ToSingle(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(long)) {
				return BitConverter.ToInt64(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(ulong)) {
				return BitConverter.ToUInt64(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(double)) {
				return BitConverter.ToDouble(BitConverter.GetBytes(adress));
			}
			return GetObjectRef(BitConverter.ToInt32(BitConverter.GetBytes(adress)));
		}

		public static int MakeHash(MethodBase methodInfo) {
			HashCode hash = new();
			foreach (var item in methodInfo.GetParameters()) {
				hash.Add(item.ParameterType.Name);
			}
			return hash.ToHashCode();
		}

		private string GetUTF8String(int targetAdress) {
			if (CompiledScript?.Exports is null) {
				return null;
			}
			var list = new List<byte>();
			for (var i = 0; i < CompiledScript.Exports.memory.Size - targetAdress; i++) {
				var currentData = ReadByte(targetAdress + i);
				if (currentData == 0) {
					list.Add(currentData);
					return Encoding.UTF8.GetString(list.ToArray());
				}
				else {
					list.Add(currentData);
				}
			}
			return null;
		}

		public void WriteBytes(int adress, byte[] data) {
			if (adress + data.Length > CompiledScript.Exports.memory.Size) {
				return;
			}
			Marshal.Copy(data, 0, CompiledScript.Exports.memory.Start, data.Length);
		}
		public void WriteBytes(int adress, byte[] data, int offset, int size) {
			if (adress + (size - offset) > CompiledScript.Exports.memory.Size) {
				return;
			}
			Marshal.Copy(data, offset, CompiledScript.Exports.memory.Start, size);
		}
		public void WriteBytes(int adress, byte[] data, int size) {
			if (adress + size > CompiledScript.Exports.memory.Size) {
				return;
			}
			Marshal.Copy(data, 0, CompiledScript.Exports.memory.Start, size);
		}

		public int ReadInt(int adress) {
			return adress > CompiledScript.Exports.memory.Size ? 0 : Marshal.ReadInt32(CompiledScript.Exports.memory.Start, adress);
		}

		public byte ReadByte(int adress) {
			return adress > CompiledScript.Exports.memory.Size ? (byte)0 : Marshal.ReadByte(CompiledScript.Exports.memory.Start, adress);
		}

		public long ReadInt64(int adress) {
			return adress > CompiledScript.Exports.memory.Size ? 0 : Marshal.ReadInt64(CompiledScript.Exports.memory.Start, adress);
		}

		public int MakeRef(object currentObject) {
			if (currentObject is null) {
				return -1;
			}
			if (_deadRefs.TryPop(out var index)) {
				_refs[index] = currentObject;
				return index;
			}
			lock (_refs) {
				var newIndex = _refs.Count;
				_refs.Add(currentObject);
				return newIndex;
			}
		}

		/// <summary>
		/// Remove Ref from wasm object try
		/// </summary>
		/// <param name="target"></param>
		public void RemoveRef(int target) {
			if (target <= -1) {
				return;
			}
			_deadRefs.Push(target);
		}

		/// <summary>
		/// Create a string from acii
		/// </summary>
		/// <param name="target"></param>
		public unsafe int CreateAsciiString(int targetAdress) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var list = new List<byte>();
			for (var i = 0; i < CompiledScript.Exports.memory.Size - targetAdress; i++) {
				var currentData = ReadByte(targetAdress + i);
				if (currentData == 0) {
					list.Add(currentData);
					return MakeRef(Encoding.ASCII.GetString(list.ToArray()));
				}
				else {
					list.Add(currentData);
				}
			}
			return -1;
		}

		/// <summary>
		/// Create a string from UTF8
		/// </summary>
		/// <param name="target"></param>
		public unsafe int CreateUTF8String(int targetAdress) {
			return MakeRef(GetUTF8String(targetAdress));
		}

		/// <summary>
		/// Create a string from UTF32
		/// </summary>
		/// <param name="target"></param>
		public unsafe int CreateUTF32String(int targetAdress) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var list = new List<char>();
			for (var i = 0; i < CompiledScript.Exports.memory.Size - targetAdress; i++) {
				var currentData = ReadInt(targetAdress + i);
				if (currentData == 0) {
					list.Add((char)currentData);
					return MakeRef(new string(list.ToArray()));
				}
				else {
					list.Add((char)currentData);
				}
			}
			return -1;
		}

		/// <summary>
		/// Gets method to been called
		/// </summary>
		/// <returns></returns>
		public int GetMethod(int typeName, int methodName, int argumentCount, int makeHash, int genericTypes, int amount) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetUTF8String(typeName);
			var methodNameString = GetUTF8String(methodName);
			var genericTypestrings = new string[amount];
			for (var i = 0; i < amount; i++) {
				genericTypestrings[i] = GetUTF8String(genericTypes + i);
			}
			var targetType = FamcyTypeParser.PraseType(typeNameString);
			if (targetType is null) {
				return -1;
			}
			var targetMethod = targetType.GetMethods()
				.Where(x => x.Name == methodNameString)
				.Where(x => x.IsPublic).Where(x => x.GetParameters().Length == argumentCount)
				.Where(x => x.GetCustomAttribute<ExposedAttribute>(true) is not null)
				.Where(x => x.GetCustomAttribute<UnExsposedAttribute>(true) is null)
				.Where(x => (MakeHash(x) == makeHash) || makeHash == -1)
				.Where(x => x.IsGenericMethod ? genericTypestrings.Length >= 1 : genericTypestrings.Length == 0)
				.FirstOrDefault();
			if (targetMethod is null) {
				return -1;
			}
			if (genericTypestrings.Length >= 1) {
				var types = genericTypestrings.Select(x => FamcyTypeParser.PraseType(x)).ToArray();
				try {
					targetMethod = targetMethod.MakeGenericMethod(types);
				}
				catch {
					return -1;
				}
			}
			return MakeRef(Delegate.CreateDelegate(targetMethod.CreateDelegateTypeWithObject(), targetMethod));
		}


		/// <summary>
		/// Gets Constructor to been called
		/// </summary>
		/// <returns></returns>
		public int GetConstructor(int typeName, int argumentCount, int makeHash) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetUTF8String(typeName);
			var targetType = FamcyTypeParser.PraseType(typeNameString);
			if (targetType is null) {
				return -1;
			}
			var targetConstructor = targetType.GetConstructors()
				.Where(x => x.IsPublic).Where(x => x.GetParameters().Length == argumentCount)
				.Where(x => x.GetCustomAttribute<ExposedAttribute>(true) is not null)
				.Where(x => x.GetCustomAttribute<UnExsposedAttribute>(true) is null)
				.Where(x => (MakeHash(x) == makeHash) || makeHash == -1)
				.FirstOrDefault();
			if (targetConstructor is null) {
				return -1;
			}
			var pramTypes = targetConstructor.GetParameters().Select(x => x.ParameterType).ToArray();
			// Todo cash this step
			var dynamic = new DynamicMethod(string.Empty, targetType, pramTypes, targetType);
			var il = dynamic.GetILGenerator();
			il.DeclareLocal(targetType);
			for (var i = 0; i < pramTypes.Length; i++) {
				if (i == 0) {
					il.Emit(OpCodes.Ldarg_1);
				}
				else if (i == 1) {
					il.Emit(OpCodes.Ldarg_2);
				}
				else if (i == 2) {
					il.Emit(OpCodes.Ldarg_3, 0);
				}
				else {
					il.Emit(OpCodes.Ldarg, i - 1);
				}
			}
			il.Emit(OpCodes.Newobj, targetConstructor);
			il.Emit(OpCodes.Stloc_0);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ret);
			return MakeRef(Delegate.CreateDelegate(dynamic.CreateDelegateTypeWithObject(), dynamic));
		}


		/// <summary>
		/// Gets field to that is callable
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public int GetField(int typeName, int fieldName) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetUTF8String(typeName);
			var targetType = FamcyTypeParser.PraseType(typeNameString);
			if (targetType is null) {
				return -1;
			}
			var fieldNameString = GetUTF8String(fieldName);
			var targetField = targetType.GetFields()
				.Where(x => x.Name == fieldNameString)
				.Where(x => (x.GetCustomAttribute<ExposedAttribute>(true) is not null) || x.FieldType.IsAssignableTo(typeof(IWorldObject)))
				.Where(x => x.GetCustomAttribute<UnExsposedAttribute>(true) is null)
				.FirstOrDefault();
			return targetField is null ? -1 : MakeRef((Delegate)(Func<object, object>)targetField.GetValue);
		}

		/// <summary>
		/// Gets PropertyGetter to that is callable
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public int GetPropertyGetter(int typeName, int fieldName) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetUTF8String(typeName);
			var targetType = FamcyTypeParser.PraseType(typeNameString);
			if (targetType is null) {
				return -1;
			}
			var fieldNameString = GetUTF8String(fieldName);
			var targetProperty = targetType.GetProperties()
				.Where(x => x.Name == fieldNameString)
				.Where(x => x.GetCustomAttribute<ExposedAttribute>(true) is not null)
				.Where(x => x.GetCustomAttribute<UnExsposedAttribute>(true) is null)
				.Where(x => x.CanRead)
				.Where(x => x.GetMethod is not null)
				.FirstOrDefault();
			return targetProperty is null
				? -1
				: MakeRef(Delegate.CreateDelegate(targetProperty.GetMethod.CreateDelegateTypeWithObject(), targetProperty.GetMethod));
		}

		/// <summary>
		/// Call Method
		/// </summary>
		/// <param name="targetMethod"></param>
		/// <param name="argments"></param>
		/// <returns></returns>
		public long CallMethod(int targetMethod, int argments) {
			var getMethod = GetObjectRef<Delegate>(targetMethod);
			if (getMethod is null) { return -1; }
			var prams = getMethod.Method.GetParameters();
			var data = new object[prams.Length];
			for (var i = 0; i < prams.Length; i += 2) {
				data[i] = ReadData(argments + i, prams[i].ParameterType);
			}
			return MakePrem(ConvertType(getMethod.DynamicInvoke(data)));
		}

		/// <summary>
		/// Gets Property Setter to that is callable
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public int GetPropertySetter(int typeName, int fieldName) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetObjectRef<string>(typeName);
			var targetType = FamcyTypeParser.PraseType(typeNameString);
			if (targetType is null) {
				return -1;
			}
			var fieldNameString = GetObjectRef<string>(fieldName);
			var targetProperty = targetType.GetProperties()
				.Where(x => x.Name == fieldNameString)
				.Where(x => x.GetCustomAttribute<ExposedAttribute>(true) is not null)
				.Where(x => x.GetCustomAttribute<UnExsposedAttribute>(true) is null)
				.Where(x => x.GetCustomAttribute<NoWriteExsposedAttribute>(true) is null)
				.Where(x => x.CanWrite)
				.Where(x => x.SetMethod is not null)
				.FirstOrDefault();
			if (targetProperty is null) {
				return -1;
			}
			return MakeRef(Delegate.CreateDelegate(targetProperty.SetMethod.CreateDelegateTypeWithObject(), targetProperty.SetMethod));
		}

		/// <summary>
		/// Gets type of object
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public int GetCsharpType(int target) {
			return MakeRef(GetObjectRef(target)?.GetType());
		}

		/// <summary>
		/// Returns string of to string
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public int CsharpToString(int target) {
			return MakeRef(GetObjectRef(target)?.ToString());
		}

		public int CsharpParsType(int target) {
			return MakeRef(FamcyTypeParser.PraseType(GetObjectRef<string>(target)));
		}

		public int CsharpParsTypeUtf8(int target) {
			return MakeRef(FamcyTypeParser.PraseType(GetUTF8String(target)));
		}

		public int GetRunner() {
			return MakeRef(wasmRunner);
		}

		public int ArrayLength(int array) {
			return GetObjectRef<Array>(array)?.Length ?? -1;
		}

		public long ArrayGetValue(int array, int index) {
			return MakePrem(ConvertType(GetObjectRef<Array>(array).GetValue(index)));
		}

		public int StringLength(int target) {
			return GetObjectRef<string>(target)?.Length ?? -1;
		}

		public int StringAppend(int a, int b) {
			return MakeRef(GetObjectRef<string>(a) + GetObjectRef<string>(b));
		}

		public int StringEqual(int a, int b) {
			return (GetObjectRef<string>(a) == GetObjectRef<string>(b)) ? 1 : 0;
		}

		public int StringGetChar(int target, int index) {
			return BitConverter.ToInt32(BitConverter.GetBytes(GetObjectRef<string>(target)[index]).Concat(new byte[2]).ToArray());
		}

		/// <summary>
		/// Puts CSharp String in wasm
		/// </summary>
		/// <returns></returns>
		public int StringToNative(int targetString, int targetAdress, int size) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var stringTarget = GetObjectRef<string>(targetString);
			var asciBYtes = Encoding.ASCII.GetBytes(stringTarget);
			var extraSize = asciBYtes.Length - size;
			var min = Math.Min(asciBYtes.Length, size);
			if (min == 0) {
				return extraSize;
			}
			var bytesToAdd = new byte[min];
			for (var i = 0; i < min; i++) {
				bytesToAdd[i] = asciBYtes[i];
			}
			WriteBytes(targetAdress, bytesToAdd);
			return extraSize;
		}

		public T GetObjectRef<T>(int target) {
			return (T)(GetObjectRef(target) ?? default(T));
		}

		public object GetObjectRef(int target) {
			return target <= -1 ? null : _refs[target];
		}

		public Instance<WasmScript> CompiledScript { get; private set; }

		public Dictionary<string, Delegate> Methods = new();

		public IEnumerable<WasmTask> GetWasmTasks() {
			foreach (var item in WasmManager.wasmTasks) {
				if (item.HasWasmInstance(this)) {
					yield return item;
				}
			}
		}

		public bool IsCompiled = false;

		public object ConvertType(object data) {
			var currentType = data.GetType();
			return currentType == typeof(int)
				? (int)data
				: currentType == typeof(float)
				? BitConverter.ToInt32(BitConverter.GetBytes((float)data))
				: currentType == typeof(long)
				? (long)data
				: currentType == typeof(double)
				? BitConverter.ToInt64(BitConverter.GetBytes((double)data))
				: currentType == typeof(char)
				? BitConverter.ToInt32(BitConverter.GetBytes((char)data).Concat(new byte[2]).ToArray())
				: currentType == typeof(byte)
				? BitConverter.ToInt32((new byte[1] { (byte)data }).Concat(new byte[3]).ToArray())
				: currentType == typeof(sbyte)
				? BitConverter.ToInt32((new byte[1] { unchecked((byte)(sbyte)data) }).Concat(new byte[3]).ToArray())
				: currentType == typeof(uint)
				? BitConverter.ToInt32(BitConverter.GetBytes((uint)data))
				: currentType == typeof(short)
				? BitConverter.ToInt32(BitConverter.GetBytes((short)data).Concat(new byte[2]).ToArray())
				: currentType == typeof(ushort)
				? BitConverter.ToInt32(BitConverter.GetBytes((ushort)data).Concat(new byte[2]).ToArray())
				: currentType == typeof(ulong)
				? BitConverter.ToInt64(BitConverter.GetBytes((ulong)data))
				: MakeRef(data);
		}

		public object[] ConvertPrams(object[] prams) {
			return prams.Select(ConvertType).ToArray();
		}

		public object InvokeMethod(string methodName, params object[] prams) {
			prams = ConvertPrams(prams);
			return !IsCompiled ? null : Methods.TryGetValue(methodName, out var method) ? method.DynamicInvoke(prams) : null;
		}

		public object InvokeMethodNoCompileCheck(string methodName, params object[] prams) {
			prams = ConvertPrams(prams);
			return Methods.TryGetValue(methodName, out var method) ? method.DynamicInvoke(prams) : null;
		}

		private IDictionary<string, IDictionary<string, RuntimeImport>> BuildImports() {
			return new Dictionary<string, IDictionary<string, RuntimeImport>> {
					{ "env", new Dictionary<string, RuntimeImport> {
						{ "remove_ref", new FunctionImport(RemoveRef) },
						{ "create_ascii_string", new FunctionImport(CreateAsciiString) },
						{ "create_UTF8_string", new FunctionImport(CreateUTF8String) },
						{ "create_UTF32_string", new FunctionImport(CreateUTF32String) },
						{ "to_native", new FunctionImport(StringToNative) },
						{ "method_bind", new FunctionImport(GetMethod) },
						{ "constructor_bind", new FunctionImport(GetConstructor) },
						{ "field_bind", new FunctionImport(GetField) },
						{ "property_get_bind", new FunctionImport(GetPropertyGetter) },
						{ "property_set_bind", new FunctionImport(GetPropertySetter) },
						{ "call_method", new FunctionImport(CallMethod) },
						{ "get_type", new FunctionImport(GetCsharpType) },
						{ "to_string", new FunctionImport(CsharpToString) },
						{ "parse_type_utf8", new FunctionImport(CsharpParsTypeUtf8) },
						{ "parse_type", new FunctionImport(CsharpParsType) },
						{ "get_runner", new FunctionImport(GetRunner) },
						{ "array_length", new FunctionImport(ArrayLength) },
						{ "array_get_value", new FunctionImport(ArrayGetValue) },
						{ "string_length", new FunctionImport(StringLength) },
						{ "string_append", new FunctionImport(StringAppend) },
						{ "string_equal", new FunctionImport(StringEqual) },
						{ "string_get_char", new FunctionImport(StringGetChar) },
					}
				}
			};
		}

		public static Delegate CreateDelegate(MethodInfo methodInfo, object target) {
			var types = methodInfo.GetParameters().Select(p => p.ParameterType);
			if (methodInfo.ReturnType == typeof(void)) {
				return Delegate.CreateDelegate(Expression.GetActionType(types.ToArray()), target, methodInfo.Name);
			}
			else {
				types = types.Append(methodInfo.ReturnType);
				return Delegate.CreateDelegate(Expression.GetFuncType(types.ToArray()), target, methodInfo.Name);
			}
		}

		public WasmTask CompileScript(Stream stream) {
#if DEBUG
			RLog.Info("Wasm CompileScript");
#endif
			CleanUp();
			return WasmTask.RunWasmCompileTask(this, () => {
#if DEBUG
				RLog.Info("Wasm Compiling");
#endif
				CompiledScript = Compile.FromBinary<WasmScript>(stream)(BuildImports());
#if DEBUG
				RLog.Info("Wasm Loading Methods");
#endif
				foreach (var item in CompiledScript.Exports.GetType().GetMethods()) {
					Methods.Add(item.Name, CreateDelegate(item, CompiledScript.Exports));
				}
				if (Methods.ContainsKey("main")) {
#if DEBUG
					RLog.Info("Wasm Running Main");
#endif
					var rturnData = InvokeMethodNoCompileCheck("main");
					if (rturnData is int code) {
						if (code != 0) {
							throw new Exception($"Error Building Code:{code}");
						}
					}
				}
				IsCompiled = true;
#if DEBUG
				RLog.Info("Wasm Compiled");
#endif
			});
		}

		public void CleanUp() {
			IsCompiled = false;
			_refs.Clear();
			_deadRefs.Clear();
			if (CompiledScript is null) {
				return;
			}
			foreach (var item in GetWasmTasks()) {
				if (!(item.Task.IsCompleted || item.Task.IsCanceled)) {
					throw new Exception("Still in use");
				}
			}
			Methods.Clear();
			CompiledScript.Dispose();
			CompiledScript = null;
#if DEBUG
			RLog.Info("Wasm CleanUp");
#endif
		}

		public WasmRunner wasmRunner;

		public void Dispose() {
			CleanUp();
		}
	}
}
