using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using PrimitiveType = MonoMac.OpenGL.BeginMode;
#elif WIN
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif
using System.Runtime.InteropServices;

namespace Lime
{
	[StructLayout(LayoutKind.Explicit, Size = 32)]
	public struct Vertex
	{
		[FieldOffset(0)]
		public Vector2 Pos;

		[FieldOffset(8)]
		public Vector2 UV1;

		[FieldOffset(16)]
		public Color4 Color;

		[FieldOffset(20)]
		public Vector2 UV2;
	}

	public static class VertexBufferPool
	{
		private static Queue<VertexBuffer> items = new Queue<VertexBuffer>(); 

		public static VertexBuffer Acquire()
		{
			if (items.Count * 2 <= VertexBuffer.TotalVertexBuffers) {
				DoubleVertexBuffers();
			}
			return items.Dequeue();
		}

		private static void DoubleVertexBuffers()
		{
			var c = Math.Max(1, VertexBuffer.TotalVertexBuffers);
			for (int i = 0; i < c; i++) {
				items.Enqueue(new VertexBuffer());
			}
		}

		public static void Release(VertexBuffer item)
		{
			item.Clear();
			items.Enqueue(item);
		}
	}

	public unsafe class VertexBuffer : IDisposable
	{
		public const int Capacity = 1000;
		public Vertex* Vertices;
		public int VertexCount;
		public bool Uploaded;

		public static int TotalVertexBuffers;
		private uint vboHandle;
		private uint vaoHandle;
		private bool disposed;

		internal static class Attributes
		{
			public const int Position = 0;
			public const int UV1 = 1;
			public const int UV2 = 2;
			public const int Color = 3;

			public static void BindLocations(ShaderProgram p)
			{
				p.BindAttribLocation(VertexBuffer.Attributes.Position, "inPos");
				p.BindAttribLocation(VertexBuffer.Attributes.UV1, "inTexCoords1");
				p.BindAttribLocation(VertexBuffer.Attributes.UV2, "inTexCoords2");
				p.BindAttribLocation(VertexBuffer.Attributes.Color, "inColor");
			}
		}

		public VertexBuffer()
		{
			TotalVertexBuffers++;
			Vertices = (Vertex*)Marshal.AllocHGlobal(sizeof(Vertex) * Capacity);
			AllocateVBOHandle();
			AllocateVAOHandle();
			GL.BindVertexArray(vaoHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
			GL.EnableVertexAttribArray(Attributes.Position);
			GL.EnableVertexAttribArray(Attributes.UV1);
			GL.EnableVertexAttribArray(Attributes.Color);
			GL.EnableVertexAttribArray(Attributes.UV2);
			GL.VertexAttribPointer(Attributes.Position, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), (IntPtr)0);
			GL.VertexAttribPointer(Attributes.UV1, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), (IntPtr)8);
			GL.VertexAttribPointer(Attributes.Color, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(Vertex), (IntPtr)16);
			GL.VertexAttribPointer(Attributes.UV2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), (IntPtr)20);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			PlatformRenderer.CheckErrors();
		}

		~VertexBuffer()
		{
			Dispose();
		}

		private void AllocateVAOHandle()
		{
			var t = new int[1];
			GL.GenVertexArrays(1, t);
			vaoHandle = (uint)t[0];
		}

		public void Clear()
		{
			VertexCount = 0;
			Uploaded = false;
		}

		private void AllocateVBOHandle()
		{
			var t = new int[1];
			GL.GenBuffers(1, t);
			vboHandle = (uint)t[0];
		}

		public void Bind()
		{
			GL.BindVertexArray(vaoHandle);
			if (!Uploaded) {
				Uploaded = true;
				GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(Vertex) * VertexCount), (IntPtr)Vertices, BufferUsageHint.DynamicDraw);
			}
			PlatformRenderer.CheckErrors();
		}

		public void Dispose()
		{
			if (!disposed) {
				TotalVertexBuffers--;
				Marshal.FreeHGlobal((IntPtr)Vertices);
				Vertices = null;
				if (OpenTK.Graphics.GraphicsContext.CurrentContext != null) {
					GL.DeleteBuffers(1, new uint[] { vboHandle });
					GL.DeleteVertexArrays(1, new uint[] { vaoHandle });
				}
				vaoHandle = 0;
				vboHandle = 0;
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}
}