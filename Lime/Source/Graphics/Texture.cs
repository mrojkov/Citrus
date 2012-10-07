using System;
using ProtoBuf;

namespace Lime
{
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
		bool ChessTexture { get; }
		[ProtoMember(1)]
		string SerializationPath { get; set; }
	}
}
