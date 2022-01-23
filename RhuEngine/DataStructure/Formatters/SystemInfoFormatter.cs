
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class SystemInfoFormatter : IMessagePackFormatter<SystemInfo>
	{
		public void Serialize(ref MessagePackWriter writer, SystemInfo value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Display displayType, int displayWidth, int displayHeight)>().Serialize(ref writer, (value.displayType, value.displayWidth, value.displayHeight), options);
		}

		public SystemInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (displayType, displayWidth, displayHeight) = options.Resolver.GetFormatterWithVerify<(Display displayType, int displayWidth, int displayHeight)>().Deserialize(ref reader, options);
			return new SystemInfo { displayType = displayType, displayWidth = displayWidth, displayHeight = displayHeight };
		}
	}
}
