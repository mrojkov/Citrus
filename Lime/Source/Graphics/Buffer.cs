using System;
using Lime.Graphics.Platform;

namespace Lime
{
	public unsafe class Buffer : IGLObject, IDisposable
	{
		private IPlatformBuffer platformBuffer;
		private BufferType bufferType;

		public bool Dynamic { get; private set; }
		public bool IsDisposed { get; private set; }

		protected Buffer(BufferType bufferType, bool dynamic)
		{
			this.bufferType = bufferType;
			Dynamic = dynamic;
			GLObjectRegistry.Instance.Add(this);
		}

		~Buffer()
		{
			Discard();
		}

		internal IPlatformBuffer GetPlatformBuffer()
		{
			return platformBuffer;
		}

		private void EnsurePlatformBuffer(int size)
		{
			if (platformBuffer == null || platformBuffer.Size < size) {
				if (platformBuffer != null) {
					platformBuffer.Dispose();
				}
				platformBuffer = RenderContextManager.CurrentContext.CreateBuffer(bufferType, size, Dynamic);
			}
		}

		public void SetData<T>(T[] data, int elementCount) where T : unmanaged
		{
			EnsurePlatformBuffer(sizeof(T) * elementCount);
			platformBuffer.SetData(0, data, 0, elementCount, BufferSetDataMode.Discard);
		}

		public void Dispose()
		{
			Discard();
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		public void Discard()
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
