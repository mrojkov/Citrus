using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	internal sealed class RewindableStream : Stream
	{
		Stream baseStream;
		List<byte> takenBytes;
		int position;
		bool firstPass;

		public RewindableStream(Stream baseStream)
		{
			this.baseStream = baseStream;
			takenBytes = new List<byte>();
			firstPass = true;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (offset != 0 || origin != SeekOrigin.Begin) {
				throw new InvalidOperationException();
			}
			if (!firstPass) {
				throw new InvalidOperationException("Attempt to call Seek() twice");
			}
			firstPass = false;
			position = 0;
			return 0;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (firstPass) {
				int read = baseStream.Read(buffer, offset, count);
				for (int i = 0; i < read; i++) {
					takenBytes.Add(buffer[i + offset]);
				}
				return read;
			} else if (position < takenBytes.Count) {
				int read = Math.Min(position + count, takenBytes.Count);
				takenBytes.CopyTo(position, buffer, offset, read);
				position += read;
				return read;
			} else {
				return baseStream.Read(buffer, offset, count);
			}
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return false; } }
		public override void Flush() { throw new InvalidOperationException(); }
		public override long Length { get { return baseStream.Length; } }
		public override void Write(byte[] buffer, int offset, int count) { throw new InvalidOperationException(); }
		public override void SetLength(long value) { throw new InvalidOperationException(); }
		public override long Position
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}
	}
}
