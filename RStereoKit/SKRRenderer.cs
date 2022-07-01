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
		public float _minClip = 0.08f;

		public float _farClip = 50;
		public float MinClip { get => _minClip; set { _minClip = value; Renderer.SetClip(_minClip, _farClip); } }
		public float FarClip { get => _farClip; set { _farClip = value; Renderer.SetClip(_minClip, _farClip); } }

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
