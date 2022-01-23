
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class QautFormatter : IMessagePackFormatter<Quat>
	{
		public void Serialize(ref MessagePackWriter writer, Quat value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float X, float Y, float Z, float W)>().Serialize(ref writer, (X: value.x, Y: value.y, Z: value.z, W: value.w), options);
		}

		public Quat Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (X, Y, Z, W) = options.Resolver.GetFormatterWithVerify<(float X, float Y, float Z, float W)>().Deserialize(ref reader, options);
			return new Quat(X, Y, Z, W);
		}
	}
}
