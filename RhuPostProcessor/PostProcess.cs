using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;

namespace RhuPostProcessor
{
	public sealed class PostProcess
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

			public void BuildIL(ModuleDefinition moduleDefinition, TypeReference actionWithChangeable, TypeReference voidType, TypeReference changableType, TypeReference action) {
				Log($"Starting building IL for Type:{_typeDefinition.Name}");


				_newMethod.Body.InitLocals = true;
				_newMethod.Body.SimplifyMacros();
				var iLProcessor = _newMethod.Body.GetILProcessor();
				iLProcessor.Append(Instruction.Create(OpCodes.Nop));

				if (_typeDefinition.BaseType.FullName != "RhuEngine.WorldObjects.SyncObject") {
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_2));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_3));
					var baseCHildMethod = (MethodReference)_typeDefinition.BaseType.Resolve().Methods.Where((x) => x.Name == "InitializeMembers").First();
					var fialMethod = moduleDefinition.ImportReference(baseCHildMethod, _typeDefinition);
					if (_typeDefinition.BaseType is GenericInstanceType genericInstanceType) {
						fialMethod = moduleDefinition.ImportReference(baseCHildMethod.MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
					}
					iLProcessor.Append(Instruction.Create(OpCodes.Call, fialMethod));
				}
				foreach (var item in _fieldInfos) {
					var filedRef = (FieldReference)item.Field.GetGenericFieldReference();
					Log($"Starting loading IL for Field {item.Field.Name} Type:{_typeDefinition.Name}");
					var constructor = item.Field.FieldType.GetConstructor(moduleDefinition, out var methodCall);
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					if (methodCall) {
						iLProcessor.Append(Instruction.Create(OpCodes.Call, moduleDefinition.ImportReference(constructor, _typeDefinition)));
					}
					else {
						iLProcessor.Append(Instruction.Create(OpCodes.Newobj, moduleDefinition.ImportReference(constructor, _typeDefinition)));
					}
					iLProcessor.Append(Instruction.Create(OpCodes.Stfld, filedRef));

					//Initialize
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Call, moduleDefinition.ImportReference((MethodReference)_typeDefinition.Resolve().AllProperties().Where((x) => x.Name == "World").First().GetMethod.Resolve(), _typeDefinition)));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(((object)item.Field.Name).GetInstructionForEvaluationStack());
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_2));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_3));
					iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, moduleDefinition.ImportReference((MethodReference)_typeDefinition.Resolve().AllMethods().Where((x) => x.Name == "Initialize").First().Resolve(), _typeDefinition)));

					//AddDisposable
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
					iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
					iLProcessor.Append(Instruction.Create(OpCodes.Call, moduleDefinition.ImportReference(_typeDefinition.Resolve().AllMethods().Where((x) => x.Name == "AddDisposable").First(), _typeDefinition)));

					if (item.DefaultAttribute != null) {
						if (item.DefaultAttribute.HasConstructorArguments) {
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
							var data = item.DefaultAttribute.ConstructorArguments.First().Value;
							iLProcessor.Append(data.GetInstructionForEvaluationStack());
							var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AllProperties().Where((x) => x.Name == "Value").First().SetMethod.Resolve(), _typeDefinition);
							if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
								fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AllProperties().Where((x) => x.Name == "Value").First().SetMethod.Resolve().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
							}
							iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
						}
					}
					if (item.OnChangedAttribute != null) {
						if (item.OnChangedAttribute.HasConstructorArguments) {
							var targetMethod = (string)item.OnChangedAttribute.ConstructorArguments.First().Value;
							var method = _typeDefinition.AllMethods().Where((x) => x.Name == targetMethod).First();
							if (method.Parameters.Count == 0) {
								var addedmethod = _typeDefinition.Methods.Where((x) => x.Name == method.Name + "_Gen_AddedPram").FirstOrDefault();
								if (addedmethod == null) {
									addedmethod = new MethodDefinition(method.Name + "_Gen_AddedPram", MethodAttributes.Private | MethodAttributes.Family, voidType);
									addedmethod.Parameters.Add(new ParameterDefinition("temp", ParameterAttributes.None, changableType));
									addedmethod.Body.InitLocals = true;
									addedmethod.Body.SimplifyMacros();
									var iLProcessor2 = addedmethod.Body.GetILProcessor();
									iLProcessor2.Append(Instruction.Create(OpCodes.Ldarg_0));
									iLProcessor2.Append(Instruction.Create(OpCodes.Call, moduleDefinition.ImportReference(method.MakeHostInstanceGeneric(filedRef.DeclaringType), _typeDefinition)));
									iLProcessor2.Append(Instruction.Create(OpCodes.Ret));
									_typeDefinition.Methods.Add(addedmethod);
									addedmethod.Body.Optimize();
								}

								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, moduleDefinition.ImportReference(addedmethod.MakeHostInstanceGeneric(filedRef.DeclaringType), _typeDefinition)));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, moduleDefinition.ImportReference(actionWithChangeable.GetConstructor(moduleDefinition, out _, true), _typeDefinition)));
								var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "Changed").First().AddMethod.Resolve(), _typeDefinition);
								if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
									fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "Changed").First().AddMethod.Resolve().Resolve().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
								}
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
							}
							else if (method.Parameters.Count == 1) {
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));

								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, moduleDefinition.ImportReference(method, _typeDefinition)));

								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, moduleDefinition.ImportReference(actionWithChangeable.GetConstructor(moduleDefinition, out _, true), _typeDefinition)));
								var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "Changed").First().AddMethod.Resolve(), _typeDefinition);
								if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
									fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "Changed").First().AddMethod.Resolve().Resolve().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
								}
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
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
									addedmethod = new MethodDefinition(method.Name + "_Gen_AddedPram", MethodAttributes.Private | MethodAttributes.Family, voidType);
									addedmethod.Parameters.Add(new ParameterDefinition("temp", ParameterAttributes.None, ((GenericInstanceType)item.Field.FieldType).GenericArguments[0]));
									addedmethod.Body.InitLocals = true;

									addedmethod.Body.SimplifyMacros();
									var iLProcessor2 = addedmethod.Body.GetILProcessor();
									iLProcessor2.Append(Instruction.Create(OpCodes.Ldarg_0));
									iLProcessor2.Append(Instruction.Create(OpCodes.Call, moduleDefinition.ImportReference(method.MakeHostInstanceGeneric(filedRef.DeclaringType), _typeDefinition)));
									iLProcessor2.Append(Instruction.Create(OpCodes.Ret));
									addedmethod.Body.Optimize();

									_typeDefinition.Methods.Add(addedmethod);
								}

								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, moduleDefinition.ImportReference(filedRef, _typeDefinition)));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, moduleDefinition.ImportReference(addedmethod.MakeHostInstanceGeneric(filedRef.DeclaringType), _typeDefinition)));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, moduleDefinition.ImportReference(action.MakeGenericInstanceType(((GenericInstanceType)item.Field.FieldType).GenericArguments[0]).GetConstructor(moduleDefinition, out _, true), _typeDefinition)));
								var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "LoadChange").First().AddMethod.Resolve(), _typeDefinition);
								if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
									fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "LoadChange").First().AddMethod.Resolve().Resolve().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
								}
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
							}
							else if (method.Parameters.Count == 1) {
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
								iLProcessor.Append(Instruction.Create(OpCodes.Ldftn, moduleDefinition.ImportReference(method.MakeHostInstanceGeneric(filedRef.DeclaringType), _typeDefinition)));
								iLProcessor.Append(Instruction.Create(OpCodes.Newobj, moduleDefinition.ImportReference(action.MakeGenericInstanceType(((GenericInstanceType)item.Field.FieldType).GenericArguments[0]).GetConstructor(moduleDefinition, out _, true), _typeDefinition)));
								var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "LoadChange").First().AddMethod.Resolve(), _typeDefinition);
								if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
									fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AlleEvents().Where((x) => x.Name == "LoadChange").First().AddMethod.Resolve().Resolve().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
								}
								iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
							}
							else {
								throw new NotSupportedException($"Method {method.Name} can not be used");
							}
						}
					}
					if (item.BindPropertyAttribute != null) {
						if (item.BindPropertyAttribute.HasConstructorArguments) {
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
							iLProcessor.Append(item.BindPropertyAttribute.ConstructorArguments.First().GetInstructionForEvaluationStack());
							iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
							var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name == "Bind").First(), _typeDefinition);
							if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
								fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AllMethods().Where((x) => x.Name == "Bind").First().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
							}
							iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
						}
					}
					if (item.NoSyncUpdateAttribute != null) {
						iLProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
						iLProcessor.Append(Instruction.Create(OpCodes.Ldfld, filedRef));
						iLProcessor.Append(((object)true).GetInstructionForEvaluationStack());
						var fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AllProperties().Where((x) => x.Name == "NoSync").First().SetMethod.Resolve(), _typeDefinition);
						if (item.Field.FieldType is GenericInstanceType genericInstanceType) {
							fialMethod = moduleDefinition.ImportReference(item.Field.FieldType.Resolve().AllProperties().Where((x) => x.Name == "NoSync").First().SetMethod.Resolve().MakeHostInstanceGeneric(genericInstanceType), _typeDefinition);
						}
						iLProcessor.Append(Instruction.Create(OpCodes.Callvirt, fialMethod));
					}

				}

				iLProcessor.Append(Instruction.Create(OpCodes.Ret));
				_newMethod.Body.Optimize();
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

		public bool ProcessDLL(string targetDLL, string[] extraDlls) {
			if (!File.Exists(targetDLL)) {
				Log("targetDLL did not exists");
				return false;
			}
			var (assembly, pdbFileExists) = GetAssemblyFromDLLPath(targetDLL, extraDlls);
			var module = assembly.MainModule;
			var getTypes = module.GetTypes().ToList();
			voidType = module.ImportReference(typeof(void));
			boolType = module.ImportReference(typeof(bool));
			syncObject = module.GetType("RhuEngine.WorldObjects.SyncObject", runtimeName: true);
			actionType = module.ImportReference(typeof(System.Action<>));
			netPointerType = module.GetType("RhuEngine.Datatypes.NetPointer", runtimeName: true);
			changableType = module.GetType("RhuEngine.WorldObjects.IChangeable", runtimeName: true);
			Log($"LoadedType {netPointerType.Name}");
			funcNetPointerType = module.GetType("RhuEngine.WorldObjects.NetPointerUpdateDelegate", runtimeName: true);
			Log($"LoadedType {funcNetPointerType.FullName}");
			actionWithChangeable = actionType.MakeGenericInstanceType(changableType);
			foreach (var item in assembly.MainModule.CustomAttributes) {
				foreach (var constructorArgument in item.ConstructorArguments) {
					if (constructorArgument.Value is string value) {
						if (value == "RHU_OPTIMIZED") {
							Log("Allready Has Been Prossed");
							assembly.Dispose();
							return true;
						}
					}
				}
			}


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
				item.BuildIL(module, actionWithChangeable, voidType, changableType, actionType);
			}
			Log("DoneWith IL Building");
			Log("Removing InitializeMembers from SyncObject");
			var replaceMethod = module.GetType("RhuEngine.WorldObjects.SyncObject", runtimeName: true).Resolve().Methods.Where(x => x.Name == "InitializeMembers").First();
			var ilProcess = replaceMethod.Body.GetILProcessor();
			ilProcess.Body.SimplifyMacros();
			ilProcess.Clear();
			ilProcess.Append("InitializeMembers Not Set up".GetInstructionForEvaluationStack());
			ilProcess.Append(Instruction.Create(OpCodes.Newobj, module.GetType("RhuEngine.RhuException").Resolve().Methods.Where((x) => x.IsConstructor && x.Parameters.Count == 1).First()));
			ilProcess.Append(Instruction.Create(OpCodes.Throw));
			ilProcess.Body.Optimize();
			var customAttribute = new CustomAttribute(module.ImportReference(typeof(DescriptionAttribute).GetConstructor(new Type[1] { typeof(string) })));
			customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference(typeof(string)), "RHU_OPTIMIZED"));
			module.CustomAttributes.Add(customAttribute);
			assembly.Write(new WriterParameters {
				WriteSymbols = pdbFileExists,
				SymbolWriterProvider = pdbFileExists ? new PdbWriterProvider() : null
			});
			assembly.Dispose();
			return true;
		}

	}
}
