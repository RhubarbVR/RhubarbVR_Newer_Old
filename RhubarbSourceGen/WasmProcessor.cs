using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SourceGenerator;

namespace RhubarbSourceGen
{
	// Todo Remove After some thought 
	//public static class WasmProcessor
	//{
	//	public static string GetTypeData(TypeSyntax typeSyntax) {
	//		var tyypeSting = typeSyntax.ToString();
	//		if (tyypeSting == "void") {
	//			return "void";
	//		}
	//		else if (tyypeSting == "bool") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "char") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "byte") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "sbyte") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "short") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "ushort") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "int") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "uint") {
	//			return "int";
	//		}
	//		else if (tyypeSting == "long") {
	//			return "long";
	//		}
	//		else if (tyypeSting == "ulong") {
	//			return "long";
	//		}
	//		else if (tyypeSting == "float") {
	//			return "float";
	//		}
	//		else if (tyypeSting == "double") {
	//			return "double";
	//		}
	//		else {
	//			return "long";
	//		}
	//	}

	//	public static string ToSnakeCase(this string text) {
	//		if (text == null) {
	//			throw new ArgumentNullException(nameof(text));
	//		}
	//		if (text.Length < 2) {
	//			return text;
	//		}
	//		var sb = new StringBuilder();
	//		sb.Append(char.ToLowerInvariant(text[0]));
	//		for (var i = 1; i < text.Length; ++i) {
	//			var c = text[i];
	//			if (char.IsUpper(c)) {
	//				sb.Append('_');
	//				sb.Append(char.ToLowerInvariant(c));
	//			}
	//			else {
	//				sb.Append(c);
	//			}
	//		}
	//		return sb.ToString();
	//	}

	//	private static IEnumerable<(string name, string typeRapped)> BuildGenericBindings(GeneratorExecutionContext context, TypeParameterListSyntax typeParameterList1, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses1, TypeParameterListSyntax typeParameterList2, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses2) {
	//		var ranFirst = false;
	//		var ranSecond = false;
	//		foreach (var first in BuildGenericBindings(context, typeParameterList1, constraintClauses1)) {
	//			ranFirst = true;
	//			foreach (var second in BuildGenericBindings(context, typeParameterList2, constraintClauses2)) {
	//				ranSecond = true;
	//				yield return (first.name + second.name, first.typeRapped.Substring(0, first.typeRapped.Length - 1) + ", " + second.typeRapped.Remove(first.typeRapped.Length - 1));
	//			}
	//		}
	//		if (!ranFirst) {
	//			foreach (var second in BuildGenericBindings(context, typeParameterList2, constraintClauses2)) {
	//				yield return second;
	//			}
	//		}
	//		else if (!ranSecond) {
	//			foreach (var first in BuildGenericBindings(context, typeParameterList1, constraintClauses1)) {
	//				yield return first;
	//			}
	//		}
	//	}

	//	private static IEnumerable<string> GetAllSyncRefTypes(GeneratorExecutionContext context) {
	//		foreach (var item in context.GetAllClasses().Where(x => context.GetHasClassOrIterface(x, "IWorldObject"))) {
	//			if (item.TypeParameterList is null) {
	//				yield return "SyncRef<" + item.Identifier.Text + ">";
	//			}
	//			else if (item.Identifier.Text == "Sync") {
	//				foreach (var syncType in GetAllSyncValueTypes(context)) {
	//					yield return "SyncRef<" + syncType + ">";
	//				}
	//			}
	//		}
	//	}

	//	private static IEnumerable<string> GetAllSyncValueTypes(GeneratorExecutionContext context) {
	//		yield return "Sync<int>";
	//		yield return "Sync<uint>";
	//		yield return "Sync<bool>";
	//		yield return "Sync<char>";
	//		yield return "Sync<string>";
	//		yield return "Sync<float>";
	//		yield return "Sync<double>";
	//		yield return "Sync<string[]>";
	//		yield return "Sync<long>";
	//		yield return "Sync<ulong>";
	//		yield return "Sync<byte>";
	//		yield return "Sync<sbyte>";
	//		yield return "Sync<short>";
	//		yield return "Sync<ushort>";
	//		yield return "Sync<decimal>";
	//		yield return "Sync<byte[]>";
	//		yield return "Sync<Enum>";
	//		yield return "Sync<Type>";
	//		yield return "Sync<Uri>";
	//		yield return "Sync<NetPointer>";
	//		yield return "Sync<DateTime>";
	//		yield return "Sync<Playback>";
	//		yield return "Sync<float[]>";
	//		yield return "Sync<int[]>";
	//		yield return "Sync<Colorb>";
	//		yield return "Sync<Colorf>";
	//		yield return "Sync<ColorHSV>";
	//		yield return "Sync<AxisAlignedBox2d>";
	//		yield return "Sync<AxisAlignedBox2f>";
	//		yield return "Sync<AxisAlignedBox2i>";
	//		yield return "Sync<AxisAlignedBox3d>";
	//		yield return "Sync<AxisAlignedBox3f>";
	//		yield return "Sync<AxisAlignedBox3i>";
	//		yield return "Sync<Box2d>";
	//		yield return "Sync<Box2f>";
	//		yield return "Sync<Box3d>";
	//		yield return "Sync<Box3f>";
	//		yield return "Sync<Frame3f>";
	//		yield return "Sync<Index3i>";
	//		yield return "Sync<Index2i>";
	//		yield return "Sync<Index4i>";
	//		yield return "Sync<Interval1d>";
	//		yield return "Sync<Interval1i>";
	//		yield return "Sync<Line2d>";
	//		yield return "Sync<Line2f>";
	//		yield return "Sync<Line3d>";
	//		yield return "Sync<Line3f>";
	//		yield return "Sync<Matrix2d>";
	//		yield return "Sync<Matrix2f>";
	//		yield return "Sync<Matrix3d>";
	//		yield return "Sync<Matrix3f>";
	//		yield return "Sync<Plane3d>";
	//		yield return "Sync<Plane3f>";
	//		yield return "Sync<Quaterniond>";
	//		yield return "Sync<Quaternionf>";
	//		yield return "Sync<Ray3d>";
	//		yield return "Sync<Ray3f>";
	//		yield return "Sync<Segment3d>";
	//		yield return "Sync<Segment3f>";
	//		yield return "Sync<Triangle2d>";
	//		yield return "Sync<Triangle2f>";
	//		yield return "Sync<Triangle3d>";
	//		yield return "Sync<Triangle3f>";
	//		yield return "Sync<Vector2b>";
	//		yield return "Sync<Vector2d>";
	//		yield return "Sync<Vector2f>";
	//		yield return "Sync<Vector2i>";
	//		yield return "Sync<Vector2l>";
	//		yield return "Sync<Vector2u>";
	//		yield return "Sync<Vector3b>";
	//		yield return "Sync<Vector3d>";
	//		yield return "Sync<Vector3f>";
	//		yield return "Sync<Vector3i>";
	//		yield return "Sync<Vector3u>";
	//		yield return "Sync<Vector4b>";
	//		yield return "Sync<Vector4d>";
	//		yield return "Sync<Vector4f>";
	//		yield return "Sync<Vector4i>";
	//		yield return "Sync<Vector4u>";
	//		yield return "Sync<Vector3dTuple2>";
	//		yield return "Sync<Vector3dTuple3>";
	//		yield return "Sync<Vector3fTuple3>";
	//		yield return "Sync<Vector2dTuple2>";
	//		yield return "Sync<Vector2dTuple3>";
	//		yield return "Sync<Vector2dTuple4>";
	//		yield return "Sync<Circle3d>";
	//		yield return "Sync<Cylinder3d>";
	//		yield return "Sync<Matrix>";
	//	}

	//	private static IEnumerable<string> GetTypes(GeneratorExecutionContext context, TypeParameterConstraintClauseSyntax typeParameterConstraintClauseSyntax, int nesting = 0) {
	//		if(nesting >= 3) { 
	//		}
	//		if (typeParameterConstraintClauseSyntax is null) {
	//			yield return "int";
	//			yield return "uint";
	//			yield return "bool";
	//			yield return "char";
	//			yield return "string";
	//			yield return "float";
	//			yield return "double";
	//			yield return "string[]";
	//			yield return "long";
	//			yield return "ulong";
	//			yield return "byte";
	//			yield return "sbyte";
	//			yield return "short";
	//			yield return "ushort";
	//			yield return "decimal";
	//			yield return "byte[]";
	//			yield return "Enum";
	//			yield return "Type";
	//			yield return "Uri";
	//			yield return "NetPointer";
	//			yield return "DateTime";
	//			yield return "Playback";
	//			yield return "float[]";
	//			yield return "int[]";
	//			yield return "Colorb";
	//			yield return "Colorf";
	//			yield return "ColorHSV";
	//			yield return "AxisAlignedBox2d";
	//			yield return "AxisAlignedBox2f";
	//			yield return "AxisAlignedBox2i";
	//			yield return "AxisAlignedBox3d";
	//			yield return "AxisAlignedBox3f";
	//			yield return "AxisAlignedBox3i";
	//			yield return "Box2d";
	//			yield return "Box2f";
	//			yield return "Box3d";
	//			yield return "Box3f";
	//			yield return "Frame3f";
	//			yield return "Index3i";
	//			yield return "Index2i";
	//			yield return "Index4i";
	//			yield return "Interval1d";
	//			yield return "Interval1i";
	//			yield return "Line2d";
	//			yield return "Line2f";
	//			yield return "Line3d";
	//			yield return "Line3f";
	//			yield return "Matrix2d";
	//			yield return "Matrix2f";
	//			yield return "Matrix3d";
	//			yield return "Matrix3f";
	//			yield return "Plane3d";
	//			yield return "Plane3f";
	//			yield return "Quaterniond";
	//			yield return "Quaternionf";
	//			yield return "Ray3d";
	//			yield return "Ray3f";
	//			yield return "Segment3d";
	//			yield return "Segment3f";
	//			yield return "Triangle2d";
	//			yield return "Triangle2f";
	//			yield return "Triangle3d";
	//			yield return "Triangle3f";
	//			yield return "Vector2b";
	//			yield return "Vector2d";
	//			yield return "Vector2f";
	//			yield return "Vector2i";
	//			yield return "Vector2l";
	//			yield return "Vector2u";
	//			yield return "Vector3b";
	//			yield return "Vector3d";
	//			yield return "Vector3f";
	//			yield return "Vector3i";
	//			yield return "Vector3u";
	//			yield return "Vector4b";
	//			yield return "Vector4d";
	//			yield return "Vector4f";
	//			yield return "Vector4i";
	//			yield return "Vector4u";
	//			yield return "Vector3dTuple2";
	//			yield return "Vector3dTuple3";
	//			yield return "Vector3fTuple3";
	//			yield return "Vector2dTuple2";
	//			yield return "Vector2dTuple3";
	//			yield return "Vector2dTuple4";
	//			yield return "Circle3d";
	//			yield return "Cylinder3d";
	//			yield return "Matrix";
	//		}
	//		else {
	//			foreach (var outputType in context.GetAllTypes().Where(x => {
	//				if (x.Modifiers.ToString().Contains("static")) {
	//					return false;
	//				}
	//				foreach (var item in typeParameterConstraintClauseSyntax.Constraints) {
	//					var targetConst = item.ToString();
	//					if (targetConst.Contains("new()")) {
	//						if (x is not ClassDeclarationSyntax) {
	//							return false;
	//						}
	//						if (x is ClassDeclarationSyntax @class) {
	//							if (@class.Modifiers.ToString().Contains("abstract")) {
	//								return false;
	//							}
	//						}
	//					}
	//					else if (targetConst.Contains("class")) {
	//						if (x is not ClassDeclarationSyntax) {
	//							return false;
	//						}
	//					}
	//					else if (targetConst.Contains("struct")) {
	//						if (x is not StructDeclarationSyntax) {
	//							return false;
	//						}
	//					}
	//					else if (!context.GetHasClassOrIterface(x, targetConst)) {
	//						return false;
	//					}
	//				}
	//				return true;
	//			})) {
	//				if (outputType.TypeParameterList is null) {
	//					yield return outputType.GetNamespaceWithClass() + "." + outputType.Identifier.Text;
	//				}
	//				else {
	//					if (outputType.ConstraintClauses.Count == 0) {
	//						foreach (var item in GetTypes(context, null, nesting + 1)) {
	//							yield return outputType.GetNamespaceWithClass() + "." + outputType.Identifier.Text + "<" + item + ">";
	//						}
	//					}
	//					if (outputType.ConstraintClauses.Count == 1) {
	//						foreach (var item in GetTypes(context, outputType.ConstraintClauses[0], nesting + 1)) {
	//							yield return outputType.GetNamespaceWithClass() + "." + outputType.Identifier.Text + "<" + item + ">";
	//						}
	//					}

	//				}
	//			}
	//		}
	//	}

	//	private static IEnumerable<(string name, string typeRapped)> BuildGeneric(GeneratorExecutionContext context, (TypeParameterSyntax, TypeParameterConstraintClauseSyntax)[] types, string[] startingTypes) {
	//		if (types.Length == 0) {
	//			yield return (string.Join("_", startingTypes.Select(x => ReadCsharpType(x))), "<" + string.Join(",", startingTypes) + ">");
	//		}
	//		else {
	//			var current = types[0];
	//			var textTypes = types.Skip(1).ToArray();
	//			foreach (var item in GetTypes(context, current.Item2)) {
	//				var newStrings = startingTypes.Append(item).ToArray();
	//				foreach (var gen in BuildGeneric(context, textTypes, newStrings)) {
	//					yield return gen;
	//				}
	//			}
	//		}
	//	}

	//	private static IEnumerable<(string name, string typeRapped)> BuildGenericBindings(GeneratorExecutionContext context, TypeParameterListSyntax typeParameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses) {
	//		var listOfSimlerTypes = new List<(TypeParameterSyntax, TypeParameterConstraintClauseSyntax)>();
	//		if (typeParameterList is not null) {
	//			foreach (var typePram in typeParameterList.Parameters) {
	//				var clauses = constraintClauses.Where(x => x.Name.ToString() == typePram.Identifier.Text).FirstOrDefault();
	//				listOfSimlerTypes.Add((typePram, clauses));
	//			}
	//			foreach (var item in BuildGeneric(context, listOfSimlerTypes.ToArray(), Array.Empty<string>())) {
	//				yield return item;
	//			}
	//		}
	//	}

	//	public static string ReadCsharpType(string currentType) {
	//		return currentType.Replace("float", "f").Replace("double", "d").Replace("bool", "b")
	//				.Replace("string", "s").Replace("[]", "array").Replace("int", "i")
	//				.Replace("uint", "ui").Replace("long", "l").Replace("ulong", "ul")
	//				.Replace("Vector", "V").Replace("<", "").Replace(">", "").Replace(".", "_");
	//	}

	//	public static void Build(GeneratorExecutionContext context) {
	//		var mainFileSource = new StringBuilder();
	//		var addMethods = new StringBuilder();
	//		mainFileSource.AppendLine($"using System;\nusing System.Linq;\nusing System.Reflection;\nusing System.Threading;\nusing RNumerics;\nusing System.Threading.Tasks;\nusing System.Collections.Generic;\nusing System.Numerics;\nusing WebAssembly;\nusing WebAssembly.Instructions;\nusing WebAssembly.Runtime;\nusing RhuEngine.Datatypes;");
	//		var usingDelecars = new List<string>();
	//		if (RhubarbSourceGenerator.isRhuEngine) {
	//			mainFileSource.AppendLine($"using BepuPhysics.Collidables;");
	//		}
	//		foreach (var type in context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
	//			.Where(x => x is TypeDeclarationSyntax)
	//			.Cast<TypeDeclarationSyntax>()) {
	//			var userData = type.GetNamespace();
	//			if (string.IsNullOrEmpty(userData)) {
	//				continue;
	//			}
	//			if (usingDelecars.Contains(userData)) {
	//				continue;
	//			}
	//			usingDelecars.Add(userData);
	//			mainFileSource.AppendLine($"using {userData};");
	//		}
	//		mainFileSource.AppendLine($"public partial class Wasm{context.Compilation.AssemblyName.Replace("Debug", "")} {{");
	//		foreach (var type in context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
	//			.Where(x => x is TypeDeclarationSyntax)
	//			.Cast<TypeDeclarationSyntax>()) {
	//			var objectData = type.GetNamespaceWithClass() + type.Identifier.ToString();
	//			objectData = objectData.Replace(".", "");
	//			var isGeneric = type.TypeParameterList is not null;
	//			var genericBaseGenericAddOnString = "";
	//			var genericBaseGenericAddOnStringTypeRequirements = "";
	//			var genericBaseGenericAddOnStringRaped = "";
	//			if (isGeneric) {
	//				var isStart = false;
	//				foreach (var pram in type.TypeParameterList.Parameters) {
	//					if (isStart) {
	//						genericBaseGenericAddOnString += ", ";
	//					}
	//					genericBaseGenericAddOnString += $"C{pram.Identifier}";
	//					var constrant = type.ConstraintClauses.Where(x => x.Name.ToString() == pram.Identifier.Text).FirstOrDefault();
	//					if (constrant is not null) {
	//						genericBaseGenericAddOnStringTypeRequirements += $" where C{pram.Identifier}: {constrant.Constraints}";
	//					}
	//					isStart = true;
	//				}
	//				genericBaseGenericAddOnStringRaped = $"_GENERIC{type.TypeParameterList.Parameters.Count}<" + genericBaseGenericAddOnString + ">";
	//			}
	//			foreach (var item in type.Members) {
	//				if (!item.HasAtrubute("ExposedAttribute")) {
	//					if (item is FieldDeclarationSyntax fielde) {
	//						if (fielde.Modifiers.ToString().Contains("readonly")) {
	//							if (!context.GetHasClassOrIterface(context.GetType(fielde.Declaration.Type.ToString()), "SyncObject")) {
	//								continue;
	//							}
	//						}
	//						else {
	//							continue;
	//						}
	//					}
	//					else {
	//						continue;
	//					}
	//				}
	//				if (item.HasAtrubute("UnExsposedAttribute")) {
	//					continue;
	//				}
	//				if (item is MethodDeclarationSyntax method) {
	//					var genMethodName = ToSnakeCase(objectData + method.Identifier.Text);
	//					var isVoid = method.ReturnType.ToString() == "void";
	//					var isGenericMethod = method.TypeParameterList is not null;
	//					var isOveralGeneric = isGeneric || isGenericMethod;
	//					var pramsData = "";
	//					var genericData = genericBaseGenericAddOnString;
	//					var genericEndData = genericBaseGenericAddOnStringTypeRequirements;
	//					if (isGenericMethod) {
	//						foreach (var pram in method.TypeParameterList.Parameters) {
	//							if (!string.IsNullOrEmpty(genericData)) {
	//								genericData += $", ";
	//							}
	//							genericData += $"M{pram.Identifier}";
	//							var constrant = type.ConstraintClauses.Where(x => x.Name.ToString() == pram.Identifier.Text).FirstOrDefault();
	//							if (constrant is not null) {
	//								genericEndData += $" where M{pram.Identifier}: {constrant.Constraints}";
	//							}
	//						}
	//					}
	//					var genericDataRapped = "";
	//					if (!string.IsNullOrEmpty(genericData)) {
	//						genericDataRapped = $"_GENERIC{(type.TypeParameterList?.Parameters.Count ?? 0) + (method.TypeParameterList?.Parameters.Count ?? 0)}<" + genericData + ">";
	//					}
	//					foreach (var pram in method.ParameterList.Parameters) {
	//						if (pramsData != "") {
	//							if (!char.IsUpper(pram.Type.ToString().First())) {
	//								pramsData += "_";
	//							}
	//						}
	//						pramsData += ReadCsharpType(pram.Type.ToString());
	//					}
	//					pramsData = ToSnakeCase(pramsData);
	//					if (string.IsNullOrEmpty(genericData)) {
	//						addMethods.AppendLine($"\t\timports.Add(nameof({genMethodName}__{pramsData}), new FunctionImport({genMethodName}__{pramsData}));");
	//					}
	//					else {
	//						foreach (var genTypeOne in BuildGenericBindings(context, method.TypeParameterList, method.ConstraintClauses, type.TypeParameterList, type.ConstraintClauses)) {
	//							addMethods.AppendLine($"\t\timports.Add(nameof({genMethodName}__{pramsData}_GENERIC{(type.TypeParameterList?.Parameters.Count ?? 0) + (method.TypeParameterList?.Parameters.Count ?? 0)}) + \"{genTypeOne.name}\", new FunctionImport({genMethodName}__{pramsData}_GENERIC{(type.TypeParameterList?.Parameters.Count ?? 0) + (method.TypeParameterList?.Parameters.Count ?? 0)}{genTypeOne.typeRapped}));");
	//						}
	//					}
	//					mainFileSource.Append($"\tpublic {GetTypeData(method.ReturnType)} {genMethodName}__{pramsData}{genericDataRapped}(");
	//					var starting = false;
	//					if (!method.Modifiers.ToString().Contains("static")) {
	//						mainFileSource.Append("long target_this");
	//						starting = true;
	//					}
	//					foreach (var pram in method.ParameterList.Parameters) {
	//						if (starting) {
	//							mainFileSource.Append(", ");
	//						}
	//						mainFileSource.Append($"{GetTypeData(pram.Type)} {pram.Identifier.Text}");
	//						starting = true;
	//					}
	//					mainFileSource.AppendLine($") {genericEndData} {{");
	//					if (!isVoid) {
	//						mainFileSource.AppendLine($"\t\treturn default;");
	//					}
	//					mainFileSource.AppendLine($"\t}}");
	//				}
	//				if (item is PropertyDeclarationSyntax property) {
	//					var genGetName = ToSnakeCase(objectData + "_" + "get" + property.Identifier.Text);
	//					var genSetName = ToSnakeCase(objectData + "_" + "set" + property.Identifier.Text);
	//					var hasGetter = false;
	//					var hasSetter = false;
	//					if (property.AccessorList is not null) {
	//						foreach (var access in property.AccessorList.Accessors) {
	//							if (access.Modifiers.ToString().Contains("private")) {
	//								continue;
	//							}
	//							if (access.Modifiers.ToString().Contains("protected")) {
	//								continue;
	//							}
	//							if (access.Keyword.ToString() == "get") {
	//								hasGetter = true;
	//							}
	//							else {
	//								hasSetter = true;
	//							}
	//						}
	//					}
	//					else {
	//						if (property.ToString().Contains("=>")) {
	//							hasGetter = true;
	//						}
	//					}
	//					if (item.HasAtrubute("NoWriteExsposedAttribute")) {
	//						hasSetter = false;
	//					}
	//					if (hasSetter) {
	//						if (string.IsNullOrEmpty(genericBaseGenericAddOnStringRaped)) {
	//							addMethods.AppendLine($"\t\timports.Add(nameof({genSetName}), new FunctionImport({genSetName}));");
	//						}
	//						else {
	//							foreach (var genType in BuildGenericBindings(context, type.TypeParameterList, type.ConstraintClauses)) {
	//								addMethods.AppendLine($"\t\timports.Add(nameof({genSetName}_GENERIC{type.TypeParameterList.Parameters.Count}) + \"{genType.name}\", new FunctionImport({genSetName}_GENERIC{type.TypeParameterList.Parameters.Count}{genType.typeRapped}));");
	//							}
	//						}
	//						mainFileSource.AppendLine($"\tpublic void {genSetName}{genericBaseGenericAddOnStringRaped}(long target_this, {GetTypeData(property.Type)} setValue){genericBaseGenericAddOnStringTypeRequirements} {{");
	//						mainFileSource.AppendLine($"\t}}");
	//					}
	//					if (hasGetter) {
	//						if (string.IsNullOrEmpty(genericBaseGenericAddOnStringRaped)) {
	//							addMethods.AppendLine($"\t\timports.Add(nameof({genGetName}), new FunctionImport({genGetName}));");
	//						}
	//						else {
	//							foreach (var genType in BuildGenericBindings(context, type.TypeParameterList, type.ConstraintClauses)) {
	//								addMethods.AppendLine($"\t\timports.Add(nameof({genGetName}_GENERIC{type.TypeParameterList.Parameters.Count}) + \"{genType.name}\", new FunctionImport({genGetName}_GENERIC{type.TypeParameterList.Parameters.Count}{genType.typeRapped}));");
	//							}
	//						}
	//						mainFileSource.AppendLine($"\tpublic {GetTypeData(property.Type)} {genGetName}{genericBaseGenericAddOnStringRaped}(long target_this){genericBaseGenericAddOnStringTypeRequirements} {{");
	//						mainFileSource.AppendLine($"\t\treturn default;");
	//						mainFileSource.AppendLine($"\t}}");
	//					}
	//				}
	//				if (item is FieldDeclarationSyntax field) {
	//					var fieldName = field.Declaration.Variables.First().Identifier.ToString();
	//					if (char.IsLower(fieldName.First())) {
	//						fieldName = "_" + fieldName;
	//					}
	//					var genGetName = ToSnakeCase(objectData + "_" + "get" + fieldName);
	//					var isStatic = field.Modifiers.ToString().Contains("static");
	//					if (string.IsNullOrEmpty(genericBaseGenericAddOnStringRaped)) {
	//						addMethods.AppendLine($"\t\timports.Add(nameof({genGetName}), new FunctionImport({genGetName}));");
	//					}
	//					else {
	//						foreach (var genType in BuildGenericBindings(context, type.TypeParameterList, type.ConstraintClauses)) {
	//							addMethods.AppendLine($"\t\timports.Add(nameof({genGetName}_GENERIC{type.TypeParameterList.Parameters.Count}) + \"{genType.name}\", new FunctionImport({genGetName}_GENERIC{type.TypeParameterList.Parameters.Count}{genType.typeRapped}));");
	//						}
	//					}
	//					if (isStatic) {
	//						mainFileSource.AppendLine($"\tpublic {GetTypeData(field.Declaration.Type)} {genGetName}{genericBaseGenericAddOnStringRaped}(){genericBaseGenericAddOnStringTypeRequirements} {{");
	//					}
	//					else {
	//						mainFileSource.AppendLine($"\tpublic {GetTypeData(field.Declaration.Type)} {genGetName}{genericBaseGenericAddOnStringRaped}(long target_this){genericBaseGenericAddOnStringTypeRequirements} {{");
	//					}
	//					mainFileSource.AppendLine($"\t\treturn default;");
	//					mainFileSource.AppendLine($"\t}}");
	//				}
	//			}
	//		}
	//		mainFileSource.AppendLine("\tpublic override void BuildWasmImports(IDictionary<string, RuntimeImport> imports) {");
	//		mainFileSource.AppendLine(addMethods.ToString());
	//		mainFileSource.AppendLine("\t\tbase.BuildWasmImports(imports);");
	//		mainFileSource.AppendLine("\t}");
	//		mainFileSource.AppendLine("}");
	//		context.AddSource($"Wasm{context.Compilation.AssemblyName}.cs", mainFileSource.ToString());
	//	}
	//}
}
