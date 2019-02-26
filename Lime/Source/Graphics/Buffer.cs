using System;
using Lime.Graphics.Platform;

namespace Lime
{
	public unsafe class Buffer : IDisposable
	{
		private IPlatformBuffer platformBuffer;
		private BufferType bufferType;

		public bool Dynamic { get; private set; }
		public bool IsDisposed { get; private set; }

		protected Buffer(BufferType bufferType, bool dynamic)
		{
			this.bufferType = bufferType;
			Dynamic = dynamic;
		}

		~Buffer()
		{
			DisposeInternal();
		}

		internal IPlatformBuffer GetPlatformBuffer()
		{
			return platformBuffer;
		}

		private void EnsurePlatformBuffer(int size)
		{
			if (platformBuffer == null || platformBuffer.Size != size) {
				if (platformBuffer != null) {
					platformBuffer.Dispose();
				}
				platformBuffer = PlatformRenderer.Context.CreateBuffer(bufferType, size, Dynamic);
				Rebind();
			}
		}

		private void Rebind()
		{
			switch (bufferType) {
				case BufferType.Vertex:
					PlatformRenderer.RebindVertexBuffer(this);
					break;
				case BufferType.Index:
					PlatformRenderer.RebindIndexBuffer(this);
					break;
			}
		}

		public void SetData<T>(T[] data, int elementCount) where T : unmanaged
		{
			EnsurePlatformBuffer(sizeof(T) * elementCount);
			platformBuffer.SetData(0, data, 0, elementCount, Dynamic ? BufferSetDataMode.Discard : BufferSetDataMode.Default);
		}

		public void Dispose()
		{
			DisposeInternal();
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void DisposeInternal()
		{
			if (platformBuffer != null) {
				var platformBufferCopy = platformBuffer;
				Window.Current.InvokeOnRendering(() => {
					platformBufferCopy.Dispose();
				});
				platformBuffer = null;
			}
		}
	}
}
