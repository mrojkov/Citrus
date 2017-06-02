using System;
using Yuzu;

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

	internal static class TextureParamsExtensions
	{
		public static int ToInt(this TextureFilter filter)
		{
			switch (filter) {
				case TextureFilter.Linear:
					return (int)OpenTK.Graphics.ES20.All.Linear;
				case TextureFilter.Nearest:
					return (int)OpenTK.Graphics.ES20.All.Nearest;
				default:
					throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
			}
		}

		public static int ToInt(this TextureWrapMode wrapMode)
		{
			switch (wrapMode) {
				case TextureWrapMode.Clamp:
					return (int)OpenTK.Graphics.ES20.All.ClampToEdge;
				case TextureWrapMode.Repeat:
					return (int)OpenTK.Graphics.ES20.All.Repeat;
				case TextureWrapMode.MirroredRepeat:
					return (int)OpenTK.Graphics.ES20.All.MirroredRepeat;
				default:
					throw new ArgumentOutOfRangeException(nameof(wrapMode), wrapMode, null);
			}
		}
	}

	public interface ITexture : IDisposable
	{
		Size ImageSize { get; }
		Size SurfaceSize { get; }
		Rectangle AtlasUVRect { get; }

		/// <summary>
		/// Used on Android for ETC1 compressed textures
		/// </summary>
		ITexture AlphaTexture { get; }

		void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv);
		uint GetHandle();
		void SetAsRenderTarget();
		void RestoreRenderTarget();
		bool IsTransparentPixel(int x, int y);
		bool IsStubTexture { get; }
		// TODO: consider moving SerializationPath from ITexture to SerializableTexture which sounds only logical
		[YuzuMember]
		string SerializationPath { get; set; }

		int MemoryUsed { get; }
		TextureWrapMode WrapModeU { get; set; }
		TextureWrapMode WrapModeV { get; set; }
		TextureFilter MinFilter { get; set; }
		TextureFilter MagFilter { get; set; }
#if UNITY
		UnityEngine.Texture GetUnityTexture();
#endif

		/// <summary>
		/// Unloads the texture from memory, frees OpenGL resources.
		/// The texture will be loaded again on the next GetHandle().
		/// RenderTexture.Unload() does nothing.
		/// </summary>
		void Discard();

		void MaybeDiscardUnderPressure();
		Color4[] GetPixels();
	}

	public class CommonTexture : IDisposable
	{
		public static int TotalMemoryUsed { get; private set; }

		public static int TotalMemoryUsedMb
		{
			get { return TotalMemoryUsed / (1024 * 1024); }
		}

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
		}
	}
}