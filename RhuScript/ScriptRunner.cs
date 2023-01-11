
using RhuScript.ScriptParts;

namespace RhuScript
{
	public enum RunningMode
	{
		Disabled,
		MethodReflection,
		ILGen,
	}

	public sealed class ScriptRunner
	{
		public RunningMode runningMode;

		private ScriptRoot _scriptPart;

		public ScriptRoot Script
		{
			get => _scriptPart;
			set {
				_scriptPart = value;
				BuildScript();
			}
		}

		private void BuildScript() {
			_scriptPart.Build();
			if(runningMode == RunningMode.ILGen) {
				//_dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(),typeof(void), Array.Empty<Type>());
			}
		}

		public ScriptRunner(ScriptRoot scriptPart = null, RunningMode runningMode = RunningMode.MethodReflection) {
			this.runningMode = runningMode;
			Script = scriptPart;
		}

		public void Invoke() {
			switch (runningMode) {
				case RunningMode.Disabled:
					throw new InvalidProgramException("Disabled");
				case RunningMode.MethodReflection:
					if(Script is IScriptVoidMethod method) {
						method.Invoke();
					}
					break;
				case RunningMode.ILGen:
					throw new InvalidProgramException("Not Supported");
				default:
					throw new InvalidProgramException("Disabled");
			}
		}

	}

}