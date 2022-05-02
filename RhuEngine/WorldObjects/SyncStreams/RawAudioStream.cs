using System;

using RhuEngine.DataStructure;
using RhuEngine.WorldObjects.ECS;
using System.Collections.Generic;

namespace RhuEngine.WorldObjects
{
	public class RawAudioStream : AudioStream
	{	
		public enum SampleBitSize
		{
			float32bits,
			short16bits,
			byte8bits,
		}
		[Default(SampleBitSize.short16bits)]
		public readonly Sync<SampleBitSize> bitSize;

		public override byte[] SendAudioSamples(float[] audio) {
			return bitSize.Value switch {
				SampleBitSize.byte8bits => WriteByte8bits(audio),
				SampleBitSize.short16bits => WriteShort16bits(audio),
				SampleBitSize.float32bits => WriteFloat32bits(audio),
				_ => throw new Exception("Uknown SampleBitSize"),
			};
		}

		public override float[] ProssesAudioSamples(byte[] data) {
			return data is null
				? (new float[SampleCount])
				: bitSize.Value switch {
				SampleBitSize.byte8bits => ReadByte8bits(data),
				SampleBitSize.short16bits => ReadShort16bits(data),
				SampleBitSize.float32bits => ReadFloat32bits(data),
				_ => throw new Exception("Uknown SampleBitSize"),
			};
		}

		private byte[] WriteByte8bits(float[] audio) {
			var byteArray = new byte[audio.Length];
			for (var i = 0; i < audio.Length; i++) {
				byteArray[i] = (byte)((((short)Math.Floor(audio[i] * 32767) + 32768) >> 8) & 0xFF);
			}
			return byteArray;
		}

		private float[] ReadByte8bits(byte[] data) {
			var audio = new float[data.Length];
			for (var i = 0; i < audio.Length; i++) {
				audio[i] = ((data[i] << 8) - 32768) * (1 / 32768.0f);
			}
			return audio;
		}

		private byte[] WriteShort16bits(float[] audio) {
			var shortArray = new short[audio.Length];
			for (var i = 0; i < audio.Length; i++) {
				shortArray[i] = (short)Math.Floor(audio[i] * 32767);
			}
			var byteArray = new byte[audio.Length * 2];
			Buffer.BlockCopy(shortArray, 0, byteArray, 0,byteArray.Length);
			return byteArray;
		}

		private float[] ReadShort16bits(byte[] data) {
			var shortArray = new byte[data.Length/2];
			Buffer.BlockCopy(data, 0, shortArray, 0, data.Length);
			var audio = new float[shortArray.Length];
			for (var i = 0; i < audio.Length; i++) {
				audio[i] = shortArray[i] * (1 / 32768.0f);
			}
			return audio;
		}


		private byte[] WriteFloat32bits(float[] audio) {
			var byteArray = new byte[audio.Length * 4];
			Buffer.BlockCopy(audio, 0, byteArray, 0, byteArray.Length);
			return byteArray;
		}

		private float[] ReadFloat32bits(byte[] data) {
			var floatArray = new float[data.Length / 4];
			Buffer.BlockCopy(data, 0, floatArray, 0, data.Length);
			return floatArray;
		}
	}
}
