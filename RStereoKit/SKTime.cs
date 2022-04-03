using System.Text;
using StereoKit;
using RhuEngine;
using RhuEngine.Linker;
using RNumerics;
using RhuEngine.WorldObjects;
using System.Numerics;

namespace RStereoKit
{
	public class SKTime : IRTime
	{
		public float Elapsedf => Time.Elapsedf;
	}
}
