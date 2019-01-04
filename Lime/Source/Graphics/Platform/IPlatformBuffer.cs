using System;

namespace Lime.Graphics.Platform
{
	public interface IPlatformBuffer : IDisposable
	{
		BufferType BufferType { get; }
		int Size { get; }
		bool Dynamic { get; }

		void SetData(int offset, IntPtr data, int size, BufferSetDataMode mode);
	}

	public enum BufferType
	{
		Vertex,
		Index
	}

	public enum BufferSetDataMode
	{
		Default,
		Discard
	}

	public static unsafe class PlatformBufferExtensions
	{
		public static void SetData<T>(
			this IPlatformBuffer buffer, int offset, ref T data, BufferSetDataMode mode) where T : unmanaged
		{
			fixed (T* p = &data) {
				buffer.SetData(offset, new IntPtr(p), sizeof(T), mode);
			}
		}

		public static void SetData<T>(
			this IPlatformBuffer buffer, int offset, T[] data, BufferSetDataMode mode) where T : unmanaged
		{
			SetData(buffer, offset, data, 0, data.Length, mode);
		}

		public static void SetData<T>(
			this IPlatformBuffer buffer, int offset, T[] data, int startIndex, int count, BufferSetDataMode mode) where T : unmanaged
		{
			fixed (T* p = &data[startIndex]) {
				buffer.SetData(offset, new IntPtr(p), count * sizeof(T), mode);
			}
		}
	}
}
