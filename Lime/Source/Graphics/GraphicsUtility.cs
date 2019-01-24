using System;
using System.Runtime.InteropServices;

namespace Lime
{
	public static class GraphicsUtility
	{
		public static ulong CombineAlignment(ulong alignment1, ulong alignment2)
		{
			if (alignment1 % alignment2 == 0) {
				return alignment1;
			} else if (alignment2 % alignment1 == 0) {
				return alignment2;
			} else {
				return alignment1 * alignment2;
			}
		}

		public static ulong AlignDown(ulong value, ulong alignment)
		{
			return (value / alignment) * alignment;
		}

		public static ulong AlignUp(ulong value, ulong alignment)
		{
			return ((value + alignment - 1) / alignment) * alignment;
		}

#if iOS
		const string stdlib = "__Internal";
#else
		const string stdlib = "msvcrt";
#endif

		[DllImport(stdlib, EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		private static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

		public static unsafe void CopyMemory(IntPtr dst, IntPtr src, int count)
		{
			memcpy(dst, src, new UIntPtr((uint)count));
		}

		[DllImport(stdlib, EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		private static extern IntPtr memset(IntPtr dest, int c, UIntPtr count);

		public static unsafe void FillMemory(IntPtr dst, int c, int count)
		{
			memset(dst, c, new UIntPtr((uint)count));
		}

		public static int CalculateMipLevelDataSize(int level, Format format, int width, int height)
		{
			CalculateMipLevelSize(level, width, height, out var levelWidth, out var levelHeight);
			return CalculateImageDataSize(format, levelWidth, levelHeight);
		}

		public static int CalculateImageDataSize(Format format, int width, int height)
		{
			format.GetBlockSize(out var blockWidth, out var blockHeight);
			var wBlocks = (width + blockWidth - 1) / blockWidth;
			var hBlocks = (height + blockHeight - 1) / blockHeight;
			return wBlocks * hBlocks * format.GetSize();
		}

		public static void CalculateMipLevelSize(int level, int width, int height, out int levelWidth, out int levelHeight)
		{
			levelWidth = width;
			levelHeight = height;
			while (level > 0) {
				levelWidth /= 2;
				levelHeight /= 2;
				level--;
			}
			levelWidth = Math.Max(levelWidth, 1);
			levelHeight = Math.Max(levelHeight, 1);
		}

		public static int CalculateMipLevelCount(int width, int height)
		{
			var size = Math.Max(width, height);
			var levels = 1;
			while (size > 1) {
				levels++;
				size /= 2;
			}
			return levels;
		}
	}
}
