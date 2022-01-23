
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class ColorFormatter : IMessagePackFormatter<Color>
	{
		public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float R, float G, float B, float A)>().Serialize(ref writer, (R: value.r, G: value.g, B: value.b, A: value.a), options);
		}

		public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (R, G, B, A) = options.Resolver.GetFormatterWithVerify<(float R, float G, float B, float A)>().Deserialize(ref reader, options);
			return new Color(R, G, B, A);
		}
	}
}
