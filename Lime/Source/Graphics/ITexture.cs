using System;
using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	[ProtoInclude(100, typeof(SerializableTexture))]
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
		[ProtoMember(1)]
		// Using YuzuOptional+YuzuDefault and not YuzuMember because there's
		// no telling which implementation will set default value to what
		// (some of implementations are throwing "Not Implemented" exceptions).
		// And Yuzu deserializer generator will try to instantiate interface
		// for this case when using YuzuMember.
		// TODO: consider moving SerializationPath from ITexture to SerializableTexture which sounds only logical
		[YuzuOptional]
		[YuzuDefault("")]
		string SerializationPath { get; set; }
		int MemoryUsed { get; }
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
	}

	public class CommonTexture : IDisposable
	{
		public static int TotalMemoryUsed { get; private set; }
		public static int TotalMemoryUsedMb { get { return TotalMemoryUsed / (1024 * 1024); } }

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

		public virtual void MaybeDiscardUnderPressure() { }

		public virtual void Dispose()
		{
			MemoryUsed = 0;
		}
	}
}