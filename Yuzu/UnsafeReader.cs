using System;
using System.IO;
using System.Text;

namespace Yuzu.Unsafe
{
	// Use with great caution. Gives 10% speedup in optimized builds only.
	unsafe public class UnsafeBinaryReader : BinaryReader
	{

		private byte[] buf;
		private int pos;
		private Decoder decoder = Encoding.UTF8.GetDecoder();

		public UnsafeBinaryReader(Stream input): base(input) {
			if (!(input is MemoryStream))
				throw new NotImplementedException();
			buf = (input as MemoryStream).GetBuffer();
			pos = (int)input.Position;
		}

		public override void Close()
		{
			BaseStream.Position = pos;
		}

		protected override void Dispose(bool disposing)
		{
			buf = null;
			base.Dispose(disposing);
		}

		protected override void FillBuffer(int numBytes) { }

		public override int PeekChar() { return (int)buf[pos]; }
		public override int Read() { return (int)buf[pos++]; }

		public override int Read(byte[] buffer, int index, int count)
		{
			Array.Copy(buf, pos, buffer, index, count);
			pos += count;
			return count;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			throw new NotImplementedException();
		}

		private int ReadVarint()
		{
			int result = 0;
			int shift = 0;
			byte b;
			unchecked {
				do {
					b = buf[pos++];
					result |= (int)(b & 0x7f) << shift;
					shift += 7;
				} while ((b & 0x80) != 0);
			}
			return result;
		}

		public override bool ReadBoolean() { unchecked { return buf[pos++] != 0; } }

		public override byte ReadByte() { unchecked { return buf[pos++]; } }

		public override byte[] ReadBytes(int count)
		{
			var result = new byte[count];
			Array.Copy(buf, pos, result, 0, count);
			pos += count;
			return result;
		}

		public override char ReadChar() {
			char result;
			fixed (byte* b = buf) {
				int bytesUsed, charsUsed;
				bool completed;
				decoder.Convert(
					b + pos, buf.Length - pos, &result, 1, true, out bytesUsed, out charsUsed, out completed);
				pos += bytesUsed;
			}
			return result;
		}

		public override char[] ReadChars(int count)
		{
			var result = new char[count];
			fixed (byte* b = buf)
			fixed (char* r = result) {
				int bytesUsed, charsUsed;
				bool completed;
				decoder.Convert(
					b + pos, buf.Length - pos, r, count, true, out bytesUsed, out charsUsed, out completed);
				pos += bytesUsed;
			}
			return result;
		}

		public override decimal ReadDecimal() { throw new NotImplementedException(); }

		public override double ReadDouble()
		{
			double result;
			fixed (byte *b = buf)
				result = *(double*)(b + pos);
			pos += 8;
			return result;
		}

		public override short ReadInt16()
		{
			short result;
			fixed (byte* b = buf)
				result = *(short*)(b + pos);
			pos += 2;
			return result;
		}

		public override int ReadInt32()
		{
			int result;
			fixed (byte* b = buf)
				result = *(int*)(b + pos);
			pos += 4;
			return result;
		}

		public override long ReadInt64()
		{
			long result;
			fixed (byte* b = buf)
				result = *(long*)(b + pos);
			pos += 8;
			return result;
		}

		public override sbyte ReadSByte()
		{
			fixed (byte* b = buf)
				return *(sbyte*)(b + pos++);
		}

		public override float ReadSingle()
		{
			float result;
			fixed (byte *b = buf)
				result = *(float*)(b + pos);
			pos += 4;
			return result;
		}

		public override string ReadString()
		{
			var length = ReadVarint();
			var result = Encoding.UTF8.GetString(buf, pos, length);
			pos += length;
			return result;
		}

		public override ushort ReadUInt16()
		{
			ushort result;
			fixed (byte* b = buf)
				result = *(ushort*)(b + pos);
			pos += 2;
			return result;
		}

		public override uint ReadUInt32()
		{
			uint result;
			fixed (byte* b = buf)
				result = *(uint*)(b + pos);
			pos += 4;
			return result;
		}

		public override ulong ReadUInt64()
		{
			ulong result;
			fixed (byte* b = buf)
				result = *(ulong*)(b + pos);
			pos += 8;
			return result;
		}
	}
}