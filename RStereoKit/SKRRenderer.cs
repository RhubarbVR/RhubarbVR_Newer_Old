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
	public class SKRRenderer : IRRenderer
	{
		public bool GetEnableSky() {
			return Renderer.EnableSky;
		}

		public RNumerics.Matrix GetCameraRootMatrix() {
			return new RNumerics.Matrix(Renderer.CameraRoot.m);
		}

		public void SetEnableSky(bool e) {
			Renderer.EnableSky = e;
		}

		public void SetCameraRootMatrix(RNumerics.Matrix m) {
			Renderer.CameraRoot = new StereoKit.Matrix(m.m);
		}
	}
}
