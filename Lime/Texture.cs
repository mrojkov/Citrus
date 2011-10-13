namespace Lime
{
    /// <summary>
    /// Enumerates available bitmap formats
    /// </summary>
    public enum BitmapFormat
    {
        /// <summary>
        /// 32 bits, RGB + Alpha (Mac, Win32, Wii, iPhone)
        /// </summary>
        RGBA8,
        /// <summary>
        /// 16 bits, RGB + Alpha (Mac, Win32, Wii, iPhone)
        /// </summary>
        RGBA4,
        /// <summary>
        /// 24 bits, RGB (Mac, Win32, iPhone)
        /// </summary>
        RGB8,
        /// <summary>
        /// 16 bits, RGB only (Mac, Win32, Wii, iPhone)
        /// </summary>
        R5G6B5,
        /// <summary>
        /// ~4 bits, Alpha + RGB (compressed DXT1, Win32, Mac, Wii)
        /// </summary>
        DXT1,
        /// <summary>
        /// ~8 bits, Alpha + RGB (compressed DXT3, Win32, Mac)
        /// </summary>
        DXT3,
        /// <summary>
        /// ~8 bits, Alpha + RGB (compressed DXT5, Win32, Mac)
        /// </summary>
        DXT5,
        /// <summary>
        /// 2 bits, RGB (PVR texture compression, iPhone only)
        /// </summary>
        PVRTC2_RGB,
        /// <summary>
        /// 2 bits, RGB + Alpha (PVR texture compression, iPhone only)
        /// </summary>
        PVRTC2_RGBA,
        /// <summary>
        /// 4 bits, RGB (PVR texture compression, iPhone only)
        /// </summary>
        PVRTC4_RGB,
        /// <summary>
        /// 4 bits, RGB + Alpha (PVR texture compression, iPhone only)
        /// </summary>
        PVRTC4_RGBA
    }

    /// <summary>
    /// Base class for texture handling.
    /// Contains functionality that is common to both PlainTexture and RenderTexture classes.
    /// </summary>
    public interface ITexture
    {
        /// <summary>
        /// Size of texture image.
        /// </summary>
        Size ImageSize { get; }
		
		/// <summary>
        /// Size of texture surface.
        /// </summary>		
		Size SurfaceSize { get; }
		
		/// <summary>
		/// UV coordinates of image rectangle in the texture
		/// </summary>
		Rectangle UVRect { get; }

        /// <summary>
        /// Returns native texture handle.
        /// </summary>
        /// <returns></returns>
        uint GetHandle();

        /// <summary>
        /// Sets texture as a render target.
        /// </summary>
        void SetAsRenderTarget();

        /// <summary>
        /// Restores default render target (backbuffer).
        /// </summary>
        void RestoreRenderTarget();

        /// <summary>
        /// Checks pixel transparency at given coordinates.
        /// </summary>
        /// <param name="x">x-coordinate of pixel</param>
        /// <param name="y">y-coordinate of pixel</param>
        /// <returns></returns>
        bool IsTransparentPixel(int x, int y);
    }
}
