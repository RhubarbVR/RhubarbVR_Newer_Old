using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using CategoryAttribute = RhuEngine.WorldObjects.ECS.CategoryAttribute;
using System.Reflection.Emit;

namespace RhuEngine.Components
{
	public interface IPrimitiveEditorBuilder : IBasePrimitive
	{

	}

	[Category(new string[] { "Developer/Inspectors/Primitives" })]
	public sealed class PrimitiveEditorBuilder<T> : BasePrimitive<Sync<T>, T>, IPrimitiveEditorBuilder
	{
		public static Type GetPrimitiveBuildType(Type targetTest) {
			if (targetTest == typeof(string)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(bool)) {
				return typeof(CheckBoxEditor);
			}
			if (targetTest == typeof(sbyte)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(byte)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(short)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(ushort)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(int)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(uint)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(long)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(ulong)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(nint)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(nuint)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(float)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(double)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(decimal)) {
				return typeof(PrimitiveEditor);
			}
			if (targetTest == typeof(Type)) {
				return typeof(TypeEditor);
			}
			if (targetTest == typeof(Quaternionf)) {
				return typeof(QuaternionEditor);
			}
			return typeof(PrimitiveEditorBuilder<>).MakeGenericType(targetTest);
		}

		public override void ValueChange() {

		}

		protected override void BuildUI() {
			var targetType = GetPrimitiveBuildType(typeof(T));
			if (!targetType.IsAssignableTo(typeof(IPrimitiveEditorBuilder))) {
				var comp = Entity.GetFirstComponentOrAttach<IBasePrimitive>(targetType);
				comp.FieldEdit = FieldEdit;
				comp.TargetObjectWorld = TargetObjectWorld;
			}
			else {
				var MainBox = Entity.AttachComponent<BoxContainer>();
				MainBox.Vertical.Value = true;
				MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

				var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
				var amountOfBox = 0;
				BoxContainer fieldBox = null;
				var needNewBox = true;
				void AddFieldBox() {
					needNewBox = false;
					amountOfBox = 0;
					fieldBox = Entity.AddChild("FieldBox").AttachComponent<BoxContainer>();
					fieldBox.Vertical.Value = false;
					fieldBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

				}
				foreach (var item in fields) {
					var biuldType = GetPrimitiveBuildType(item.FieldType);
					if (item.FieldType.IsAssignableTo(typeof(IPrimitiveEditorBuilder))) {
						var newEdit = Entity.AddChild(item.Name).AttachComponent<IBasePrimitive>(biuldType);
						newEdit.FieldEdit = FieldEdit is null ? item.Name : FieldEdit + $".{item.Name}";
						newEdit.TargetObjectWorld = TargetObjectWorld;
						needNewBox = true;
					}
					else {
						if (needNewBox) {
							AddFieldBox();
						}
						if (amountOfBox >= 4) {
							AddFieldBox();
						}
						var newEdit = fieldBox.Entity.AddChild(item.Name).AttachComponent<IBasePrimitive>(biuldType);
						newEdit.FieldEdit = FieldEdit is null ? item.Name : FieldEdit + $".{item.Name}";
						newEdit.TargetObjectWorld = TargetObjectWorld;
						amountOfBox++;
					}
				}

			}
		}
	}
}