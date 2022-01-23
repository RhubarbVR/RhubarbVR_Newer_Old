
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class RayFormatter : IMessagePackFormatter<Ray>
	{
		public void Serialize(ref MessagePackWriter writer, Ray value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 position, Vec3 direction)>().Serialize(ref writer, (value.position, value.direction), options);
		}

		public Ray Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (position, direction) = options.Resolver.GetFormatterWithVerify<(Vec3 position, Vec3 direction)>().Deserialize(ref reader, options);
			return new Ray { position = position, direction = direction };
		}
	}
}
