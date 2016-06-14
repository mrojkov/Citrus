using System;
using System.IO;
using SevenZip.Compression.LZMA;

namespace Lzma
{
	public class LzmaDecompressionStream : LzmaStream
	{
		Decoder decoder;
		MemoryStream bufferStream;
		byte[] propertyBuffer = new byte[5];

		public override bool CanRead { get { return BaseStream.CanRead; } }
		public override bool CanWrite { get { return false; } }

		public LzmaDecompressionStream(Stream baseStream)
			: this(baseStream, false) { }

		public LzmaDecompressionStream(Stream baseStream, bool leaveOpen)
			: base(baseStream, leaveOpen)
		{
			decoder = new Decoder();
			bufferStream = new MemoryStream(DefaultBufferCapacity);
		}

		protected override void DoDispose(bool disposing)
		{
			if (disposing) {
				bufferStream.Dispose();
				bufferStream = null;
				decoder = null;
				propertyBuffer = null;
			}
			base.DoDispose(disposing);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var remnant = count;
			int read;
			while ((read = bufferStream.Read(buffer, offset, remnant)) != remnant) {
				offset += read;
				remnant -= read;
				if (!ReadChunk())
					return count - remnant;
			}
			return count;
		}

		public override int ReadByte()
		{
			var b = bufferStream.ReadByte();
			if (b == -1 && ReadChunk())
				b = bufferStream.ReadByte();
			return b;
		}

		bool ReadChunk()
		{
			var read = BaseStream.Read(propertyBuffer, 0, propertyBuffer.Length);
			if (read == 0)
				return false;
			long chunkSize;
			if (read != propertyBuffer.Length || !BaseStream.TryReadInt64(out chunkSize))
				throw CorruptionFound();
			if (bufferStream.Length != chunkSize) {
				bufferStream.SetLength(chunkSize);
			}
			if (chunkSize == 0)
				return false;
			bufferStream.Position = 0;
			decoder.SetDecoderProperties(propertyBuffer);
			decoder.Code(BaseStream, bufferStream, -1, chunkSize, null);
			bufferStream.Position = 0;
			return true;
		}

		public override void Flush() { }

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		LzmaException CorruptionFound()
		{
			return new LzmaException("Corruption found");
		}
	}
}