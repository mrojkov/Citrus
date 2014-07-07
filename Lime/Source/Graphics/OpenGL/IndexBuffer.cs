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
	public static class IndexBufferPool
	{
		private static Queue<IndexBuffer> items = new Queue<IndexBuffer>();

		public static IndexBuffer Acquire()
		{
			if (items.Count * 2 <= IndexBuffer.TotalIndexBuffers) {
				DoubleIndexBuffers();
			}
			return items.Dequeue();
		}

		private static void DoubleIndexBuffers()
		{
			var c = Math.Max(1, IndexBuffer.TotalIndexBuffers);
			for (int i = 0; i < c; i++) {
				items.Enqueue(new IndexBuffer());
			}
		}

		public static void Release(IndexBuffer item)
		{
			item.Clear();
			items.Enqueue(item);
		}
	}

	public unsafe class IndexBuffer : IDisposable
	{
		public const int DefaultCapacity = 400;
		public readonly int Capacity;
		public ushort* Indices;
		public int IndexCount;
		public bool Uploaded;

		public static int TotalIndexBuffers;
		private uint iboHandle;
		private bool disposed;

		public IndexBuffer(int capacity = DefaultCapacity)
		{
			this.Capacity = capacity;
			TotalIndexBuffers++;
			Indices = (ushort*)Marshal.AllocHGlobal(sizeof(ushort) * Capacity);
			AllocateIBOHandle();
			PlatformRenderer.CheckErrors();
		}

		~IndexBuffer()
		{
			Dispose();
		}

		public void Clear()
		{
			IndexCount = 0;
			Uploaded = false;
		}

		private void AllocateIBOHandle()
		{
			var t = new int[1];
			GL.GenBuffers(1, t);
			iboHandle = (uint)t[0];
		}

		private static uint boundHandle = 0;

		public void Bind()
		{
			if (boundHandle != iboHandle) {
				boundHandle = iboHandle;
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboHandle);
			}
			if (!Uploaded) {
				Uploaded = true;
				GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * IndexCount), (IntPtr)Indices, BufferUsageHint.DynamicDraw);
			}
		}

		public void Dispose()
		{
			if (!disposed) {
				TotalIndexBuffers--;
				Marshal.FreeHGlobal((IntPtr)Indices);
				Indices = null;
				if (OpenTK.Graphics.GraphicsContext.CurrentContext != null) {
					GL.DeleteBuffers(1, new uint[] { iboHandle });
				}
				iboHandle = 0;
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}
}