using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine;
using RhuEngine.Linker;
using RNumerics;

namespace RStereoKit
{
	public class SkRText : IRText
	{
		public void Add(string v, RNumerics.Matrix p) {
			Text.Add(v, new StereoKit.Matrix(p.m));
		}

		public Vector2f Size(string text) {
			return (Vector2f)Text.Size(text).v;
		}
	}
}
