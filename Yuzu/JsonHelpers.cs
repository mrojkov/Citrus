using System;
using System.IO;
using System.Text;

namespace Yuzu.Json
{
	internal static class JsonEscapeData
	{
		public static char[] unescapeChars = new char['t' + 1];
		public static char[] escapeChars = new char['\\' + 1];
		public static int[] hexDigits = new int['f' + 1];
		public static char[] digitHex = new char[16];

		// Optimization: array access is slightly faster than two or more sequential comparisons.
		static JsonEscapeData()
		{
			for (int i = 0; i < hexDigits.Length; ++i)
				hexDigits[i] = -1;
			for (int i = 0; i < 10; ++i) {
				hexDigits[i + '0'] = i;
				digitHex[i] = (char)(i + '0');
			}
			for (int i = 0; i < 6; ++i) {
				hexDigits[i + 'a'] = hexDigits[i + 'A'] = i + 10;
				digitHex[i + 10] = (char)(i + 'a');
			}
			unescapeChars['"'] = '"';
			unescapeChars['\\'] = '\\';
			unescapeChars['/'] = '/';
			unescapeChars['b'] = '\b';
			unescapeChars['f'] = '\f';
			unescapeChars['n'] = '\n';
			unescapeChars['r'] = '\r';
			unescapeChars['t'] = '\t';

			escapeChars['"'] = '"';
			escapeChars['\\'] = '\\';
			// Do not escape forward slash, see http://stackoverflow.com/questions/1580647/json-why-are-forward-slashes-escaped
			// escapeChars['/'] = '/';
			escapeChars['\b'] = 'b';
			escapeChars['\f'] = 'f';
			escapeChars['\n'] = 'n';
			escapeChars['\r'] = 'r';
			escapeChars['\t'] = 't';
		}
	}

	internal static class JsonIntWriter
	{
		private static byte[][] digitPairsZero = new byte[100][];
		private static byte[][] digitPairsNoZero = new byte[100][];
		private static byte[] minIntValueBytes = Encoding.ASCII.GetBytes(int.MinValue.ToString());
		private static byte[] minLongValueBytes = Encoding.ASCII.GetBytes(long.MinValue.ToString());

		static JsonIntWriter()
		{
			for (int i = 0; i < 10; ++i)
				for (int j = 0; j < 10; ++j)
					digitPairsZero[i * 10 + j] = new byte[] { (byte)((int)'0' + i), (byte)((int)'0' + j) };
			for (int j = 0; j < 10; ++j)
				digitPairsNoZero[j] = new byte[] { (byte)((int)'0' + j) };
			for (int i = 1; i < 10; ++i)
				for (int j = 0; j < 10; ++j)
					digitPairsNoZero[i * 10 + j] = new byte[] { (byte)((int)'0' + i), (byte)((int)'0' + j) };
		}

		private static void WriteUIntInternal(BinaryWriter writer, uint x)
		{
			uint d;
			if (x < 10000) {
				d = x / 100;
				writer.Write(digitPairsNoZero[d]);
				writer.Write(digitPairsZero[x - d * 100]);
				return;
			}
			if (x < 1000000) {
				d = x / 10000;
				writer.Write(digitPairsNoZero[d]);
				x -= d * 10000;
				d = x / 100;
				writer.Write(digitPairsZero[d]);
				writer.Write(digitPairsZero[x - d * 100]);
				return;
			}
			if (x < 100000000) {
				d = x / 1000000;
				writer.Write(digitPairsNoZero[d]);
				x -= d * 1000000;
				d = x / 10000;
				writer.Write(digitPairsZero[d]);
				x -= d * 10000;
				d = x / 100;
				writer.Write(digitPairsZero[d]);
				writer.Write(digitPairsZero[x - d * 100]);
				return;
			}
			d = x / 100000000;
			writer.Write(digitPairsNoZero[d]);
			x -= d * 100000000;
			d = x / 1000000;
			writer.Write(digitPairsZero[d]);
			x -= d * 1000000;
			d = x / 10000;
			writer.Write(digitPairsZero[d]);
			x -= d * 10000;
			d = x / 100;
			writer.Write(digitPairsZero[d]);
			writer.Write(digitPairsZero[x - d * 100]);
		}

		public static void WriteInt(BinaryWriter writer, object obj)
		{
			var x = Convert.ToInt32(obj);
			if (x == int.MinValue) {
				writer.Write(minIntValueBytes);
				return;
			}
			if (x < 0) {
				writer.Write((byte)'-');
				x = -x;
			}
			if (x < 100) {
				writer.Write(digitPairsNoZero[x]);
				return;
			}
			unchecked { WriteUIntInternal(writer, (uint)x); }
		}

		public static void WriteUInt(BinaryWriter writer, object obj)
		{
			var x = Convert.ToUInt32(obj);
			if (x < 100) {
				writer.Write(digitPairsNoZero[x]);
				return;
			}
			WriteUIntInternal(writer, x);
		}

		public static void WriteLong(BinaryWriter writer, object obj)
		{
			var x = Convert.ToInt64(obj);
			if (x == long.MinValue) {
				writer.Write(minLongValueBytes);
				return;
			}
			if (x < 0) {
				writer.Write((byte)'-');
				x = -x;
			}
			if (x < 100) {
				writer.Write(digitPairsNoZero[x]);
				return;
			}
			if (x < int.MaxValue) {
				unchecked { WriteUIntInternal(writer, (uint)x); }
				return;
			}
			// TODO: Optimize long case.
			writer.Write(Encoding.ASCII.GetBytes(x.ToString()));
		}

		public static void WriteULong(BinaryWriter writer, object obj)
		{
			var x = Convert.ToUInt64(obj);
			if (x < 100) {
				writer.Write(digitPairsNoZero[x]);
				return;
			}
			if (x < int.MaxValue) {
				unchecked { WriteUIntInternal(writer, (uint)x); }
				return;
			}
			// TODO: Optimize long case.
			writer.Write(Encoding.ASCII.GetBytes(x.ToString()));
		}

		public static void WriteInt2Digits(BinaryWriter writer, int x)
		{
			writer.Write(digitPairsZero[x]);
		}

		public static void WriteInt4Digits(BinaryWriter writer, int x)
		{
			var d = x / 100;
			writer.Write(digitPairsZero[d]);
			writer.Write(digitPairsZero[x - d * 100]);
		}

		public static void WriteInt7Digits(BinaryWriter writer, int x)
		{
			var d = x / 1000000;
			writer.Write(digitPairsNoZero[d]);
			x -= d * 1000000;
			d = x / 10000;
			writer.Write(digitPairsZero[d]);
			x -= d * 10000;
			d = x / 100;
			writer.Write(digitPairsZero[d]);
			writer.Write(digitPairsZero[x - d * 100]);
		}
	}

}
