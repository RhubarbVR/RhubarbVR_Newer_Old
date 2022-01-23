
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class Color32Formatter : IMessagePackFormatter<Color32>
	{
		public void Serialize(ref MessagePackWriter writer, Color32 value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(byte R, byte G, byte B, byte A)>().Serialize(ref writer, (R: value.r, G: value.g, B: value.b, A: value.a), options);
		}

		public Color32 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (R, G, B, A) = options.Resolver.GetFormatterWithVerify<(byte R, byte G, byte B, byte A)>().Deserialize(ref reader, options);
			return new Color32(R, G, B, A);
		}
	}
}
