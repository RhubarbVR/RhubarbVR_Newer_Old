using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RNumerics
{
	public static class FamcyTypeParser
	{
		public static Type PraseType(in string type, Assembly[] asm = null) {
			asm ??= AppDomain.CurrentDomain.GetAssemblies();
			return type.Contains("<") && type.Contains(">") ? PraseGeneric(type,asm) : SingleTypeParse(type,asm);
		}
		public static string[] ExtraNameSpaces = new string[] {
			"System.",
			"RNumerics.",
			"RhuEngine.",
			"RhuEngine.Components.",
			"RhuEngine.WorldObjects.",
			"RhuEngine.WorldObjects.ECS.",
		};

		public static Type SingleTypeParse(string type, in Assembly[] asm) {
			if (type == "int") {
				type = nameof(Int32);
			}
			if (type == "uint") {
				type = nameof(UInt32);
			}
			if (type == "bool") {
				type = nameof(Boolean);
			}
			if (type == "char") {
				type = nameof(Char);
			}
			if (type == "string") {
				type = nameof(String);
			}
			if (type == "float") {
				type = nameof(Single);
			}
			if (type == "double") {
				type = nameof(Double);
			}
			if (type == "long") {
				type = nameof(Int64);
			}
			if (type == "ulong") {
				type = nameof(UInt64);
			}
			if (type == "byte") {
				type = nameof(Byte);
			}
			if (type == "sbyte") {
				type = nameof(SByte);
			}
			if (type == "short") {
				type = nameof(Int16);
			}
			if (type == "ushort") {
				type = nameof(UInt16);
			}
			var returnType = Type.GetType(type, false, true);
			if (returnType == null) {
				foreach (var item in asm) {
					if (returnType == null) {
						returnType = item.GetType(type, false, true);
						if (returnType != null) {
							return returnType;
						}
					}
				}
			}
			if (returnType == null) {
				foreach (var item in ExtraNameSpaces) {
					returnType = Type.GetType(item + type, false, true);
					if (returnType == null) {
						foreach (var itema in asm) {
							if (returnType == null) {
								returnType = itema.GetType(item + type, false, true);
								if (returnType != null) {
									return returnType;
								}
							}
						}
					}
					if (returnType != null) {
						return returnType;
					}
				}
			}
			return returnType;
		}
		public static Type PraseGeneric(in string type, in Assembly[] asm) {
			var firstGroup = type.IndexOf('<');
			var depth = 0;
			var lastIndex = firstGroup + 1;
			var types = new List<Type>();
			for (var i = lastIndex; i < type.Length; i++) {
				var c = type[i];
				if ((c == ','|| c == '>') && depth == 0) {
					var ennerdata = type.Substring(lastIndex, i - lastIndex);
					types.Add(PraseType(ennerdata, asm));
					lastIndex = i + 1;
				}
				if (c == '<') {
					depth++;
				}
				if (c == '>') {
					depth--;
				}
			}
			var FirstPartOfType = type.Substring(0, firstGroup);
			var starttype = SingleTypeParse(FirstPartOfType + $"`{types.Count}",asm);
			
			return starttype.MakeGenericType(types.ToArray());
		}
	}

}
