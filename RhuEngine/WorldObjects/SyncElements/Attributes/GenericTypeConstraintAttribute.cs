using System;

namespace RhuEngine.WorldObjects
{
	public enum TypeConstGroups {
		None,
		Serializable,
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class GenericTypeConstraintAttribute : Attribute
	{
		public Type[] Data { get; private set; }

		public TypeConstGroups Groups { get; private set; }

		public GenericTypeConstraintAttribute(Type[] value, TypeConstGroups typeConstGroups = TypeConstGroups.None) {
			Data = value;
			Groups = typeConstGroups;
		}
		public GenericTypeConstraintAttribute(TypeConstGroups typeConstGroups = TypeConstGroups.Serializable) {
			Data = Array.Empty<Type>();
			Groups = typeConstGroups;
		}
	}
}
