
using MessagePack;
using MessagePack.Formatters;

using StereoKit;
using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class Vec2Formatter : IMessagePackFormatter<Vec2>
	{
		public void Serialize(ref MessagePackWriter writer, Vec2 value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float X, float Y)>().Serialize(ref writer, (X: value.x, Y: value.y), options);
		}

		public Vec2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (X, Y) = options.Resolver.GetFormatterWithVerify<(float X, float Y)>().Deserialize(ref reader, options);
			return new Vec2(X, Y);
		}
	}
}
