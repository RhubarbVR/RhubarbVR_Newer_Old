
using MessagePack;
using MessagePack.Formatters;

using SharedModels;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class LinePointFormatter : IMessagePackFormatter<LinePoint>
	{
		public void Serialize(ref MessagePackWriter writer, LinePoint value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 point, Color32 color, float thickness)>().Serialize(ref writer, (point: value.pt, value.color, value.thickness), options);
		}

		public LinePoint Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (point, color, thickness) = options.Resolver.GetFormatterWithVerify<(Vec3 point, Color32 color, float thickness)>().Deserialize(ref reader, options);
			return new LinePoint(point, color, thickness);
		}
	}
}
