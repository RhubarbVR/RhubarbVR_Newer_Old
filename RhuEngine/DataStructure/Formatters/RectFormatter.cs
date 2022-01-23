
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class RectFormatter : IMessagePackFormatter<Rect>
	{
		public void Serialize(ref MessagePackWriter writer, Rect value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float x, float y, float width, float height)>().Serialize(ref writer, (value.x, value.y, value.width, value.height), options);
		}

		public Rect Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (x, y, width, height) = options.Resolver.GetFormatterWithVerify<(float x, float y, float width, float height)>().Deserialize(ref reader, options);
			return new Rect(x, y, width, height);
		}
	}
}
