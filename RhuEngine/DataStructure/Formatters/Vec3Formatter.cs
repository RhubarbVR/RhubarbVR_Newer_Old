
using MessagePack;
using MessagePack.Formatters;

using StereoKit;
using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class Vec3Formatter : IMessagePackFormatter<Vec3>
	{
		public void Serialize(ref MessagePackWriter writer, Vec3 value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float X, float Y, float Z)>().Serialize(ref writer, (X: value.x, Y: value.y, Z: value.z), options);
		}

		public Vec3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (X, Y, Z) = options.Resolver.GetFormatterWithVerify<(float X, float Y, float Z)>().Deserialize(ref reader, options);
			return new Vec3(X, Y, Z);
		}
	}
}
