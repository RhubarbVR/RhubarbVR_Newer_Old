using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RNumerics
{
	public static class FastReflection
	{
		private readonly static Dictionary<Type, MemberInfo[]> _members = new();

		public static MemberInfo[] FastGetMembers(this Type type) {
			lock (_members) {
				if (_members.TryGetValue(type, out var data)) {
					return data;
				}
				else {
					data = type.GetMembers();
					_members.Add(type, data);
					return data;
				}
			}
		}
		public static IEnumerable<FieldInfo> FastGetFields(this Type type, BindingFlags bindingFlags) {
			var data = type.GetMembers();
			foreach (var member in data) {
				if (member is FieldInfo field) {
					if (field.IsPublic && !bindingFlags.HasFlag(BindingFlags.Public)) {
						continue;
					}
					if (field.IsStatic && !bindingFlags.HasFlag(BindingFlags.Static)) {
						continue;
					}
					if (!field.IsStatic && !bindingFlags.HasFlag(BindingFlags.Instance)) {
						continue;
					}
					if (field.IsInitOnly && !bindingFlags.HasFlag(BindingFlags.DeclaredOnly)) {
						continue;
					}
					if (!field.IsPublic && !bindingFlags.HasFlag(BindingFlags.NonPublic)) {
						continue;
					}
					yield return field;
				}
			}
		}

		public static IEnumerable<MethodInfo> FastGetMethods(this Type type, BindingFlags bindingFlags) {
			var data = type.GetMembers();
			foreach (var member in data) {
				if (member is MethodInfo field) {
					if (field.IsPublic && !bindingFlags.HasFlag(BindingFlags.Public)) {
						continue;
					}
					if (field.IsStatic && !bindingFlags.HasFlag(BindingFlags.Static)) {
						continue;
					}
					if (!field.IsStatic && !bindingFlags.HasFlag(BindingFlags.Instance)) {
						continue;
					}
					if (!field.IsPublic && !bindingFlags.HasFlag(BindingFlags.NonPublic)) {
						continue;
					}
					yield return field;
				}
			}
		}

		public static IEnumerable<MethodInfo> FastGetMethods(this Type type, string name, BindingFlags bindingFlags) {
			foreach (var member in type.GetMethods(bindingFlags)) {
				if(member.Name == name) {
					yield return member;
				}
			}
		}

		public static IEnumerable<MethodInfo> FastGetMethods(this Type type) {
			var data = type.GetMembers();
			foreach (var member in data) {
				if (member is MethodInfo field) {
					yield return field;
				}
			}
		}

		public static IEnumerable<FieldInfo> FastGetFields(this Type type) {
			var data = type.GetMembers();
			foreach (var member in data) {
				if (member is FieldInfo field) {
					yield return field;
				}
			}
		}

	}

}
