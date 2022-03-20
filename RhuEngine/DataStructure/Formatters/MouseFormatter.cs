
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class MouseFormatter : IMessagePackFormatter<Mouse>
	{
		public void Serialize(ref MessagePackWriter writer, Mouse value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(bool available, Vec2 pos, Vec2 posChange, float scroll, float scrollChange)>().Serialize(ref writer, (value.available, value.pos, value.posChange, value.scroll, value.scrollChange), options);
		}

		public Mouse Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (available, pos, posChange, scroll, scrollChange) = options.Resolver.GetFormatterWithVerify<(bool available, Vec2 pos, Vec2 posChange, float scroll, float scrollChange)>().Deserialize(ref reader, options);
			return new Mouse { available = available, pos = pos, posChange = posChange, scroll = scroll, scrollChange = scrollChange };
		}
	}
}
