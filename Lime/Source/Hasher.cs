using System;
using System.Runtime.CompilerServices;

namespace Lime
{
	public unsafe struct Hasher
	{
		private const long FnvPrime = unchecked((long)1099511628211);
		private const long FnvOffset = unchecked((long)14695981039346656037);

		private long hash;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Begin()
		{
			hash = FnvOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(string data)
		{
			fixed (char* p = data) {
				WriteInternal((byte*)p, sizeof(char) * data.Length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(T[] data) where T : unmanaged
		{
			Write(data, 0, data.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(T[] data, int index, int count) where T : unmanaged
		{
			fixed (T* p = &data[index]) {
				WriteInternal((byte*)p, sizeof(T) * count);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(ref T data) where T : unmanaged
		{
			fixed (T* p = &data) {
				WriteInternal((byte*)p, sizeof(T));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(T data) where T : unmanaged
		{
			WriteInternal((byte*)&data, sizeof(T));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(IntPtr data, int count)
		{
			WriteInternal((byte*)data, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteInternal(byte* data, int count)
		{
			unchecked {
				while (count-- > 0) {
					hash = (hash ^ (*data++)) * FnvPrime;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long End() => hash;
	}
}
