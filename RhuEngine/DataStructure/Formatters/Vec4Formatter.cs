
using MessagePack;
using MessagePack.Formatters;

using StereoKit;
using SharedModels;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class Vec4Formatter : IMessagePackFormatter<Vec4>
	{
		public void Serialize(ref MessagePackWriter writer, Vec4 value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float X, float Y, float Z, float W)>().Serialize(ref writer, (X: value.x, Y: value.y, Z: value.z, W: value.w), options);
		}

		public Vec4 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (X, Y, Z, W) = options.Resolver.GetFormatterWithVerify<(float X, float Y, float Z, float W)>().Deserialize(ref reader, options);
			return new Vec4(X, Y, Z, W);
		}
	}
}
