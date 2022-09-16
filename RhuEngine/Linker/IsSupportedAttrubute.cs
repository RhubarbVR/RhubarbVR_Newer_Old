using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public class SupportedAttribute : Attribute
	{
		public SupportedFancyFeatures FancyFeatures { get; private set; }

		public bool GetIfSupported(Engine engine) {
			return (engine.EngineLink.SupportedFeatures & FancyFeatures) != SupportedFancyFeatures.Basic;
		}

		public SupportedAttribute(SupportedFancyFeatures supportedFancyFeatures) {
			FancyFeatures = supportedFancyFeatures;
		}
	}
}
