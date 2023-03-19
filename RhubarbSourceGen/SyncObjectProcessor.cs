using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhubarbSourceGen
{
	public static class SyncObjectProcessor
	{
		public static void Build(GeneratorExecutionContext context) {
			var syncObjects = context.GetAllSyncObjects();
			var targetWorldType = context.GetType("World");
			foreach (var syncObject in syncObjects.Concat(new[] { targetWorldType })) {
				var name = syncObject.Identifier.Text;
				if (name == "SyncObject") {
					continue;
				}
				var isWorldBuild = name == "World";
				var hasAnyLoadClasses = false;
				var loadClasses = new StringBuilder();
				var initializeMembersMethod = new StringBuilder();
				var addedGenMethods = new List<string>();
				var addedInnerClassData = new StringBuilder();
				if (isWorldBuild) {
					initializeMembersMethod.AppendLine("protected void InitializeMembers(bool networkedObject, bool deserialize, NetPointerUpdateDelegate netPointer) {");
				}
				else {
					initializeMembersMethod.AppendLine("protected override void InitializeMembers(bool networkedObject, bool deserialize, NetPointerUpdateDelegate netPointer) {");
				}
				var nameSpace = syncObject.GetNamespaceWithClass();
				foreach (var field in syncObject.Members.Where(x => x is FieldDeclarationSyntax).Cast<FieldDeclarationSyntax>()) {
					var isFieldSyncObject = context.GetHasClassOrIterface(context.GetType(field.Declaration.Type.ToString()), "SyncObject");
					if (!isFieldSyncObject) {
						continue;
					}
					if (field.HasAtrubute("NoLoadAttribute")) {
						continue;
					}
					if (field.HasAtrubute("NoSaveAttribute") && field.HasAtrubute("NoSyncAttribute")) {
						continue;
					}

					var modify = field.Modifiers.ToString();
					if (!(modify.Contains("public") && modify.Contains("readonly"))) {
						continue;
					}
					loadClasses.AppendLine($"\t\t{field.Declaration.Variables} = new();");
					if (isWorldBuild) {
						initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.Initialize(this, this, nameof({field.Declaration.Variables}), networkedObject, deserialize, netPointer);");
					}
					else {
						initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.Initialize(World, this, nameof({field.Declaration.Variables}), networkedObject, deserialize, netPointer);");
					}
					initializeMembersMethod.AppendLine($"\tAddDisposable({field.Declaration.Variables});");

					var isISyncProperty = context.GetHasClassOrIterface(context.GetType(field.Declaration.Type.ToString()), "ISyncProperty");
					if (isISyncProperty) {
						var bindPropertyAttribute = field.GetAtrubute("BindPropertyAttribute");
						if (bindPropertyAttribute is not null) {
							initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.Bind({bindPropertyAttribute.ArgumentList.Arguments[0]}, this);");
						}
					}
					var isIAssetRef = context.GetHasClassOrIterface(context.GetType(field.Declaration.Type.ToString()), "IAssetRef");
					if (isIAssetRef) {
						var onAssetLoadedAttribute = field.GetAtrubute("OnAssetLoadedAttribute");
						if (onAssetLoadedAttribute is not null) {
							var targetMethodName = onAssetLoadedAttribute.ArgumentList.Arguments.First().ToString();
							if (targetMethodName.StartsWith("nameof(")) {
								targetMethodName = targetMethodName.Substring(7);
								targetMethodName = targetMethodName.Remove(targetMethodName.Length - 1);
							}
							initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.LoadChange += {targetMethodName};");
						}
					}
					var isISync = context.GetHasClassOrIterface(context.GetType(field.Declaration.Type.ToString()), "ISync");
					if (isISync) {
						var defultValue = field.GetAtrubute("DefaultAttribute");
						if (defultValue is not null) {
							initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.ChangeStartingValue({defultValue.ArgumentList.Arguments.First()});");
						}
						initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.SetStartingValue();");
					}
					var isINetworkedObject = context.GetHasClassOrIterface(context.GetType(field.Declaration.Type.ToString()), "INetworkedObject");
					if (isINetworkedObject) {
						if (field.HasAtrubute("NoSyncUpdateAttribute")) {
							initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.NoSync = true;");
						}
					}
					var isIChangeable = context.GetHasClassOrIterface(context.GetType(field.Declaration.Type.ToString()), "IChangeable");
					if (isIChangeable) {
						var onChangedAttribute = field.GetAtrubute("OnChangedAttribute");
						if (onChangedAttribute is not null) {
							var targetMethodName = onChangedAttribute.ArgumentList.Arguments.First().ToString();
							if (targetMethodName.StartsWith("nameof(")) {
								targetMethodName = targetMethodName.Substring(7);
								targetMethodName = targetMethodName.Remove(targetMethodName.Length - 1);
							}
							void AddMethod(MethodDeclarationSyntax methodDeclarationSyntax) {
								var hasPram = methodDeclarationSyntax.ParameterList.Parameters.Count == 1;
								if (hasPram) {
									initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.Changed += {targetMethodName};");
								}
								else {
									if (!addedGenMethods.Contains(targetMethodName)) {
										addedInnerClassData.AppendLine($"\tprivate void {targetMethodName}_AddedPram_Gen(IChangeable change) {{\n\t\t{targetMethodName}();\n\t}}");
										addedGenMethods.Add(targetMethodName);
									}
									initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.Changed += {targetMethodName}_AddedPram_Gen;");
								}
							}
							var targetMethods = syncObject.Members.Where(x => x is MethodDeclarationSyntax).Cast<MethodDeclarationSyntax>().Where(x => x.Identifier.ToString() == targetMethodName).Where(x => x.ParameterList.Parameters.Count <= 1).ToList();
							if (targetMethods.Count == 0) {
								Debug.WriteLine($"Method {targetMethodName} not part of this class so using it plane needs to have pram");
								initializeMembersMethod.AppendLine($"\t{field.Declaration.Variables}.Changed += {targetMethodName};");
							}
							else if (targetMethods.Count == 1) {
								AddMethod(targetMethods.First());
							}
							else if (targetMethods.Count > 2) {
								MethodDeclarationSyntax bestOption = null;
								foreach (var item in targetMethods) {
									if (bestOption is null) {
										bestOption = item;
										continue;
									}
									if (item.ParameterList.Parameters.Count == 1 && bestOption.ParameterList.Parameters.Count == 0) {
										if (item.ParameterList.Parameters[0].Type.ToString() == "IChangeable") {
											bestOption = item;
											continue;
										}
									}
									if (bestOption.ParameterList.Parameters.Count == 1 && item.ParameterList.Parameters.Count == 0) {
										if (bestOption.ParameterList.Parameters[0].Type.ToString() == "IChangeable") {
											continue;
										}
										else {
											bestOption = item;
										}
									}
								}
								AddMethod(bestOption);
							}

						}
					}
					hasAnyLoadClasses = true;
				}
				if (!syncObject.BaseList.Types.Any(x => x.Type.ToString() == "SyncObject")) {
					if (!isWorldBuild) {
						initializeMembersMethod.AppendLine("\tbase.InitializeMembers(networkedObject, deserialize, netPointer);");
					}
				}
				initializeMembersMethod.AppendLine("}");
				var fileBuilder = new StringBuilder();
				fileBuilder.AppendLine($"using System;\nusing System.Linq;\nusing System.Net.WebSockets;\nusing System.Reflection;\nusing System.Threading;\nusing RhuEngine.Managers;\nusing RhuEngine.WorldObjects.ECS;\nusing RhuEngine.Linker;\nusing RNumerics;\nusing System.Threading.Tasks;\nusing System.Collections.Generic;\nusing RhuEngine.Datatypes;\nusing RhuEngine.Components;\nusing BepuPhysics;\nusing BepuUtilities.Memory;\nusing RhuEngine.Physics;\nusing System.Numerics;\nusing RhuEngine;\nusing RhuEngine.WorldObjects;\nusing OpusDotNet;");
				var classInerData = new StringBuilder();
				classInerData.AppendLine($"{syncObject.Modifiers} class {syncObject.Identifier}{syncObject.TypeParameterList} {{");
				if (!syncObject.Members.Where(x => x is MethodDeclarationSyntax).Cast<MethodDeclarationSyntax>().Any(x => x.Identifier.Text == "InitializeMembers" && x.TypeParameterList is null && x.ParameterList.Parameters.Count == 3)) {
					if (hasAnyLoadClasses) {
						classInerData.AppendLine($"\tpublic {syncObject.Identifier}() {{");
						classInerData.AppendLine(loadClasses.ToString());
						classInerData.AppendLine($"\t}}");
					}
					classInerData.AppendLine(initializeMembersMethod.ToString());
				}
				classInerData.AppendLine(addedInnerClassData.ToString());
				classInerData.AppendLine($"}}");
				fileBuilder.AppendLine($"{syncObject.PutDataInNested(classInerData.ToString())}");
				var formatedFileName = $"{nameSpace}.{name}{syncObject.TypeParameterList?.ToString().Replace("<", "[").Replace(">", "]")}";
				Debug.WriteLine("Loaded sync object " + formatedFileName);
				context.AddSource($"{formatedFileName}.cs", fileBuilder.ToString());
			}
		}
	}
}
