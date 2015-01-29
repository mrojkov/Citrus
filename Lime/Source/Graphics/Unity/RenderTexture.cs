#if UNITY
using System;
using System.Diagnostics;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using OGL = MonoMac.OpenGL.GL;
#elif OPENGL
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class RenderTexture : ITexture, IDisposable
	{
		int framebuffer;
		uint id;
		Size size = new Size(0, 0);
		Rectangle uvRect;

		public RenderTexture(int width, int height)
		{
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
		
		public Rectangle UVRect {
			get { return uvRect; }
		}

		public UnityEngine.Texture GetUnityTexture()
		{
			throw new NotImplementedException();
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
			Renderer.FlushSpriteBatch();
		}

		public void RestoreRenderTarget()
		{
			Renderer.FlushSpriteBatch();
		}

		public bool IsTransparentPixel(int x, int y)
		{
			return false;
		}
	}
}
#endif