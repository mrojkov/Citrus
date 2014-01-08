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
		public static class Delegates
		{
			public delegate void ClientActiveTexture(TextureUnit texture);
			public delegate void ActiveTexture(TextureUnit texture);
			public delegate void GenerateMipmap(GenerateMipmapTarget target);
			public delegate void CompressedTexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, int imageSize, IntPtr data);
			public delegate void GenFramebuffers(int num, out int framebuffer);
			public delegate void BindFramebuffer(FramebufferTarget target, int framebuffer);
			public delegate void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level);
			public delegate FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget target);
		}

		public static Delegates.GenerateMipmap GenerateMipmap = GetProc<Delegates.GenerateMipmap>("glGenerateMipmap");
		public static Delegates.ClientActiveTexture ClientActiveTexture = GetProc<Delegates.ClientActiveTexture>("glClientActiveTexture");
		public static Delegates.ActiveTexture ActiveTexture = GetProc<Delegates.ActiveTexture>("glActiveTexture");
		public static Delegates.GenFramebuffers GenFramebuffers = GetProc<Delegates.GenFramebuffers>("glGenFramebuffers");
		public static Delegates.BindFramebuffer BindFramebuffer = GetProc<Delegates.BindFramebuffer>("glBindFramebuffer");
		public static Delegates.FramebufferTexture2D FramebufferTexture2D = GetProc<Delegates.FramebufferTexture2D>("glFramebufferTexture2D");
		public static Delegates.CheckFramebufferStatus CheckFramebufferStatus = GetProc<Delegates.CheckFramebufferStatus>("glCheckFramebufferStatus");
		private static Delegates.CompressedTexImage2D compressedTexImage2D = GetProc<Delegates.CompressedTexImage2D>("glCompressedTexImage2D");

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

		[DllImport(library, EntryPoint = "glScissor")]
		public static extern void Scissor(int x, int y, int width, int height);

		[DllImport(library, EntryPoint = "glViewport")]
		public static extern void Viewport(int x, int y, int width, int height);

		[DllImport(library, EntryPoint = "glEnable")]
		public static extern void Enable(EnableCap cap);

		[DllImport(library, EntryPoint = "glDisable")]
		public static extern void Disable(EnableCap cap);

		[DllImport(library, EntryPoint = "glBindTexture")]
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

		[DllImport(library, EntryPoint = "glLoadMatrixf")]
		public static extern void LoadMatrix(float[] values);

		[DllImport(library, EntryPoint = "glFinish")]
		public static extern void Finish();
		
		[DllImport(library, EntryPoint = "glGetIntegerv")]
		public static extern void GetInteger(GetPName pname, out int value);
	
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

		[DllImport(library, EntryPoint = "glTexSubImage2D")]
		public static extern void TexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, IntPtr pixels);

		public static void TexSubImage2D<T>(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, T[] pixels) where T : struct
		{
			GCHandle ptr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			try {
				TexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, (IntPtr)ptr.AddrOfPinnedObject());
			} finally {
				ptr.Free();
			}
		}
	}
}
#endif