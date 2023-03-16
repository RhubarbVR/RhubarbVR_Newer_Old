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
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var list = new List<byte>();
			for (var i = 0; i < CompiledScript.Exports.memory.Size - targetAdress; i++) {
				var currentData = ReadByte(targetAdress + i);
				if (currentData == 0) {
					list.Add(currentData);
					return MakeRef(Encoding.UTF8.GetString(list.ToArray()));
				}
				else {
					list.Add(currentData);
				}
			}
			return -1;
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

		public static int MakeHash(MethodInfo methodInfo) {
			HashCode hash = new();
			foreach (var item in methodInfo.GetParameters()) {
				hash.Add(item.ParameterType.Name);
			}
			return hash.ToHashCode();
		}

		/// <summary>
		/// Gets method to been called
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="methodName"></param>
		/// <param name="argumentCount"></param>
		/// <param name="makeHash"></param>
		/// <param name="genericTypes"></param>
		/// <returns></returns>
		public int GetMethod(int typeName, int methodName, int argumentCount, int makeHash, int genericTypes) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetObjectRef<string>(typeName);
			var methodNameString = GetObjectRef<string>(methodName);
			var genericTypesString = GetObjectRef<string>(genericTypes);
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
				.Where(x => x.IsGenericMethod ? genericTypesString is not null : genericTypesString is null)
				.FirstOrDefault();
			if (targetMethod is null) {
				return -1;
			}
			if (genericTypesString is not null) {
				var types = genericTypesString.Split(',').Select(x => FamcyTypeParser.PraseType(x)).ToArray();
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
		/// Gets field to that is callable
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public int GetField(int typeName, int fieldName) {
			if (CompiledScript?.Exports is null) {
				return -1;
			}
			var typeNameString = GetObjectRef<string>(typeName);
			var targetType = FamcyTypeParser.PraseType(typeNameString);
			if (targetType is null) {
				return -1;
			}
			var fieldNameString = GetObjectRef<string>(fieldName);
			var targetField = targetType.GetFields()
				.Where(x => x.Name == fieldNameString)
				.Where(x => (x.GetCustomAttribute<ExposedAttribute>(true) is not null) || x.FieldType.IsAssignableTo(typeof(IWorldObject)))
				.Where(x => x.GetCustomAttribute<UnExsposedAttribute>(true) is null)
				.FirstOrDefault();
			if (targetField is null) {
				return -1;
			}
			return MakeRef((Delegate)(Func<object, object>)targetField.GetValue);
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
				.Where(x => x.CanRead)
				.Where(x => x.GetMethod is not null)
				.FirstOrDefault();
			if (targetProperty is null) {
				return -1;
			}
			return MakeRef(Delegate.CreateDelegate(targetProperty.GetMethod.CreateDelegateTypeWithObject(), targetProperty.GetMethod));
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
			var addedOffset = 0;
			for (var i = 0; i < prams.Length; i++) {
				var targetType = prams[i].ParameterType;
				data[i] = ReadData(argments + i + addedOffset, targetType);
				if (targetType == typeof(long) || targetType == typeof(ulong) || targetType == typeof(double)) {
					addedOffset++;
				}
			}
			return MakePrem(ConvertType(getMethod.DynamicInvoke(data)));
		}

		private long MakePrem(object target) {
			return target is int @int
				? (long)@int
				: target is float @float
				? BitConverter.ToInt32(BitConverter.GetBytes(@float))
				: target is long @long
				? @long
				: target is double @double
				? BitConverter.ToInt64(BitConverter.GetBytes(@double)) 
				: throw new Exception("Not supported Type");
		}

		private object ReadData(int readingAdress, Type type) {
			var adress = ReadInt(readingAdress);
			if (type == typeof(int)) {
				return (object)adress;
			}
			else if (type == typeof(uint)) {
				return (object)(uint)adress;
			}
			else if (type == typeof(bool)) {
				return adress == 1;
			}
			else if (type == typeof(byte)) {
				return (byte)adress;
			}
			else if (type == typeof(sbyte)) {
				return (sbyte)adress;
			}
			else if (type == typeof(short)) {
				return (short)adress;
			}
			else if (type == typeof(ushort)) {
				return (ushort)adress;
			}
			else if (type == typeof(char)) {
				return (char)adress;
			}
			else if (type == typeof(float)) {
				return BitConverter.ToSingle(BitConverter.GetBytes(adress));
			}
			else if (type == typeof(long)) {
				return ReadInt64(readingAdress);
			}
			else if (type == typeof(ulong)) {
				return (ulong)ReadInt64(readingAdress);
			}
			else if (type == typeof(double)) {
				return (ulong)BitConverter.ToDouble(BitConverter.GetBytes(ReadInt64(readingAdress)));
			}
			return GetObjectRef(adress);
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
				? data
				: currentType == typeof(float)
				? data
				: currentType == typeof(long)
				? data
				: currentType == typeof(double)
				? data
				: currentType == typeof(char)
				? (int)(char)data
				: currentType == typeof(byte)
				? (int)(byte)data
				: currentType == typeof(sbyte)
				? (int)(sbyte)data
				: currentType == typeof(uint)
				? (int)(uint)data
				: currentType == typeof(short)
				? (int)(short)data
				: currentType == typeof(ushort)
				? (int)(ushort)data
				: currentType == typeof(ulong)
				? (long)(ulong)data
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
						{ "method_bind", new FunctionImport(GetMethod) },
						{ "field_bind", new FunctionImport(GetField) },
						{ "property_get_bind", new FunctionImport(GetPropertyGetter) },
						{ "property_set_bind", new FunctionImport(GetPropertySetter) },
						{ "call_method", new FunctionImport(CallMethod) },
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
