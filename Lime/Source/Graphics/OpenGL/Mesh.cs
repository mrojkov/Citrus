using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif
using Yuzu;

namespace Lime
{
	public partial class Mesh : IMesh, IDisposable, IGLObject
	{
		private bool disposed;
		private uint vaoHandle;
		private static bool vaoSupported;
		private static bool vaoChecked;
		private static int currentContext;

#if iOS
		internal const string OpenGLLibrary = ObjCRuntime.Constants.OpenGLESLibrary;
#elif MAC
		internal const string OpenGLLibrary = ObjCRuntime.Constants.OpenGLLibrary;
#elif ANDROID
		internal const string OpenGLLibrary = "libGLESv2.dll";
#elif MONOMAC
		internal const string OpenGLLibrary = "/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif WIN
		internal const string OpenGLLibrary = "opengl32.dll";
#endif

		[DllImport(OpenGLLibrary, EntryPoint = "glGenVertexArrays", ExactSpelling = true)]
		private extern unsafe static void glGenVertexArrays(uint n, uint *arrays);

		[DllImport(OpenGLLibrary, EntryPoint = "glDeleteVertexArrays", ExactSpelling = true)]
		private extern unsafe static void glDeleteVertexArrays(uint n, uint *arrays);

		[DllImport(OpenGLLibrary, EntryPoint = "glBindVertexArray", ExactSpelling = true)]
		private extern static void glBindVertexArray(uint array);

		public IIndexBuffer IndexBuffer { get; set; }
		public IVertexBuffer[] VertexBuffers { get; set; }
		public int[][] Attributes { get; set; }
		
		private int context;
		private IGLVertexBuffer[] glVertexBuffers;
		private IGLIndexBuffer glIndexBuffer;
		
		internal static void InvalidateVertexArrayObjects()
		{
			currentContext++;
		}

		public Mesh()
		{
#if WIN || MAC || MONOMAC
			var requiredVaoCheck = !vaoChecked && !(CommonWindow.Current != null && CommonWindow.Current is DummyWindow);
#else
			var requiredVaoCheck = !vaoChecked;
#endif
			if (requiredVaoCheck) {
				vaoChecked = true;
				var ext = GL.GetString(StringName.Extensions);
				vaoSupported = ext?.Contains("OES_vertex_array_object") ?? false;
				Debug.Write(vaoSupported ? "VAO supported." : "VAO not supported.");
			}
			GLObjectRegistry.Instance.Add(this);
		}
		
		public IMesh ShallowClone()
		{
			return (IMesh)MemberwiseClone();
		}

		private unsafe void AllocateVAOHandle()
		{
			fixed (uint* p = &vaoHandle) {
				glGenVertexArrays(1, p);
			}
		}

		~Mesh()
		{
			Dispose();
		}
		
		internal void Bind()
		{
			if (glIndexBuffer == null) {
				glIndexBuffer = (IGLIndexBuffer)IndexBuffer;
				glVertexBuffers = new IGLVertexBuffer[VertexBuffers.Length];
				for (int i = 0; i < VertexBuffers.Length; i++) {
					glVertexBuffers[i] = (IGLVertexBuffer)VertexBuffers[i];
				}
			}
			if (vaoSupported) {
				if (vaoHandle == 0 || context != currentContext) {
					context = currentContext;
					AllocateVAOHandle();
					glBindVertexArray(vaoHandle);
					int i = 0;
					foreach (var vb in glVertexBuffers) {
						vb.SetAttribPointers(Attributes[i++]);
					}
				}
				glBindVertexArray(vaoHandle);
				foreach (var vb in glVertexBuffers) {
					vb.BufferData();
				}
				glIndexBuffer.BufferData();
			} else {
				int i = 0;
				foreach (var vb in glVertexBuffers) {
					vb.SetAttribPointers(Attributes[i++]);
					vb.BufferData();
				}
				glIndexBuffer.BufferData();
			}
		}

		public void Dispose()
		{
			if (!disposed) {
				Discard();
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public unsafe void Discard()
		{
			if (vaoHandle != 0) {
				uint capturedVaoHandle = vaoHandle;
				Window.Current.InvokeOnRendering(() => {
					#if !MAC && !MONOMAC
					if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
						return;
					#endif
					uint h = capturedVaoHandle;
					glDeleteVertexArrays(1, &h);
				});
				vaoHandle = 0;
			}
		}
	}
}
