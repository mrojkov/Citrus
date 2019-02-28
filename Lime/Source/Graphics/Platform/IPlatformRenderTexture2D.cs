using System;

namespace Lime.Graphics.Platform
{
	public interface IPlatformRenderTexture2D : IPlatformTexture2D
	{
		void ReadPixels(Format format, int x, int y, int width, int height, IntPtr pixels);
	}
}
