
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class SphereFormatter : IMessagePackFormatter<Sphere>
	{
		public void Serialize(ref MessagePackWriter writer, Sphere value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 center, float diameter)>().Serialize(ref writer, (value.center, diameter: value.Diameter), options);
		}

		public Sphere Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (center, diameter) = options.Resolver.GetFormatterWithVerify<(Vec3 center, float diameter)>().Deserialize(ref reader, options);
			return new Sphere(center, diameter);
		}
	}
}
