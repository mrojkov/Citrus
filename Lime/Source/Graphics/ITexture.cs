using System;
using ProtoBuf;
using System.Collections.Generic;

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
		void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1);
		uint GetHandle();
		void SetAsRenderTarget();
		void RestoreRenderTarget();
		bool IsTransparentPixel(int x, int y);
		bool IsStubTexture { get; }
		[ProtoMember(1)]
		string SerializationPath { get; set; }
		int MemoryUsed { get; }
#if UNITY
		UnityEngine.Texture GetUnityTexture();
#endif
	}

	public class CommonTexture : IDisposable
	{
		public static int TotalMemoryUsed { get; private set; }
		public static int TotalMemoryUsedMb { get { return TotalMemoryUsed / (1024 * 1024); } }

		private int memoryUsed;
		public int MemoryUsed 
		{
			get { return memoryUsed; }
			set
			{
				TotalMemoryUsed += value - memoryUsed;
				memoryUsed = value;
			}
		}

		public virtual void Dispose()
		{
			MemoryUsed = 0;
		}
	}
}