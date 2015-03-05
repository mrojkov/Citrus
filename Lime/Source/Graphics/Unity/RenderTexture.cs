#if UNITY
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Lime
{
	public class RenderTexture : ITexture, IDisposable
	{
		int framebuffer;
		uint id;
		Size size = new Size(0, 0);
		Rectangle uvRect;
		UnityEngine.RenderTexture unityTexture;

		private static readonly Stack<UnityEngine.RenderTexture> textureStack = new Stack<UnityEngine.RenderTexture>();

		public RenderTexture(int width, int height)
		{
			unityTexture = new UnityEngine.RenderTexture(width, height, 0);
			unityTexture.Create();
			size.Width = width;
			size.Height = height;
			uvRect = new Rectangle(0, 0, 1, 1);
		}

		public Size ImageSize {
			get { return size; }
		}

		public Size SurfaceSize {
			get { return size; }
		}		
		
		public Rectangle AtlasUVRect {
			get { return uvRect; }
		}

		public UnityEngine.Texture GetUnityTexture()
		{
			return unityTexture;
		}
		
		public void Dispose()
		{
		}

		~RenderTexture()
		{
			Dispose();
		}

		public uint GetHandle()
		{
			return id;
		}

		public bool IsStubTexture { get { return false; } }

		public string SerializationPath {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public void SetAsRenderTarget()
		{
			Renderer.Flush();
			var currentTexture = UnityEngine.RenderTexture.active;
			textureStack.Push(currentTexture);
			UnityEngine.RenderTexture.active = unityTexture;
		}

		public void RestoreRenderTarget()
		{
			Renderer.Flush();
			var prevTexture = textureStack.Pop();
			UnityEngine.RenderTexture.active = prevTexture;
		}

		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}

		public ITexture AlphaTexture { get; private set; }
		
		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1) { }

		public void Discard() {}
		public void MaybeDiscardUnderPressure() {}
		
		public int MemoryUsed { get { return 0; } }
	}
}
#endif