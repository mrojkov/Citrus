using System;
using System.IO;

namespace Lzma
{
	public abstract class LzmaStream : Stream
	{
		internal const int DefaultBufferCapacity = 1024 * 1024;

		bool disposed;
		bool leaveOpen;

		public Stream BaseStream { get; private set; }
		public override bool CanSeek { get { return false; } }
		public override long Length { get { throw new NotSupportedException(); } }

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		protected LzmaStream(Stream baseStream, bool leaveOpen)
		{
			BaseStream = baseStream;
			this.leaveOpen = leaveOpen;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			DoDispose(disposing);
			disposed = true;
		}

		protected virtual void DoDispose(bool disposing)
		{
			if (disposing) {
				if (!leaveOpen)
					BaseStream.Dispose();
				BaseStream = null;
			}
		}
	}
}