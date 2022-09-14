using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine.Linker;
using RNumerics;
using System.Runtime.InteropServices;
using System.Numerics;


namespace RStereoKit
{
	public sealed class SKHead : IRHead
	{
		public Vector3f Position => (Vector3)Input.Head.position;

		public RNumerics.Matrix HeadMatrix => Input.Head.ToMatrix(1).m;
	}

	public sealed class SKMouse : IRMouse
	{
		public Vector2f ScrollChange => new Vector2f(0, Input.Mouse.scrollChange / 1000);

		public Vector2f PosChange => (Vector2f)(Vector2)Input.Mouse.posChange;

		public bool HideMouse { get; set; }
		public bool CenterMouse { get; set; }
	}

	public sealed class SKKeyPress : IKeyPress
	{
		public SKKeyPress(BtnState btnState) {
			BtnState = btnState;
		}

		public BtnState BtnState { get; }

		public bool IsActive() {
			return BtnState.IsActive();
		}

		public bool IsJustActive() {
			return BtnState.IsJustActive();
		}
	}

	public sealed class SKStick : IRStick
	{
		public SKStick(Controller c) {
			Controller = c;
		}

		public Controller Controller { get; set; }
		public Vector2f YX => (Vector2f)(Vector2)Controller.stick;
	}

	public sealed class SKConttroller : IRController
	{
		public SKConttroller(Controller c) {
			Controller = c;
			Stick = new SKStick(Controller);
		}

		public Controller Controller { get; set; }

		public float Trigger => Controller.trigger;

		public float Grip => Controller.grip;

		public IKeyPress StickClick => new SKKeyPress(Controller.stickClick);

		public IKeyPress X1 => new SKKeyPress(Controller.x1);

		public IKeyPress X2 => new SKKeyPress(Controller.x2);

		public IRStick Stick { get; set; }

		public string Model => "Unkown";

		public float BatteryPercentage =>1f;

		public KnownControllers ModelEnum => KnownControllers.Unknown;
	}

	public sealed class SKHand : IRHand
	{
		public StereoKit.Handed Handed { get; set; }

		public SKHand(StereoKit.Handed handed) {
			Handed = handed;
		}

		public RNumerics.Matrix Wrist => Input.Hand(Handed).wrist.ToMatrix().m;
	}

	public sealed class SKInput : IRInput
	{
		public IRHead Head { get; set; } = new SKHead();


		public IRMouse Mouse { get; set; } = new SKMouse();

		public IRHand HLeft { get; set; } = new SKHand(StereoKit.Handed.Left);

		public IRHand HRight { get; set; } = new SKHand(StereoKit.Handed.Right);

		public IRController CLeft { get; set; } = new SKConttroller(Input.Controller(StereoKit.Handed.Left));

		public IRController CRight { get; set; } = new SKConttroller(Input.Controller(StereoKit.Handed.Right));

		public string TypeDelta
		{
			get {
				var chare = Input.TextConsume();
				Input.TextReset();
				return chare == '\0' ? "" : chare.ToString();
			}
		}

		public IRController Controller(RhuEngine.Linker.Handed handed) {
			switch (handed) {
				case RhuEngine.Linker.Handed.Left:
					return CLeft;
				case RhuEngine.Linker.Handed.Right:
					return CRight;
				case RhuEngine.Linker.Handed.Max:
					break;
				default:
					break;
			}
			return null;
		}

		public IRHand Hand(RhuEngine.Linker.Handed value) {
			switch (value) {
				case RhuEngine.Linker.Handed.Left:
					return HLeft;
				case RhuEngine.Linker.Handed.Right:
					return HRight;
				case RhuEngine.Linker.Handed.Max:
					break;
				default:
					break;
			}
			return null;
		}

		public IKeyPress Key(RhuEngine.Linker.Key secondKey) {
			return new SKKeyPress(Input.Key((StereoKit.Key)secondKey));
		}
	}
}
