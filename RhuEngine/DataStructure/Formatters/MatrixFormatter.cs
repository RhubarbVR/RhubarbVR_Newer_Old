
using MessagePack;
using MessagePack.Formatters;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure.Formatters
{
	[Formatter]
	public class MatrixFormatter : IMessagePackFormatter<Matrix>
	{
		public void Serialize(ref MessagePackWriter writer, Matrix value, MessagePackSerializerOptions options) {
			value.Decompose(out var trans, out var rot, out var scale);
			options.Resolver.GetFormatterWithVerify<(Quat rot, Vec3 pos, Vec3 scale)>().Serialize(ref writer, (rot, pos: trans, scale), options);
		}

		public Matrix Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
			var (rot, pos, scale) = options.Resolver.GetFormatterWithVerify<(Quat rot, Vec3 pos, Vec3 scale)>().Deserialize(ref reader, options);
			return Matrix.TRS(pos, rot, scale);
		}
	}
}
