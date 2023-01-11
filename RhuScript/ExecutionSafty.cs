using System.Reflection.Emit;


namespace RhuScript
{
	public abstract class ScriptSaftyException : Exception {
		public ScriptSaftyException(string message) : base($"Script Limmit {message}") {

		}
	}

	public sealed class ScriptExecuteMaxException : ScriptSaftyException
	{
		public ScriptExecuteMaxException() : base("Execute Limit") {
		}
	}
	public sealed class ScriptStackMaxException : ScriptSaftyException
	{
		public ScriptStackMaxException() : base("Stack Limit") {
		}
	}
	public static class ExecutionSafty
	{
		[ThreadStatic]
		private static ulong _stackPoint;
		[ThreadStatic]
		private static ulong _executePoint;

		public const ulong STACK_MAX = 255;

		public const ulong EXECUTE_MAX = 55000;

		public static void End() {
			_stackPoint--;
		}
		public static void Start() {
			_stackPoint++;
			_executePoint++;
			if(_executePoint >= EXECUTE_MAX) {
				throw new ScriptExecuteMaxException();
			}
			if (_stackPoint >= STACK_MAX) {
				throw new ScriptStackMaxException();
			}
		}
	}

}