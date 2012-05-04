using System;
using ProtoBuf;

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
	[ProtoContract]
	[ProtoInclude(100, typeof(SerializableTexture))]
	public interface ITexture : IDisposable
	{
		Size ImageSize { get; }
		Size SurfaceSize { get; }
		Rectangle UVRect { get; }
		uint GetHandle();
		void SetAsRenderTarget();
		void RestoreRenderTarget();
		bool IsTransparentPixel(int x, int y);
		[ProtoMember(1)]
		string SerializationPath { get; set; }
	}
}
