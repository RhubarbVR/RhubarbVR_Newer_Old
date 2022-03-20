
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class GradientKeyFormatter : IMessagePackFormatter<GradientKey>
	{
		public void Serialize(ref MessagePackWriter writer, GradientKey value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Color color, float pos)>().Serialize(ref writer, (value.color, pos: value.position), options);
		}

		public GradientKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (color, pos) = options.Resolver.GetFormatterWithVerify<(Color color, float pos)>().Deserialize(ref reader, options);
			return new GradientKey(color, pos);
		}
	}
}
