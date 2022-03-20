
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class VertexFormatter : IMessagePackFormatter<Vertex>
	{
		public void Serialize(ref MessagePackWriter writer, Vertex value, MessagePackSerializerOptions options) {
			options.Resolver.GetFormatterWithVerify<(Vec3 position, Vec3 normal, Vec2 textureCoordinates, Color32 color)>().Serialize(ref writer, (position: value.pos, normal: value.norm, textureCoordinates: value.uv, color: value.col), options);
		}

		public Vertex Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (position, normal, textureCoordinates, color) = options.Resolver.GetFormatterWithVerify<(Vec3 position, Vec3 normal, Vec2 textureCoordinates, Color32 color)>().Deserialize(ref reader, options);
			return new Vertex(position, normal, textureCoordinates, color);
		}
	}
}
