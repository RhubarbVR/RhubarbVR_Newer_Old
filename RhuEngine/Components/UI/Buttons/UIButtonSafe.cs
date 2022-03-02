using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public readonly struct SafeCall
	{
		internal readonly long _number;
		internal SafeCall(long number) {
			_number = number;
		}
		internal static SafeCall MakeResponses(long number,Engine Engine) {
			return new SafeCall(((number * Engine.version.GetHashCode()) << Engine.version.Major) >> Engine.version.Minor);
		}
	}

	[Category(new string[] { "UI\\Buttons" })]
	public class UIButtonSafe : UIComponent
	{
		[Default("Safe Button")]
		public Sync<string> Text;

		public Sync<Vec2> Size;
		
		public SyncDelegate<Func<SafeCall,(Action,SafeCall)>> onClick;

		private static readonly Random _rand = new();

		private void Call() {
			var result = (long)_rand.Next((int)(long.MinValue >> 32), (int)(long.MaxValue >> 32));
			result <<= 32;
			result |= (long)_rand.Next(int.MinValue, int.MaxValue);
			var e = new SafeCall(result);
			var resonses = onClick.Target?.Invoke(e);
			if(resonses.Value.Item2._number == ((result * Engine.version.GetHashCode()) << Engine.version.Major) >> Engine.version.Minor) {
				resonses.Value.Item1.Invoke();
			}
		}

		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			if(Size.Value.v == Vec2.Zero.v) {
				if (UI.Button(Text.Value ?? "")) {
					AddWorldCoroutine(() => Call());
				}
			}
			else {
				if (UI.Button(Text.Value ?? "", Size)) {
					AddWorldCoroutine(() => Call());
				}
			}
			UI.PopId();
		}
	}
}
