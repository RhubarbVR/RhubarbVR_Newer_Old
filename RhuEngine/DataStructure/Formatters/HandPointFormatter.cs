
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class HandPointFormatter : IMessagePackFormatter<HandJoint>
	{
		public void Serialize(ref MessagePackWriter writer, HandJoint value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 position, Quat orientation, float radius)>().Serialize(ref writer, (value.position, value.orientation, value.radius), options);
		}

		public HandJoint Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (position, orientation, radius) = options.Resolver.GetFormatterWithVerify<(Vec3 position, Quat orientation, float radius)>().Deserialize(ref reader, options);
			return new HandJoint(position, orientation, radius);
		}
	}
}
