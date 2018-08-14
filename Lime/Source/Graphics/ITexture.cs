using System;
using Yuzu;
#if OPENGL
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#elif MAC
using OpenTK.Graphics.OpenGL;
#elif MONOMAC
using MonoMac.OpenGL;
#endif

namespace Lime
{
	// TODO: add mipmap related filters when fixing mipmaps
	public enum TextureFilter
	{
		Linear,
		Nearest,
	}

	public enum TextureWrapMode
	{
		Clamp,
		Repeat,
		MirroredRepeat,
	}

	public class TextureParams : IEquatable<TextureParams>
	{
		[YuzuMember]
		[TangerineKeyframeColor(5)]
		public TextureWrapMode WrapModeU { get; set; } = TextureWrapMode.Clamp;
		[YuzuMember]
		[TangerineKeyframeColor(6)]
		public TextureWrapMode WrapModeV { get; set; } = TextureWrapMode.Clamp;
		[YuzuMember]
		public TextureFilter MinFilter { get; set; } = TextureFilter.Linear;
		[YuzuMember]
		public TextureFilter MagFilter { get; set; } = TextureFilter.Linear;

		public TextureWrapMode WrapMode
		{
			set
			{
				WrapModeU = WrapModeV = value;
			}
		}

		public TextureFilter MinMagFilter
		{
			set
			{
				MinFilter = MagFilter = value;
			}
		}

		public static TextureParams Default = new TextureParams();

		public bool Equals(TextureParams other)
		{
			return WrapModeU == other.WrapModeU &&
				WrapModeV == other.WrapModeV &&
				MinFilter == other.MinFilter &&
				MagFilter == other.MagFilter;
		}
	}

	internal static class TextureParamsExtensions
	{
		public static int ToInt(this TextureFilter filter)
		{
			switch (filter) {
			case TextureFilter.Linear:
				return (int)All.Linear;
			case TextureFilter.Nearest:
				return (int)All.Nearest;
			default:
				throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
			}
		}

		public static int ToInt(this TextureWrapMode wrapMode)
		{
			switch (wrapMode) {
			case TextureWrapMode.Clamp:
				return (int)All.ClampToEdge;
			case TextureWrapMode.Repeat:
				return (int)All.Repeat;
			case TextureWrapMode.MirroredRepeat:
				return (int)All.MirroredRepeat;
			default:
				throw new ArgumentOutOfRangeException(nameof(wrapMode), wrapMode, null);
			}
		}
	}
#endif

	[YuzuDontGenerateDeserializer]
	public interface ITexture : IDisposable
	{
		Size ImageSize { get; }
		Size SurfaceSize { get; }
		Rectangle AtlasUVRect { get; }
		ITexture AtlasTexture { get; }
		bool IsDisposed { get; }

		void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv);
		uint GetHandle();
		void SetAsRenderTarget();
		void RestoreRenderTarget();
		bool IsTransparentPixel(int x, int y);
		bool IsStubTexture { get; }
		int MemoryUsed { get; }
		TextureParams TextureParams { get; set; }

		/// <summary>
		/// Unloads the texture from memory, frees OpenGL resources.
		/// The texture will be loaded again on the next GetHandle().
		/// RenderTexture.Unload() does nothing.
		/// </summary>
		void Discard();

		void MaybeDiscardUnderPressure();
		Color4[] GetPixels();
	}

	public class CommonTexture
	{
		public static int TotalMemoryUsed { get; private set; }

		public static int TotalMemoryUsedMb
		{
			get { return TotalMemoryUsed / (1024 * 1024); }
		}

		public bool IsDisposed { get; private set; }

		private int memoryUsed;

		public int MemoryUsed
		{
			get { return memoryUsed; }
			protected set
			{
				TotalMemoryUsed += value - memoryUsed;
				memoryUsed = value;
			}
		}

		public virtual void MaybeDiscardUnderPressure()
		{
		}

		public virtual void Dispose()
		{
			MemoryUsed = 0;
			IsDisposed = true;
		}
	}
}
