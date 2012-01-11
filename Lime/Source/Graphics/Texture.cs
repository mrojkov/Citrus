namespace Lime
{
	public enum BitmapFormat
	{
		RGBA8,
		RGBA4,
		RGB8,
		R5G6B5,
		DXT1,
		DXT3,
		DXT5,
		PVRTC2_RGB,
		PVRTC2_RGBA,
		PVRTC4_RGB,
		PVRTC4_RGBA
	}

	/// <summary>
	/// Base class for texture handling.
	/// Contains functionality that is common to both PlainTexture and RenderTexture classes.
	/// </summary>
	public interface ITexture
	{
		Size ImageSize { get; }
		Size SurfaceSize { get; }
		Rectangle UVRect { get; }
		uint GetHandle();
		void SetAsRenderTarget();
		void RestoreRenderTarget();
		bool IsTransparentPixel(int x, int y);
	}
}
