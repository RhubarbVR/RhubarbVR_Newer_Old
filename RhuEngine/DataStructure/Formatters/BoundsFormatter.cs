
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class BoundsFormatter : IMessagePackFormatter<Bounds>
	{
		public void Serialize(ref MessagePackWriter writer, Bounds value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 center, Vec3 dimensions)>().Serialize(ref writer, (value.center, value.dimensions), options);
		}

		public Bounds Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (center, dimensions) = options.Resolver.GetFormatterWithVerify<(Vec3 center, Vec3 dimensions)>().Deserialize(ref reader, options);
			return new Bounds(center, dimensions);
		}
	}
}
