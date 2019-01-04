using System;

namespace Lime.Graphics.Platform
{
	public interface IPlatformTexture2D : IDisposable
	{
		Format Format { get; }
		int Width { get; }
		int Height { get; }
		int LevelCount { get; }
		TextureParams TextureParams { get; set; }

		void SetData(int level, int x, int y, int width, int height, IntPtr data);
	}

	public unsafe static class PlatformTexture2DExtensions
	{
		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, T data) where T : unmanaged
		{
			SetData(texture, level, new IntPtr(&data));
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, ref T data) where T : unmanaged
		{
			GraphicsUtility.CalculateMipLevelSize(level, texture.Width, texture.Height, out var levelWidth, out var levelHeight);
			SetData(texture, level, 0, 0, levelWidth, levelHeight, ref data);
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, T[] data) where T : unmanaged
		{
			SetData(texture, level, data, 0);
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, T[] data, int startIndex) where T : unmanaged
		{
			GraphicsUtility.CalculateMipLevelSize(level, texture.Width, texture.Height, out var levelWidth, out var levelHeight);
			SetData(texture, level, 0, 0, levelWidth, levelHeight, data, startIndex);
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, int x, int y, int width, int height, T data) where T : unmanaged
		{
			texture.SetData(level, x, y, width, height, new IntPtr(&data));
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, int x, int y, int width, int height, ref T data) where T : unmanaged
		{
			fixed (T* p = &data) {
				texture.SetData(level, x, y, width, height, new IntPtr(p));
			}
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, int x, int y, int width, int height, T[] data) where T : unmanaged
		{
			SetData(texture, level, x, y, width, height, data, 0);
		}

		public static void SetData<T>(
			this IPlatformTexture2D texture, int level, int x, int y, int width, int height, T[] data, int startIndex) where T : unmanaged
		{
			fixed (T* p = &data[startIndex]) {
				texture.SetData(level, x, y, width, height, new IntPtr(p));
			}
		}

		public static void SetData(this IPlatformTexture2D texture, int level, IntPtr data)
		{
			GraphicsUtility.CalculateMipLevelSize(level, texture.Width, texture.Height, out var levelWidth, out var levelHeight);
			texture.SetData(level, 0, 0, levelWidth, levelHeight, data);
		}
	}
}
