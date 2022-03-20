
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class SphericalHarmonicsFormatter : IMessagePackFormatter<SphericalHarmonics>
	{
		public void Serialize(ref MessagePackWriter writer, SphericalHarmonics value, MessagePackSerializerOptions options) {
			var valuea = value.ToArray();
			options.Resolver.GetFormatterWithVerify<Vec3[]>().Serialize(ref writer, valuea, options);
		}

		public SphericalHarmonics Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var val = options.Resolver.GetFormatterWithVerify<Vec3[]>().Deserialize(ref reader, options);
			return new SphericalHarmonics(val);
		}
	}
}
