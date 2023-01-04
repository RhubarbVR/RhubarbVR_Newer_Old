using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RhuEngine.Components
{
	public interface IBasePrimitive : IInspector
	{
		public string FieldEdit { get; set; }
	}

	[Category(new string[] { "Developer/Inspectors/Primitives" })]
	public abstract class BasePrimitive<T, TValue> : BaseInspector<ISync>, IBasePrimitive where T : class, ISync
	{
		[OnChanged(nameof(ValueChange))]
		public readonly Sync<string> TargetField;
		public string FieldEdit { get => TargetField.Value; set => TargetField.Value = value; }

		[Exposed]
		public void SetNull() {
			SetValue(default);
		}

		public TValue GetCastedValue() {
			try {
				return (TValue)GetValue();
			}
			catch {
				return default;
			}
		}

		public Type GetFieldType() {
			if (TargetField.Value is null) {
				return Target.GetValueType();
			}
			return StructEditor.GetFielType(Target.GetValueType(),TargetField.Value);
		}

		public object GetValue() {
			if (TargetField.Value is null) {
				return Target.GetValue();
			}
			return StructEditor.GetFieldValue(Target.GetValue(), TargetField.Value);
		}

		public void SetCastedValue(TValue value) {
			try {
				SetValue(value);
			}
			catch { }
		}

		public void SetValue(object data) {
			if (TargetField.Value is null) {
				Target.SetValue(data);
			}
			else {
				var editedData = Target.GetValue();
				StructEditor.SetFieldValueObject(editedData, TargetField.Value, data);
				Target.SetValue(editedData);
			}
		}


		public abstract void ValueChange();

		private IChangeable _lastSync;

		public override void LocalBind() {
			base.LocalBind();
			if (_lastSync is not null) {
				_lastSync.Changed -= Sync_Changed;
			}
			_lastSync = null;
			if (TargetObject.Target is IChangeable changeable) {
				changeable.Changed += Sync_Changed;
				_lastSync = changeable;
				Sync_Changed(null);
			}
		}

		private void Sync_Changed(IChangeable obj) {
			ValueChange();
		}

		public T Target => TargetObject.Target as T;

	}
}