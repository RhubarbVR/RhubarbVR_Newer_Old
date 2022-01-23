
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class PointerFormatter : IMessagePackFormatter<Pointer>
	{
		public void Serialize(ref MessagePackWriter writer, Pointer value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(InputSource source, BtnState tracked, BtnState state, Ray ray, Quat orientation)>().Serialize(ref writer, (value.source, value.tracked, value.state, value.ray, value.orientation), options);
		}

		public Pointer Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (source, tracked, state, ray, orientation) = options.Resolver.GetFormatterWithVerify<(InputSource source, BtnState tracked, BtnState state, Ray ray, Quat orientation)>().Deserialize(ref reader, options);
			return new Pointer { source = source, tracked = tracked, state = state, ray = ray, orientation = orientation };
		}
	}
}
