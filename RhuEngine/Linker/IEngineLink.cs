﻿using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Managers;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface IEngineLink
	{
		bool ForceLibLoad { get; }
		bool SpawnPlayer { get; }
		bool CanRender { get; }
		bool CanAudio { get; }
		bool CanInput { get; }
		string BackendID { get; }
		bool InVR { get; }

		event Action<bool> VRChange;

		bool LiveVRChange { get; }

		void ChangeVR(bool value);

		Type RenderSettingsType { get; }
		void BindEngine(Engine engine);
		void LoadInput(InputManager manager);

		void Start();
		void LoadStatics();
		void LoadArgs();
	}
}
