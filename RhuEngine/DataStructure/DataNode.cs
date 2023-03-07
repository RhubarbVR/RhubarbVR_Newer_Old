using System;
using System.IO;
using System.Runtime.CompilerServices;

using RNumerics;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure
{
	public interface IDateNodeValue
	{
		public object ObjectValue { get; set; }
		public Type Type { get; }
	}

	public sealed class DataNode<T> : IDataNode, IDateNodeValue
	{
		public DataNode(T def = default) {
			Value = def;
		}

		public DataNode() {
			Value = default;
		}
		public T Value { get; set; }

		public object ObjectValue { get => Value; set => Value = (T)value; }
		public Type Type => typeof(T);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(DataNode<T> data) => data.Value;

		public void SaveAction(DataSaver DataSaver) {
			//Not needed no children
		}

		public void InitData() {
			//Not needed no children
		}

		public void ReadChildEnd(IDataNode child) {
			//Not needed no children
		}

		public void Serlize(BinaryWriter binaryWriter) {
			if (Value is ISerlize serlize) {
				serlize.Serlize(binaryWriter);
			}
			else if (Value is bool bv) {
				binaryWriter.Write(bv);
			}
			else if (Value is byte bb) {
				binaryWriter.Write(bb);
			}
			else if (Value is sbyte bsb) {
				binaryWriter.Write(bsb);
			}
			else if (Value is short shortv) {
				binaryWriter.Write(shortv);
			}
			else if (Value is ushort ushortv) {
				binaryWriter.Write(ushortv);
			}
			else if (Value is int intv) {
				binaryWriter.Write(intv);
			}
			else if (Value is uint uintv) {
				binaryWriter.Write(uintv);
			}
			else if (Value is long longv) {
				binaryWriter.Write(longv);
			}
			else if (Value is ulong ulongv) {
				binaryWriter.Write(ulongv);
			}
			else if (Value is float floatv) {
				binaryWriter.Write(floatv);
			}
			else if (Value is double doublev) {
				binaryWriter.Write(doublev);
			}
			else if (Value is char charv) {
				binaryWriter.Write(charv);
			}
			else if (Value is decimal decimalv) {
				binaryWriter.Write(decimalv);
			}
			else if (Value is DateTime DateTimev) {
				binaryWriter.Write(DateTimev.Ticks);
			}
			else if (Type == typeof(string)) {
				if(Value is null) {
					binaryWriter.Write((byte)1);
				}
				else {
					binaryWriter.Write((byte)0);
					binaryWriter.Write((string)(object)Value);
				}
			}
			else if (Value is string[] stringarrayv) {
				binaryWriter.Write(stringarrayv.Length);
				for (var i = 0; i < stringarrayv.Length; i++) {
					binaryWriter.Write(stringarrayv[i]);
				}
			}
			else if (Type == typeof(string[])) {
				binaryWriter.Write(0);
			}
			else if (Value is byte[] bytearrayv) {
				binaryWriter.Write(bytearrayv.Length);
				for (var i = 0; i < bytearrayv.Length; i++) {
					binaryWriter.Write(bytearrayv[i]);
				}
			}
			else if (Type == typeof(byte[])) {
				binaryWriter.Write(0);
			}
			else if (Value is float[] floatarrayv) {
				binaryWriter.Write(floatarrayv.Length);
				for (var i = 0; i < floatarrayv.Length; i++) {
					binaryWriter.Write(floatarrayv[i]);
				}
			}
			else if (Type == typeof(float[])) {
				binaryWriter.Write(0);
			}
			else if (Value is int[] intarrayv) {
				binaryWriter.Write(intarrayv.Length);
				for (var i = 0; i < intarrayv.Length; i++) {
					binaryWriter.Write(intarrayv[i]);
				}
			}
			else if (Type == typeof(int[])) {
				binaryWriter.Write(0);
			}
			else {
				throw new NotImplementedException("type " + Type.Name);
			}
		}

		public void DeSerlize(BinaryReader binaryReader) {
			if (Value is ISerlize serlize) {
				serlize.DeSerlize(binaryReader);
				Value = (T)serlize;
			}
			else if (Value is bool) {
				Value = (T)(object)binaryReader.ReadBoolean();
			}
			else if (Value is byte) {
				Value = (T)(object)binaryReader.ReadByte();
			}
			else if (Value is sbyte) {
				Value = (T)(object)binaryReader.ReadSByte();
			}
			else if (Value is short) {
				Value = (T)(object)binaryReader.ReadInt16();
			}
			else if (Value is ushort) {
				Value = (T)(object)binaryReader.ReadUInt16();
			}
			else if (Value is int) {
				Value = (T)(object)binaryReader.ReadInt32();
			}
			else if (Value is uint) {
				Value = (T)(object)binaryReader.ReadUInt32();
			}
			else if (Value is long) {
				Value = (T)(object)binaryReader.ReadInt64();
			}
			else if (Value is ulong) {
				Value = (T)(object)binaryReader.ReadUInt64();
			}
			else if (Value is float) {
				Value = (T)(object)binaryReader.ReadSingle();
			}
			else if (Value is double) {
				Value = (T)(object)binaryReader.ReadDouble();
			}
			else if (Value is char) {
				Value = (T)(object)binaryReader.ReadChar();
			}
			else if (Value is decimal) {
				Value = (T)(object)binaryReader.ReadDecimal();
			}
			else if (Value is DateTime) {
				Value = (T)(object)new DateTime(binaryReader.ReadInt64());
			}
			else if (Type == typeof(string)) {
				var isNull = binaryReader.ReadByte();
				if(isNull != 0) {
					Value = (T)(object)null;
				}
				else {
					Value = (T)(object)binaryReader.ReadString();
				}
			}
			else if (Type == typeof(string[])) {
				var array = new string[binaryReader.ReadInt32()];
				for (var i = 0; i < array.Length; i++) {
					array[i] = binaryReader.ReadString();
				}
				Value = (T)(object)array;
			}
			else if (Type == typeof(byte[])) {
				var array = new byte[binaryReader.ReadInt32()];
				for (var i = 0; i < array.Length; i++) {
					array[i] = binaryReader.ReadByte();
				}
				Value = (T)(object)array;
			}
			else if (Type == typeof(float[])) {
				var array = new float[binaryReader.ReadInt32()];
				for (var i = 0; i < array.Length; i++) {
					array[i] = binaryReader.ReadSingle();
				}
				Value = (T)(object)array;
			}
			else if (Type == typeof(int[])) {
				var array = new int[binaryReader.ReadInt32()];
				for (var i = 0; i < array.Length; i++) {
					array[i] = binaryReader.ReadInt32();
				}
				Value = (T)(object)array;
			}
			else {
				throw new NotImplementedException();
			}
		}
	}
}
