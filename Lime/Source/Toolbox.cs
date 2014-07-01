using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lime
{
	public static class Toolbox
	{
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		public static int ComputeHash(byte[] data, int length)
		{
			unchecked {
				const int p = 16777619;
				int hash = (int)2166136261;
				for (int i = 0; i < length; i++) {
					hash = (hash ^ data[i]) * p;
				}
				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				return hash;
			}
		}

		public static void CopyStream(Stream input, Stream output)
		{
			var bufferSize = 32768;
			byte[] buffer = new byte[bufferSize];
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
				output.Write(buffer, 0, read);
			}
		}

	}
}