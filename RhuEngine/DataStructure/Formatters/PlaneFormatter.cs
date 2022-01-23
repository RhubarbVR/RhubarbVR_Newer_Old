
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class PlaneFormatter : IMessagePackFormatter<Plane>
	{
		public void Serialize(ref MessagePackWriter writer, Plane value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 normal, float d)>().Serialize(ref writer, (value.normal, value.d), options);
		}

		public Plane Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (normal, d) = options.Resolver.GetFormatterWithVerify<(Vec3 normal, float d)>().Deserialize(ref reader, options);
			return new Plane(normal, d);
		}
	}
}
