#if WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using System.Reflection;

namespace Lime
{
	public static partial class OGL
	{
		const string library = "opengl32.dll";

		delegate void ClientActiveTextureHandler(TextureUnit texture);
		delegate void ActiveTextureHandler(TextureUnit texture);
		delegate void GenerateMipmapHandler(GenerateMipmapTarget target);
		delegate void CompressedTexImage2DHandler(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, int imageSize, IntPtr data);

		static ClientActiveTextureHandler clientActiveTexture = GetProc<ClientActiveTextureHandler>("glClientActiveTexture");
		static ActiveTextureHandler activeTexture = GetProc<ActiveTextureHandler>("glActiveTexture");
		static CompressedTexImage2DHandler compressedTexImage2D = GetProc<CompressedTexImage2DHandler>("glCompressedTexImage2D");
		static GenerateMipmapHandler generateMipmap = GetProc<GenerateMipmapHandler>("glGenerateMipmap");

		[DllImport(library, EntryPoint = "wglGetProcAddress")]
		static extern IntPtr GetProcAddress(string name);

		static T GetProc<T>(string name) where T: class
		{
			var ptr = GetProcAddress(name);
			if (ptr == new IntPtr(0)) {
				throw new Lime.Exception("Unsupported OpenGL procedure: {0}", name);
			}
			return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
		}

		[DllImport(library, EntryPoint = "glViewport")]
		public static extern void Viewport(int x, int y, int width, int height);

		[DllImport(library, EntryPoint = "glEnable")]
		public static extern void Enable(EnableCap cap);

		[DllImport(library, EntryPoint="glDisable")]
		public static extern void Disable(EnableCap cap);

		[DllImport(library, EntryPoint="glBindTexture")]
		public static extern void BindTexture(TextureTarget target, UInt32 texture);

		[DllImport(library, EntryPoint = "glVertexPointer")]
		public static extern void VertexPointer(Int32 size, VertexPointerType type, Int32 stride, IntPtr pointer);

		[DllImport(library, EntryPoint = "glEnableClientState")]
		public static extern void EnableClientState(ArrayCap array);

		[DllImport(library, EntryPoint = "glColorPointer")]
		public static extern void ColorPointer(Int32 size, ColorPointerType type, Int32 stride, IntPtr pointer);

		[DllImport(library, EntryPoint = "glTexCoordPointer")]
		public static extern void TexCoordPointer(int size, TexCoordPointerType type, int stride, IntPtr pointer);

		[DllImport(library, EntryPoint = "glMatrixMode")]
		public static extern void MatrixMode(MatrixMode mode);

		[DllImport(library, EntryPoint = "glPushMatrix")]
		public static extern void PushMatrix();

		[DllImport(library, EntryPoint = "glPopMatrix")]
		public static extern void PopMatrix();

		[DllImport(library, EntryPoint = "glLoadIdentity")]
		public static extern void LoadIdentity();

		[DllImport(library, EntryPoint = "glOrtho")]
		public static extern void Ortho(double left, double right, double bottom, double top, double zNear, double zFar);

		[DllImport(library, EntryPoint = "glTexEnvf")]
		public static extern void TexEnv(TextureEnvTarget target, TextureEnvParameter pname, Single param);

		[DllImport(library, EntryPoint = "glBlendFunc")]
		public static extern void BlendFunc(BlendingFactorSrc src, BlendingFactorDest dst);

		[DllImport(library, EntryPoint = "glClearColor")]
		public static extern void ClearColor(float r, float g, float b, float a);

		[DllImport(library, EntryPoint = "glClear")]
		public static extern void Clear(ClearBufferMask mask);

		[DllImport(library, EntryPoint = "glDrawElements")]
		public static extern void DrawElements(BeginMode mode, int count, DrawElementsType type, IntPtr indices);

		[DllImport(library, EntryPoint = "glGenTextures")]
		public static extern void GenTextures(Int32 n, UIntPtr textures);

		[DllImport(library, EntryPoint = "glDeleteTextures")]
		public static extern void DeleteTextures(Int32 n, UIntPtr textures);

		[DllImport(library, EntryPoint = "glFinish")]
		public static extern void Finish();

		public static uint GenTexture()
		{
			var t = new UInt32[1];
			unsafe {
				fixed (UInt32* p = &t[0]) {
					GenTextures(1, (UIntPtr)p);
				}
			}
			return t[0];
		}

		public static void DeleteTextures(int n, uint[] textures)
		{
			unsafe {
				fixed (UInt32* p = &textures[0]) {
					DeleteTextures(n, (UIntPtr)p);
				}
			}
		}

		[DllImport(library, EntryPoint = "glTexParameteri")]
		public static extern void TexParameter(TextureTarget target, TextureParameterName name, int value);
		
		[DllImport(library, EntryPoint = "glHint")]
		public static extern void Hint(HintTarget target, HintMode mode);

		public static void GenerateMipmap(GenerateMipmapTarget target)
		{
			generateMipmap(target);
		}

		public static void CompressedTexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, int imageSize, IntPtr data)
		{
			compressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data);
		}

		public static void CompressedTexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, int imageSize, byte[] data)
		{
			unsafe {
				fixed (byte* p = &data[0]) {
					CompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, (IntPtr)p);
				}
			}
		}

		[DllImport(library, EntryPoint = "glTexImage2D")]
		public static extern void TexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr pixels);

		public static void TexImage2D<T>(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, PixelFormat format, PixelType type, T[] pixels) where T: struct
		{
			GCHandle ptr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			try {
				TexImage2D(target, level, internalformat, width, height, border, format, type, (IntPtr)ptr.AddrOfPinnedObject());
			} finally {
				ptr.Free();
			}
		}

		public static void ClientActiveTexture(TextureUnit texture)
		{
			clientActiveTexture(texture);
		}

		public static void ActiveTexture(TextureUnit texture)
		{
			activeTexture(texture);
		}
	}
}
#endif