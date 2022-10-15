using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;
using RhuEngine.Commads;
using System;
using System.Linq;
using MessagePack;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/Primitives" })]
	public class MultiNumberSyncObserver<T> : EditingField<Sync<T>> where T : struct
	{
		public readonly SyncObjList<SyncRef<SpinBox>> SpinBoxes;
		public readonly SyncObjList<Linker<double>> SpinBoxesLinkers;
		public int AmountOfFileds { get; private set; }
		public Type FieldsType { get; private set; }
		public FieldInfo[] Fileds { get; private set; }

		private void LoadField(UIBuilder2D ui, int index, FieldInfo fieldInfo) {
			var numberEditor = ui.PushElement<SpinBox>();
			SpinBoxes.Add().Target = numberEditor;
			SpinBoxesLinkers.Add().Target = numberEditor.Value;
			numberEditor.Prefix.Value = fieldInfo.Name?.ToUpper();
			switch (Type.GetTypeCode(FieldsType)) {
				case TypeCode.Boolean:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.MaxValue.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MinValue.Value = 0;
					break;
				case TypeCode.Byte:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = byte.MaxValue;
					numberEditor.MinValue.Value = byte.MinValue;
					break;
				case TypeCode.Char:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = int.MaxValue;
					numberEditor.MinValue.Value = int.MinValue;
					break;
				case TypeCode.Decimal:
					//To big for this
					numberEditor.ArrowStep.Value = 0.001;
					numberEditor.MaxValue.Value = (double)decimal.MaxValue;
					numberEditor.MinValue.Value = (double)decimal.MinValue;
					break;
				case TypeCode.Double:
					numberEditor.ArrowStep.Value = 0.001;
					numberEditor.MaxValue.Value = double.MaxValue;
					numberEditor.MinValue.Value = double.MinValue;
					break;
				case TypeCode.Int16:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = short.MaxValue;
					numberEditor.MinValue.Value = short.MinValue;
					break;
				case TypeCode.Int32:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = int.MaxValue;
					numberEditor.MinValue.Value = int.MinValue;
					break;
				case TypeCode.Int64:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = long.MaxValue;
					numberEditor.MinValue.Value = long.MinValue;
					break;
				case TypeCode.SByte:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = sbyte.MaxValue;
					numberEditor.MinValue.Value = sbyte.MinValue;
					break;
				case TypeCode.Single:
					numberEditor.ArrowStep.Value = 0.5;
					numberEditor.MaxValue.Value = float.MaxValue;
					numberEditor.MinValue.Value = float.MinValue;
					break;
				case TypeCode.UInt16:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = ushort.MaxValue;
					numberEditor.MinValue.Value = ushort.MinValue;
					break;
				case TypeCode.UInt32:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = uint.MaxValue;
					numberEditor.MinValue.Value = uint.MinValue;
					break;
				case TypeCode.UInt64:
					numberEditor.ArrowStep.Value = 1;
					numberEditor.StepValue.Value = 1;
					numberEditor.MaxValue.Value = ulong.MaxValue;
					numberEditor.MinValue.Value = ulong.MinValue;
					break;
				default:
					numberEditor.ArrowStep.Value = 1;
					break;
			}
			var added = ui.Entity.AttachComponent<AddSingleValuePram<int>>();
			added.Value.Value = index;
			numberEditor.ValueUpdated.Target = added.Call;
			added.Target.Target = ValueUpdated;
			ui.Pop();
		}

		protected override Task LoadEditor(UIBuilder2D ui) {
			Fileds = typeof(T).GetFields().Where(x => x.GetCustomAttribute<KeyAttribute>() is not null).ToArray();
			AmountOfFileds = Fileds.Length;
			if (AmountOfFileds <= 1) {
				throw new NotSupportedException();
			}
			FieldsType = Fileds.First().FieldType;
			for (var i = 0; i < Fileds.Length; i++) {
				if (Fileds[i].FieldType != FieldsType) {
					throw new NotSupportedException();
				}
			}
			var sizeIn = 1f / AmountOfFileds;
			ui.PushElement<UIElement>();
			for (var i = 0; i < Fileds.Length; i++) {
				var element = ui.PushElement<UIElement>();
				element.Min.Value = new Vector2f((sizeIn * i) + 0.01f, 0);
				element.Max.Value = new Vector2f((sizeIn * (i + 1)) - 0.01f, 1);
				LoadField(ui, i, Fileds[i]);
				ui.Pop();
			}
			ui.Pop();
			return Task.CompletedTask;
		}

		protected bool ValueLoadedIn = false;


		[Exposed]
		public void ValueUpdated(int index) {
			if (!ValueLoadedIn) {
				return;
			}
			if (SpinBoxes.Count != AmountOfFileds) {
				return;
			}
			try {
				var value = (object)TargetElement.Value;
				for (var i = 0; i < SpinBoxes.Count; i++) {

					try {
						if (FieldsType == typeof(bool)) {
							Fileds[i].SetValue(value, SpinBoxes[i].Target.Value.Value == 1);
							LoadValueIn();
							return;
						}
						Fileds[i].SetValue(value, Convert.ChangeType(SpinBoxes[i].Target.Value.Value, FieldsType));
					}
					catch { }
				}
				TargetElement.SetValue(value);
			}
			catch { }
			LoadValueIn();
		}

		protected override void LoadValueIn() {
			if (SpinBoxes.Count != AmountOfFileds) {
				return;
			}
			for (var i = 0; i < SpinBoxes.Count; i++) {
				if (!(SpinBoxes[i].Target?.Value.IsLinkedTo ?? false)) {
					return;
				}
			}
			try {
				var value = TargetElement.Value;
				for (var i = 0; i < SpinBoxes.Count; i++) {
					var eachValue = Fileds[i].GetValue(value);
					try {
						if (eachValue is bool boolValue) {
							SpinBoxes[i].Target.Value.Value = boolValue ? 1 : 0;
							return;
						}
						var data = (double)Convert.ChangeType(eachValue, typeof(double));
						SpinBoxes[i].Target.Value.Value = data;
					}
					catch { }
				}
				ValueLoadedIn = true;
			}
			catch { }
		}
	}
}