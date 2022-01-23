
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class PosFormatter : IMessagePackFormatter<Pose>
	{
		public void Serialize(ref MessagePackWriter writer, Pose value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 position, Quat rot)>().Serialize(ref writer, (value.position, rot: value.orientation), options);
		}

		public Pose Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (position, rot) = options.Resolver.GetFormatterWithVerify<(Vec3 position, Quat rot)>().Deserialize(ref reader, options);
			return new Pose { position = position, orientation = rot };
		}
	}
}
