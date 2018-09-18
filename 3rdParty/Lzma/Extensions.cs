using System;
using System.IO;

namespace Lzma
{
	static class Extensions
	{
		public static string GetName(this LzmaMatchFinder matchFinder)
		{
			switch (matchFinder) {
				case LzmaMatchFinder.BT2:
					return "BT2";
				case LzmaMatchFinder.BT4:
					return "BT4";
				default:
					throw new ArgumentException("Invalid match finder");
			}
		}

		public static void WriteInt64(this Stream stream, long value)
		{
			for (var shift = 0; shift < 64; shift += 8)
				stream.WriteByte((byte)(value >> shift));
		}

		public static bool TryReadInt64(this Stream stream, out long value)
		{
			value = 0L;
			for (var shift = 0; shift < 64; shift += 8) {
				var b = stream.ReadByte();
				if (b == -1)
					return false;
				value |= (long)b << shift;
			}
			return true;
		}
	}
}