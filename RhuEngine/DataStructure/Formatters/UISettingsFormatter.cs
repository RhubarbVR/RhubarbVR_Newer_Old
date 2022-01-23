
using MessagePack;
using MessagePack.Formatters;

using StereoKit;
using SharedModels;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class UISettingsFormatter : IMessagePackFormatter<UISettings>
	{
		public void Serialize(ref MessagePackWriter writer, UISettings value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(float padding, float gutter, float depth, float backplateDepth, float backplateBorder)>().Serialize(ref writer, (value.padding, value.gutter, value.depth, value.backplateDepth, value.backplateBorder), options);
		}

		public UISettings Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (padding, gutter, depth, backplateDepth, backplateBorder) = options.Resolver.GetFormatterWithVerify<(float padding, float gutter, float depth, float backplateDepth, float backplateBorder)>().Deserialize(ref reader, options);
			return new UISettings { padding = padding, gutter = gutter, depth = depth, backplateDepth = backplateDepth, backplateBorder = backplateBorder };
		}
	}
}
