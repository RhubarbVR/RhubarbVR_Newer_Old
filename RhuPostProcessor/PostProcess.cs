using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace RhuPostProcessor
{
	public class PostProcess
	{
		public static Action<string>? logAction;

		public static void Log(string logstring) {
			if (logAction is null) {
				Console.WriteLine(logstring);
			}
			else {
				logAction(logstring);
			}
		}

		public (AssemblyDefinition, bool) GetAssemblyFromDLLPath(string targetDLL, string[] extraDlls) {
			var defaultAssemblyResolver = new DefaultAssemblyResolver();
			defaultAssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(targetDLL));
			foreach (var item in extraDlls) {
				defaultAssemblyResolver.AddSearchDirectory(Path.GetFullPath(item));
			}
			var pdbFileExists = File.Exists(Path.ChangeExtension(targetDLL, ".pdb"));
			return (AssemblyDefinition.ReadAssembly(targetDLL, new ReaderParameters {
				AssemblyResolver = defaultAssemblyResolver,
				ReadSymbols = pdbFileExists,
				SymbolReaderProvider = pdbFileExists ? new PdbReaderProvider() : null,
				ReadWrite = true
			}), pdbFileExists);
		}

		public class FieldInfoForMethod
		{
			public FieldDefinition Field;
			public FieldInfoForMethod(FieldDefinition field) {
				Field = field;
			}

			public CustomAttribute? NoLoadAttribute;
			public CustomAttribute? NoSaveAttribute;
			public CustomAttribute? NoSyncAttribute;
			public CustomAttribute? BindPropertyAttribute;
			public CustomAttribute? DefaultAttribute;
			public CustomAttribute? OnAssetLoadedAttribute;
			public CustomAttribute? OnChangedAttribute;
			public CustomAttribute? NoSyncUpdateAttribute;
		}
		public List<AddILJob> addILJobs = new List<AddILJob>();
		public void BuildInitializeMembersMethod(TypeDefinition typeDefinition, List<FieldInfoForMethod> fieldInfos) {
			var newMethod = new MethodDefinition("InitializeMembers", MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, voidType);
			newMethod.Parameters.Add(new ParameterDefinition("networkedObject", ParameterAttributes.None, boolType));
			newMethod.Parameters.Add(new ParameterDefinition("deserialize", ParameterAttributes.None, boolType));
			newMethod.Parameters.Add(new ParameterDefinition("netPointer", ParameterAttributes.None, funcNetPointerType));
			typeDefinition.Methods.Add(newMethod);
			newMethod.Body.InitLocals = true;
			addILJobs.Add(new AddILJob(typeDefinition, fieldInfos, newMethod));
		}
		public class AddILJob
		{
			readonly TypeDefinition _typeDefinition;
			readonly List<FieldInfoForMethod> _fieldInfos;
			readonly MethodDefinition _newMethod;
			public AddILJob(TypeDefinition typeDefinition, List<FieldInfoForMethod> fieldInfos, MethodDefinition newMethod) {
				_typeDefinition = typeDefinition;
				_fieldInfos = fieldInfos;
				_newMethod = newMethod;
			}

			public void BuildIL(ModuleDefinition moduleDefinition,TypeReference actionWithChangeable,TypeReference voidType,TypeReference changableType, TypeReference action) {
				Log($"Starting building IL for Type:{_typeDefinition.Name}");
				var iLProcessor = _newMethod.Body.GetILProcessor();
				if (_typeDefinition.BaseType.FullName != "RhuEngine.WorldObjects.SyncObject") {
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_2));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_3));
					iLProcessor.Append(Instruction.Create(OpCodes.Call, _typeDefinition.BaseType.Resolve().Methods.Where((x) => x.Name == "InitializeMembers").First()));
				}
				foreach (var item in _fieldInfos) {
					Log($"Starting loading IL for Field {item.Field.Name} Type:{_typeDefinition.Name}");
					var constructor = item.Field.FieldType.GetConstructor(moduleDefinition, out var methodCall);
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					if (methodCall) {
						iLProcessor.Append(Instruction.Create(OpCodes.Call, constructor));
					}
					else {
						iLProcessor.Append(Instruction.Create(OpCodes.Newobj, constructor));
					}
					iLProcessor.Append(Instruction.Create(OpCodes.Stfld, item.Field));

					//Initialize
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Call, _typeDefinition.Resolve().AllMethods().Where((x) => x.Name == "get_World").First()));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(((object)item.Field.Name).GetInstructionForEvaluationStack());
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_2));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_3));
					iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, _typeDefinition.Resolve().AllMethods().Where((x) => x.Name == "Initialize").First()));

					//AddDisposable
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
					iLProcessor.Append(Instruction.Create(OpCodes.Call, _typeDefinition.Resolve().AllMethods().Where((x) => x.Name == "AddDisposable").First()));

					if (item.DefaultAttribute != null) {
						if (item.DefaultAttribute.HasConstructorArguments) {
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
							var data = item.DefaultAttribute.ConstructorArguments.First().Value;
							iLProcessor.Append(data.GetInstructionForEvaluationStack());
							iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name == "set_Value").First()));
						}
					}
					if (item.OnChangedAttribute != null) {
						if (item.OnChangedAttribute.HasConstructorArguments) {
							var targetMethod = (string)item.OnChangedAttribute.ConstructorArguments.First().Value;
							var method = _typeDefinition.AllMethods().Where((x) => x.Name == targetMethod).First();
							if (method.Parameters.Count == 0) {
								var addedmethod = _typeDefinition.Methods.Where((x) => x.Name == method.Name + "_Gen_AddedPram").FirstOrDefault();
								if (addedmethod == null) {
									addedmethod = new MethodDefinition(method.Name + "_Gen_AddedPram", MethodAttributes.Private, voidType);
									addedmethod.Parameters.Add(new ParameterDefinition("temp", ParameterAttributes.None, changableType));
									addedmethod.Body.InitLocals = true;
									var iLProcessor2 = addedmethod.Body.GetILProcessor();
									iLProcessor2.Append(Instruction.Create(OpCodes.Ldarg_0));
									iLProcessor2.Append(Instruction.Create(OpCodes.Call, method));
									iLProcessor2.Append(Instruction.Create(OpCodes.Ret));
									_typeDefinition.Methods.Add(addedmethod);
								}

								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, addedmethod));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, actionWithChangeable.GetConstructor(moduleDefinition, out _, true)));
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name == "add_Changed").First()));
							}
							else if (method.Parameters.Count == 1) {
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn,method));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, actionWithChangeable.GetConstructor(moduleDefinition, out _,true)));
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name == "add_Changed").First()));
							}
							else {
								throw new NotSupportedException($"Method {method.Name} can not be used");
							}
						}
					}
					if (item.OnAssetLoadedAttribute != null) {
						if (item.OnAssetLoadedAttribute.HasConstructorArguments) {
							var targetMethod = (string)item.OnAssetLoadedAttribute.ConstructorArguments.First().Value;
							var method = _typeDefinition.AllMethods().Where((x) => x.Name == targetMethod).First();
							if (method.Parameters.Count == 0) {
								var addedmethod = _typeDefinition.Methods.Where((x) => x.Name == method.Name + "_Gen_AddedPram").FirstOrDefault();
								if (addedmethod == null) {
									addedmethod = new MethodDefinition(method.Name + "_Gen_AddedPram", MethodAttributes.Private, voidType);
									addedmethod.Parameters.Add(new ParameterDefinition("temp", ParameterAttributes.None, ((GenericInstanceType)item.Field.FieldType).GenericArguments[0]));
									addedmethod.Body.InitLocals = true;
									var iLProcessor2 = addedmethod.Body.GetILProcessor();
									iLProcessor2.Append(Instruction.Create(OpCodes.Ldarg_0));
									iLProcessor2.Append(Instruction.Create(OpCodes.Call, method));
									iLProcessor2.Append(Instruction.Create(OpCodes.Ret));
									_typeDefinition.Methods.Add(addedmethod);
								}

								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, addedmethod));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, action.MakeGenericInstanceType(((GenericInstanceType)item.Field.FieldType).GenericArguments[0]).GetConstructor(moduleDefinition, out _, true)));
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name.Contains("add_LoadChange")).First()));
							}
							else if (method.Parameters.Count == 1) {
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, method));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, action.MakeGenericInstanceType(((GenericInstanceType)item.Field.FieldType).GenericArguments[0]).GetConstructor(moduleDefinition, out _, true)));
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name.Contains("add_LoadChange")).First()));
							}
							else {
								throw new NotSupportedException($"Method {method.Name} can not be used");
							}
						}
					}
					if(item.BindPropertyAttribute != null) {
						if (item.BindPropertyAttribute.HasConstructorArguments) {
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
							iLProcessor.Append(item.BindPropertyAttribute.ConstructorArguments.First().GetInstructionForEvaluationStack());
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							iLProcessor.Append(Instruction.Create(OpCodes.Callvirt,item.Field.FieldType.Resolve().AllMethods().Where((x)=>x.Name=="Bind").First()));
						}
					}
					if (item.NoSyncUpdateAttribute != null) {
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, item.Field));
							iLProcessor.Append(((object)true).GetInstructionForEvaluationStack());
							iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name == "set_NoSync").First()));
					}

				}

				iLProcessor.Append(Instruction.Create(OpCodes.Ret));
				Log($"Done With IL for Type:{_typeDefinition.Name}");
			}
		}


		public void SetUpInitializeMembersMethod(TypeDefinition typeDefinition) {
			var fieldsToAddToMethod = new List<FieldInfoForMethod>();
			foreach (var field in typeDefinition.Fields) {
				if (!(field.IsPublic | field.IsInitOnly)) {
					continue;
				}
				try {
					if (!IsBasedOff(field.FieldType.Resolve())) {
						continue;
					}
				}
				catch {
					Log($"Field Failed to load {field.Name}");
					continue;
				}
				CustomAttribute? NoLoadAttribute = null;
				CustomAttribute? NoSaveAttribute = null;
				CustomAttribute? NoSyncAttribute = null;
				CustomAttribute? BindPropertyAttribute = null;
				CustomAttribute? DefaultAttribute = null;
				CustomAttribute? OnAssetLoadedAttribute = null;
				CustomAttribute? OnChangedAttribute = null;
				CustomAttribute? NoSyncUpdateAttribute = null;
				foreach (var attribute in field.CustomAttributes) {
					switch (attribute.AttributeType.Name) {
						case "NoLoadAttribute":
							NoLoadAttribute = attribute;
							break;
						case "NoSaveAttribute":
							NoSaveAttribute = attribute;
							break;
						case "NoSyncAttribute":
							NoSyncAttribute = attribute;
							break;
						case "BindPropertyAttribute":
							BindPropertyAttribute = attribute;
							break;
						case "DefaultAttribute":
							DefaultAttribute = attribute;
							break;
						case "OnAssetLoadedAttribute":
							OnAssetLoadedAttribute = attribute;
							break;
						case "OnChangedAttribute":
							OnChangedAttribute = attribute;
							break;
						case "NoSyncUpdateAttribute":
							NoSyncUpdateAttribute = attribute;
							break;
						default:
							break;
					}
				}
				if (NoLoadAttribute is null) {
					Log($"Adding Field Proccesed field: {field.Name} type {typeDefinition.Name}");
					Log($"HasNoSaveAttribute:{NoSaveAttribute != null} HasNoSyncAttribute:{NoSyncAttribute != null} HasBindPropertyAttribute:{BindPropertyAttribute != null} HasDefaultAttribute:{DefaultAttribute != null} HasOnChangedAttribute:{OnChangedAttribute != null} HasNoSyncUpdateAttribute{NoSyncUpdateAttribute != null} HasOnAssetLoadedAttribute{OnAssetLoadedAttribute != null}");
					fieldsToAddToMethod.Add(new FieldInfoForMethod(field) {
						NoLoadAttribute = NoLoadAttribute,
						NoSaveAttribute = NoSaveAttribute,
						NoSyncAttribute = NoSyncAttribute,
						BindPropertyAttribute = BindPropertyAttribute,
						DefaultAttribute = DefaultAttribute,
						OnAssetLoadedAttribute = OnAssetLoadedAttribute,
						OnChangedAttribute = OnChangedAttribute,
						NoSyncUpdateAttribute = NoSyncUpdateAttribute
					});
				}
				else {
					Log($"Skipped Proccesed field: {field.Name} type {typeDefinition.Name}");
				}
			}
			Log("Loaded Allfields Now MakingMethod");
			BuildInitializeMembersMethod(typeDefinition, fieldsToAddToMethod);
		}

		public bool IsBasedOff(TypeDefinition typeDefinition, string type = "RhuEngine.WorldObjects.SyncObject") {
			var parrentType = typeDefinition.BaseType;
			return parrentType != null && (parrentType.FullName == type || IsBasedOff(parrentType.Resolve(), type));
		}
		public TypeReference? voidType;
		public TypeReference? boolType;
		public TypeReference? netPointerType;
		public TypeReference? actionType;
		public TypeReference? changableType;

		public TypeReference? funcNetPointerType;
		public TypeReference? syncObject;
		public TypeReference? actionWithChangeable;

		public void ProcessDLL(string targetDLL, string[] extraDlls) {
			if (!File.Exists(targetDLL)) {
				return;
			}
			var (assembly, pdbFileExists) = GetAssemblyFromDLLPath(targetDLL, extraDlls);
			var module = assembly.MainModule;
			var getTypes = module.GetTypes().ToList();
			voidType = module.ImportReference(typeof(void));
			boolType = module.ImportReference(typeof(bool));
			syncObject = module.GetType("RhuEngine.WorldObjects.SyncObject", runtimeName: true);
			actionType = module.ImportReference(typeof(RNumerics.RhuAction<>));
			netPointerType = module.GetType("RhuEngine.Datatypes.NetPointer", runtimeName: true);
			changableType = module.GetType("RhuEngine.WorldObjects.IChangeable", runtimeName: true);
			Log($"LoadedType {netPointerType.Name}");
			funcNetPointerType = module.GetType("RhuEngine.WorldObjects.NetPointerUpdateDelegate", runtimeName: true);
			Log($"LoadedType {funcNetPointerType.FullName}");
			actionWithChangeable = actionType.MakeGenericInstanceType(changableType);
			foreach (var type in getTypes) {
				if (type.BaseType is null) {
					continue;
				}
				if (!IsBasedOff(type)) {
					continue;
				}
				var hasInitializeMembersMethod = false;
				foreach (var method in type.Methods) {
					if (method.Name == "InitializeMembers") {
						hasInitializeMembersMethod = true;
						continue;
					}
				}
				if (!hasInitializeMembersMethod) {
					Log($"Starting Making InitializeMembersMethod on {type.Name}");
					SetUpInitializeMembersMethod(type);
					Log($"Done Making InitializeMembersMethod on {type.Name}");
				}
				else {
					Log($"Already Had InitializeMembersMethod on {type.Name}");
				}
			}

			Log("Running IL Building");
			foreach (var item in addILJobs) {
				item.BuildIL(module, actionWithChangeable, voidType, changableType,actionType);
			}
			Log("DoneWith IL Building");
			Log("Removing InitializeMembers from SyncObject");
			var replaceMethod = module.GetType("RhuEngine.WorldObjects.SyncObject", runtimeName: true).Resolve().Methods.Where(x => x.Name == "InitializeMembers").First();
			var ilProcess = replaceMethod.Body.GetILProcessor();
			ilProcess.Clear();
			ilProcess.Append("InitializeMembers Not Set up".GetInstructionForEvaluationStack());
			ilProcess.Append(Instruction.Create(OpCodes.Newobj, module.GetType("RhuEngine.RhuException").Resolve().Methods.Where((x) => x.IsConstructor && x.Parameters.Count == 1).First()));
			ilProcess.Append(Instruction.Create(OpCodes.Throw));
			assembly.Write(new WriterParameters {
				WriteSymbols = pdbFileExists,
				SymbolWriterProvider = pdbFileExists ? new PdbWriterProvider() : null
			});
			assembly.Dispose();
		}

	}
}
