
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class SHLightFormatter : IMessagePackFormatter<SHLight>
	{
		public void Serialize(ref MessagePackWriter writer, SHLight value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Color color, Vec3 dir)>().Serialize(ref writer, (value.color, dir: value.directionTo), options);
		}

		public SHLight Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (color, dir) = options.Resolver.GetFormatterWithVerify<(Color color, Vec3 dir)>().Deserialize(ref reader, options);
			return new SHLight { color = color, directionTo = dir };
		}
	}
}
