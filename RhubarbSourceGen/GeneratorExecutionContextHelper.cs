using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static System.Net.Mime.MediaTypeNames;

namespace RhubarbSourceGen
{
	public static class GeneratorExecutionContextHelper
	{
		public static AttributeSyntax GetAtrubute(this MemberDeclarationSyntax classDeclarationSyntax, string attributeName) {
			var hasAtrubute = classDeclarationSyntax.AllAtrubutes().Where(x => x.Name.ToString() == attributeName).FirstOrDefault();
			if (hasAtrubute is not null) {
				return hasAtrubute;
			}
			if (attributeName.EndsWith("Attribute")) {
				attributeName = attributeName.Replace("Attribute", "");
				return classDeclarationSyntax.AllAtrubutes().Where(x => x.Name.ToString() == attributeName).FirstOrDefault();
			}
			return null;
		}

		public static bool HasAtrubute(this MemberDeclarationSyntax classDeclarationSyntax, string attributeName) {
			var hasAtrubute = classDeclarationSyntax.AllAtrubutes().Any(x => x.Name.ToString() == attributeName);
			if (hasAtrubute) {
				return true;
			}
			if (attributeName.EndsWith("Attribute")) {
				attributeName = attributeName.Replace("Attribute", "");
				return classDeclarationSyntax.AllAtrubutes().Any(x => x.Name.ToString() == attributeName);
			}
			return false;
		}

		public static IEnumerable<AttributeSyntax> AllAtrubutes(this MemberDeclarationSyntax classDeclarationSyntax) {
			return classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes);
		}

		public static string PutDataInNested(this BaseTypeDeclarationSyntax syntax, string injectedString) {
			var addToEnd = new List<string>();
			var potentialNamespaceParent = syntax.Parent;
			while (potentialNamespaceParent is not null and
					not NamespaceDeclarationSyntax
					and not FileScopedNamespaceDeclarationSyntax) {
				if (potentialNamespaceParent is ClassDeclarationSyntax @base) {
					addToEnd.Add(@base.Modifiers.ToString() + " class " + @base.Identifier.Text + @base.TypeParameterList?.ToString());
				}
				potentialNamespaceParent = potentialNamespaceParent.Parent;
			}
			addToEnd.Reverse();
			var fullstring = "namespace " + syntax.GetNamespace() + ";";
			foreach (var item in addToEnd) {
				fullstring += "\n" + item + "{";
			}
			fullstring += "\n" + injectedString;
			foreach (var item in addToEnd) {
				fullstring += "\n}";
			}

			return fullstring;
		}

		public static string GetNamespace(this BaseTypeDeclarationSyntax syntax) {
			var nameSpace = string.Empty;
			var potentialNamespaceParent = syntax.Parent;
			while (potentialNamespaceParent is not null and
					not NamespaceDeclarationSyntax
					and not FileScopedNamespaceDeclarationSyntax) {
				potentialNamespaceParent = potentialNamespaceParent.Parent;
			}
			if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent) {
				nameSpace = namespaceParent.Name.ToString();
				while (true) {
					if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent) {
						break;
					}
					nameSpace = $"{namespaceParent.Name}.{nameSpace}";
					namespaceParent = parent;
				}
			}
			return nameSpace;
		}

		public static string GetNamespaceWithClass(this BaseTypeDeclarationSyntax syntax) {
			var nameSpace = string.Empty;
			var addToEnd = new List<string>();
			var potentialNamespaceParent = syntax.Parent;
			while (potentialNamespaceParent is not null and
					not NamespaceDeclarationSyntax
					and not FileScopedNamespaceDeclarationSyntax) {
				if (potentialNamespaceParent is ClassDeclarationSyntax @base) {
					addToEnd.Add(@base.Identifier.Text + @base.TypeParameterList?.ToString());
				}
				potentialNamespaceParent = potentialNamespaceParent.Parent;
			}
			if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent) {
				nameSpace = namespaceParent.Name.ToString();
				while (true) {
					if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent) {
						break;
					}
					nameSpace = $"{namespaceParent.Name}.{nameSpace}";
					namespaceParent = parent;
				}
			}
			addToEnd.Reverse();
			foreach (var item in addToEnd) {
				nameSpace = $"{nameSpace}.{item}";
			}
			return nameSpace;
		}


		public static bool GetHasClassOrIterface(this GeneratorExecutionContext context, TypeDeclarationSyntax typeDeclaration, string classOrInterfaceName) {
			var stacks = new Stack<TypeDeclarationSyntax>();
			stacks.Push(typeDeclaration);
			while (stacks.Count > 0) {
				var currentType = stacks.Pop();
				if (currentType == null) {
					continue;
				}
				if (currentType.Identifier.Text == classOrInterfaceName) {
					return true;
				}
				if (currentType.BaseList is not null) {
					foreach (var item in currentType.BaseList.Types) {
						var targetTypeString = item.Type.ToString();
						stacks.Push(context.GetType(targetTypeString));
					}
				}
			}
			return false;
		}

		private static readonly Dictionary<string, TypeDeclarationSyntax> _cache = new();

		public static TypeDeclarationSyntax GetType(this GeneratorExecutionContext context, string name) {
			if (name.Contains("<") && name.Contains(">")) {
				var startPoint = name.IndexOf('<');
				name = name.Remove(startPoint, 1).Insert(startPoint, "[");
				var endPoint = name.LastIndexOf('>');
				name = name.Remove(endPoint, 1).Insert(endPoint, "]");
				name = Regex.Replace(name, @"\<.*?\>", "");
				var amountOfTypes = name.Where(x => x == ',').Count();
				var searchName = name.Substring(0, startPoint);
				name = name.Substring(0, startPoint) + "[" + new string(',', amountOfTypes) + "]";
				if (_cache.TryGetValue(name, out var type)) {
					return type;
				}
				var returnData = context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
					.Where(x => x is TypeDeclarationSyntax)
					.Cast<TypeDeclarationSyntax>()
					.Where(x => x.Identifier.Text == searchName)
					.Where(x => x.TypeParameterList is not null)
					.Where(x => x.TypeParameterList.Parameters.Count == (amountOfTypes + 1))
					.FirstOrDefault();
				_cache.Add(name, returnData);
				return returnData;
			}
			else {
				if (_cache.TryGetValue(name, out var type)) {
					return type;
				}
				var returnData = context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
					.Where(x => x is TypeDeclarationSyntax)
					.Cast<TypeDeclarationSyntax>()
					.Where(x => x.TypeParameterList is null)
					.Where(x => x.Identifier.Text == name).FirstOrDefault();
				_cache.Add(name, returnData);
				return returnData;
			}
		}


		public static IEnumerable<ClassDeclarationSyntax> GetAllSyncObjects(this GeneratorExecutionContext context) {
			return context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
				.Where(x => x is ClassDeclarationSyntax)
				.Cast<ClassDeclarationSyntax>()
				.Where(x => context.GetHasClassOrIterface(x, "SyncObject"));
		}

		public static IEnumerable<ClassDeclarationSyntax> GetAllClasses(this GeneratorExecutionContext context) {
			return context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
				.Where(x => x is ClassDeclarationSyntax)
				.Cast<ClassDeclarationSyntax>();
		}

		public static IEnumerable<StructDeclarationSyntax> GetAllStructs(this GeneratorExecutionContext context) {
			return context.Compilation.SyntaxTrees.SelectMany((x) => x.GetRoot().DescendantNodes())
				.Where(x => x is StructDeclarationSyntax)
				.Cast<StructDeclarationSyntax>();
		}
	}
}
