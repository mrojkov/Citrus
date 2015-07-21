using System;
using System.Text;

namespace Yuzu
{
	internal enum WireType {
		Varint = 0,
		Double = 1,
		LengthDelimited = 2,
		StartGroup = 3,
		EndGroup = 4,
		Single = 5,
	}

	public class ProtobufSerializeOptions
	{
	}

	public class ProtobufSerializer : AbstractWriterSerializer
	{
		public ProtobufSerializeOptions ProtobufOptions = new ProtobufSerializeOptions();

		private void WriteVarint(long value)
		{
			do {
				byte b = (byte)(value & 0x7f);
				value >>= 7;
				if (value != 0) {
					b |= 0x80;
				}
				writer.Write(b);
			} while (value != 0);
		}

		protected override void ToWriter(object obj)
		{
			int count = 1;
			foreach (var f in obj.GetType().GetFields()) {
				var t = f.FieldType;
				if (t == typeof(int)) {
					WriteVarint((count << 3) + (int)WireType.Varint);
					WriteVarint((int)f.GetValue(obj));
				}
				else if (t == typeof(string)) {
					var s = f.GetValue(obj).ToString();
					WriteVarint((count << 3) + (int)WireType.LengthDelimited);
					WriteVarint(Encoding.UTF8.GetByteCount(s));
					writer.Write(Encoding.UTF8.GetBytes(s));
				}
				else {
					throw new NotImplementedException(t.Name);
				}
				++count;
			}
		}
	}

	public class ProtobufDeserializeOptions
	{
	}

	public class ProtobufDeserializer : AbstractDeserializer
	{
		public ProtobufDeserializeOptions ProtobufOptions = new ProtobufDeserializeOptions();

		private long ReadVarint()
		{
			long result = 0;
			int shift = 0;
			byte b;
			do {
				b = Reader.ReadByte();
				result |= (long)(b & 0x7f) << shift;
				shift += 7;
			} while ((b & 0x80) != 0);
			return result;
		}

		public override void FromReader(object obj)
		{
			int count = 1;
			foreach (var f in obj.GetType().GetFields()) {
				var t = f.FieldType;
				if (t == typeof(int)) {
					if (ReadVarint() != (count << 3) + (int)WireType.Varint)
						throw new YuzuException();
					f.SetValue(obj, (int)ReadVarint());
				}
				else if (t == typeof(string)) {
					if (ReadVarint() != (count << 3) + (int)WireType.LengthDelimited)
						throw new YuzuException();
					f.SetValue(obj, Encoding.UTF8.GetString(Reader.ReadBytes((int)ReadVarint())));
				}
				else {
					throw new NotImplementedException(t.Name);
				}
				++count;
			}
		}
	}
}
