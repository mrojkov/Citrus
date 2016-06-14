using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Lzma
{
	public class LzmaCompressionStream : LzmaStream
	{
		Encoder encoder;
		MemoryStream bufferStream;

		public LzmaOptions Options { get; private set; }
		public override bool CanRead { get { return false; } }
		public override bool CanWrite { get { return BaseStream.CanWrite; } }

		public LzmaCompressionStream(Stream baseStream)
			: this(baseStream, null) { }

		public LzmaCompressionStream(Stream baseStream, LzmaOptions options)
			: this(baseStream, options, false) { }

		public LzmaCompressionStream(Stream baseStream, bool leaveOpen)
			: this(baseStream, null, leaveOpen) { }

		public LzmaCompressionStream(Stream baseStream, LzmaOptions options, bool leaveOpen)
			: base(baseStream, leaveOpen)
		{
			Options = options ?? LzmaOptions.Default;
			bufferStream = new MemoryStream(DefaultBufferCapacity);
			encoder = new Encoder();
			encoder.SetCoderProperties(
				new CoderPropID[] {
					CoderPropID.DictionarySize,
					CoderPropID.PosStateBits,
					CoderPropID.LitContextBits,
					CoderPropID.LitPosBits,
					CoderPropID.Algorithm,
					CoderPropID.NumFastBytes,
					CoderPropID.MatchFinder,
					CoderPropID.EndMarker
				},
				new object[] {
					Options.DictionarySize,
					Options.PosStateBits,
					Options.LiteralContextBits,
					Options.LiteralPosBits,
					Options.AlgorithmVersion,
					Options.NumFastBytes,
					Options.MatchFinder.GetName(),
					false
				});
		}

		protected override void DoDispose(bool disposing)
		{
			if (disposing) {
				Flush();
				bufferStream.Dispose();
				bufferStream = null;
				encoder = null;
			}
			base.DoDispose(disposing);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			int n;
			while ((n = Options.MaxChunkSize - (int)bufferStream.Length) <= count) {
				bufferStream.Write(buffer, offset, n);
				offset += n;
				count -= n;
				FlushChunk();
			}
			if (count > 0)
				bufferStream.Write(buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			bufferStream.WriteByte(value);
			if (bufferStream.Length == Options.MaxChunkSize)
				FlushChunk();
		}

		public override void Flush()
		{
			if (bufferStream.Length > 0)
				FlushChunk();
		}

		void FlushChunk()
		{
			encoder.WriteCoderProperties(BaseStream);
			BaseStream.WriteInt64(bufferStream.Length);
			bufferStream.Position = 0;
			encoder.Code(bufferStream, BaseStream, -1, -1, null);
			bufferStream.Position = 0;
			bufferStream.SetLength(0);
		}
	}
}