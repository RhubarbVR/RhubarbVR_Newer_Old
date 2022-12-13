using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using RhuEngine.Commads;
using System;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/Primitives" })]
	[GenericTypeConstraint(TypeConstGroups.Serializable)]
	public class NumberSyncObserver<T> : EditingField<Sync<T>> where T : struct
	{
		public readonly SyncRef<SpinBox> TargetSpinBox;
		public readonly Linker<double> Linker;
		protected override Task LoadEditor(UIBuilder2D ui) {
			var numberEditor = ui.PushElement<SpinBox>();
			Linker.Target = numberEditor.Value;
			TargetSpinBox.Target = numberEditor;
			switch (Type.GetTypeCode(typeof(T))) {
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
			numberEditor.ValueUpdated.Target = ValueUpdated;
			ui.Pop();
			return Task.CompletedTask;
		}

		protected bool ValueLoadedIn = false;


		[Exposed]
		public void ValueUpdated() {
			if (!ValueLoadedIn) {
				return;
			}
			try {
				if (typeof(T) == typeof(bool)) {
					TargetElement.SetValue(TargetSpinBox.Target.Value.Value == 1);
					LoadValueIn();
					return;
				}
				var data = Convert.ChangeType(TargetSpinBox.Target?.Value.Value, TargetElement.GetValueType());
				TargetElement.SetValue(data);
			}
			catch {

			}
			LoadValueIn();
		}

		protected override void LoadValueIn() {
			if (Linker.Linked) {
				if (TargetSpinBox.Target?.Value.IsLinkedTo ?? false) {
					try {
						if (TargetElement.GetValue() is bool boolValue) {
							TargetSpinBox.Target.Value.Value = boolValue ? 1 : 0;
							return;
						}
						var data = (double)Convert.ChangeType(TargetElement.Value, typeof(double));
						TargetSpinBox.Target.Value.Value = data;
						ValueLoadedIn = true;
					}
					catch { }
				}
			}
		}
	}
}