using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	public enum MultiOperators
	{
		Addition,
		Subtraction,
		Multiplication,
		Division,
		Modulus,
		ConditionallogicalAND,
		ConditionallogicalOR,
		LeftShift,
		RightShift,
		LogicalOR,
		LogicalAND,
		LogicalExclusiveOR,
	}
	public enum DualOperators
	{
		Equal,
		NotEqual,
		LessThan,
		GreaterThan,
		LessThanOrEqual,
		GreaterThanOrEqual,
	}

	public enum SingleOperators
	{
		Cast,
		LogicalNegation,
		BitwiseComplement,
	}


	public abstract class AbMultiOperation<Out,In> : Component where Out : struct where In : struct
	{
		public readonly Linker<Out> Output;

		[OnChanged(nameof(Inputs_ChildChanged))]
		public readonly SyncObjList<SyncRef<IValueSource<In>>> Inputs;

		public override void OnLoaded() {
			base.OnLoaded();
			Inputs.ChildChanged += Inputs_ChildChanged;
		}

		internal void Inputs_ChildChanged(IChangeable obj) {
			ComputeOutput();
		}

		public void ComputeOutput() {
			if (Output.Linked) {
				var ins = new In[Inputs.Count];
				for (var i = 0; i < ins.Length; i++) {
					ins[i] = Inputs[i].Target?.Value ?? default;
				}
				Output.LinkedValue = Compute(ins);
			}
		}

		public abstract Out Compute(In[] ins);
	}

	public abstract class AbDualOperation<Out, In> : Component where Out : struct where In : struct
	{
		public readonly Linker<Out> Output;

		[OnChanged(nameof(BindOne))]
		public readonly SyncRef<IValueSource<In>> InputOne;

		[OnChanged(nameof(BindTwo))]
		public readonly SyncRef<IValueSource<In>> InputTwo;

		IValueSource<In> _lastInputOne;
		IValueSource<In> _lastInputTwo;

		internal void BindOne() {
			if (_lastInputOne is not null) {
				_lastInputOne.Changed -= Compute;
			}
			if (InputOne.Target is not null) {
				_lastInputOne.Changed += Compute;
				Compute(null);
			}
			_lastInputOne = InputOne.Target;
		}

		internal void BindTwo() {
			if (_lastInputTwo is not null) {
				_lastInputTwo.Changed -= Compute;
			}
			if (InputTwo.Target is not null) {
				_lastInputTwo.Changed += Compute;
				Compute(null);
			}
			_lastInputTwo = InputTwo.Target;
		}

		public void ComputeOutput() {
			Compute(null);
		}

		internal void Compute(IChangeable changeable) {
			if (Output.Linked) {
				var a = InputOne.Target?.Value ?? default;
				var b = InputTwo.Target?.Value ?? default;
				Output.LinkedValue = Compute(a,b);
			}
		}
		public abstract Out Compute(In a, In b);
	}

	public abstract class AbTripleOperation<Out, In> : Component where Out : struct where In : struct
	{
		public readonly Linker<Out> Output;

		[OnChanged(nameof(BindOne))]
		public readonly SyncRef<IValueSource<In>> InputOne;

		[OnChanged(nameof(BindTwo))]
		public readonly SyncRef<IValueSource<In>> InputTwo;

		[OnChanged(nameof(BindThree))]
		public readonly SyncRef<IValueSource<In>> InputThree;

		IValueSource<In> _lastInputOne;
		IValueSource<In> _lastInputTwo;
		IValueSource<In> _lastInputThree;

		internal void BindOne() {
			if (_lastInputOne is not null) {
				_lastInputOne.Changed -= Compute;
			}
			if (InputOne.Target is not null) {
				_lastInputOne.Changed += Compute;
				Compute(null);
			}
			_lastInputOne = InputOne.Target;
		}

		internal void BindTwo() {
			if (_lastInputTwo is not null) {
				_lastInputTwo.Changed -= Compute;
			}
			if (InputTwo.Target is not null) {
				_lastInputTwo.Changed += Compute;
				Compute(null);
			}
			_lastInputTwo = InputTwo.Target;
		}

		internal void BindThree() {
			if (_lastInputThree is not null) {
				_lastInputThree.Changed -= Compute;
			}
			if (InputThree.Target is not null) {
				_lastInputThree.Changed += Compute;
				Compute(null);
			}
			_lastInputThree = InputThree.Target;
		}
		public void ComputeOutput() {
			Compute(null);
		}

		internal void Compute(IChangeable changeable) {
			if (Output.Linked) {
				var a = InputOne.Target?.Value ?? default;
				var b = InputTwo.Target?.Value ?? default;
				var c = InputThree.Target?.Value ?? default;
				Output.LinkedValue = Compute(a, b,c);
			}
		}
		public abstract Out Compute(In a, In b,In c);
	}



	public abstract class AbSingleOperation<Out, In> : Component where Out : struct where In : struct
	{
		public readonly Linker<Out> Output;

		[OnChanged(nameof(Bind))]
		public readonly SyncRef<IValueSource<In>> Input;

		IValueSource<In> _lastInput;

		internal void Bind() {
			if (_lastInput is not null) {
				_lastInput.Changed -= Compute;
			}
			if(Input.Target is not null) {
				_lastInput.Changed += Compute;
				Compute(null);
			}
			_lastInput = Input.Target;
		}
		public void ComputeOutput() {
			Compute(null);
		}
		internal void Compute(IChangeable changeable) {
			if (Output.Linked) {
				if(Input.Target is not null) {
					Output.LinkedValue = Compute(Input.Target.Value);
				}
			}
		}
		public abstract Out Compute(In ins);
	}
}
