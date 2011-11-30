using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Audio.OpenAL;

namespace Lime
{
	public class WaveIMA4Decoder : IAudioDecoder
	{
		enum WaveFormat
		{
			Unknown,
			PCM,
			ADPCM,
			IMA_ADPCM = 0x11
		}

		Stream stream;
		int blockSize;
		ushort channels;
		int samplesPerSec;
		ushort origBlockSize;
		int dataPosition, dataSize;
		byte [] origBlockBuffer;
		int currentBlock, totalBlocks;

		public WaveIMA4Decoder (Stream stream)
		{
			this.stream = stream;
			if (ReadID () != "RIFF")
				throw Abort ("Invalid RIFF header");
			ReadInt32 ();
			if (ReadID () != "WAVE")
				throw Abort ("Wave type is expected");
			int fmtSize = 0;
			dataSize = 0;
			while (stream.Position < stream.Length) {
				switch (ReadID ()) {
				case "fmt ":
					fmtSize = ReadInt32 ();
					if (ReadUInt16 () != (ushort)WaveFormat.IMA_ADPCM)
						throw Abort ("Not IMA ADPCM");
					channels = ReadUInt16 ();
					samplesPerSec = ReadInt32 ();
					ReadInt32 ();
					origBlockSize = ReadUInt16 ();
					if (ReadUInt16 () != 4)
						throw Abort ("Not 4-bit format");
					ReadBytes (fmtSize - 16);
					break;
				case "data":
					dataSize = ReadInt32 ();
					dataPosition = (int)stream.Position;
					stream.Position += dataSize;
					break;
				default:
					var size = ReadInt32 ();
					stream.Position += size;
					break;
				}
			}
			if (fmtSize == 0)
				throw Abort ("No format information");
			else if (dataSize == 0)
				throw Abort ("No data");
			totalBlocks = (int)(dataSize / origBlockSize);
			blockSize = (origBlockSize - channels * 4) * 4 + channels * 2;
			origBlockBuffer = new byte [origBlockSize];
			ResetToBeginning ();
		}

		public bool ResetToBeginning ()
		{
			currentBlock = 0;
			stream.Seek (dataPosition, SeekOrigin.Begin);
			return true;
		}

		public int ReadBlocks (IntPtr buffer, int startIndex, int blockCount)
		{
			
			buffer = (IntPtr)(buffer.ToInt64 () + blockSize * startIndex);
			int c = Math.Min (blockCount, totalBlocks - currentBlock);
			for (int i = 0; i < c; i++) {
				if (channels == 2) {
					DecodeStereoBlock (buffer);
				} else {
					throw Abort ("Unsupported number of channels");
				}
				buffer = (IntPtr)(buffer.ToInt64() + blockSize);
			}
			currentBlock += c;
			return c;
		}

		public int GetCompressedSize ()
		{
			return (int)dataSize;
		}

		public int GetBlockSize ()
		{
			return blockSize;
		}

		public ALFormat GetFormat ()
		{
			return channels == 2 ? ALFormat.Stereo16 : ALFormat.Mono16;
		}

		public int GetFrequency ()
		{
			return samplesPerSec;
		}

		public void Dispose ()
		{
			if (stream != null) {
				stream.Dispose ();
				stream = null;
			}
		}

		void DecodeStereoBlock (IntPtr buffer)
		{
			unsafe {
				short* pBuffer = (short*)buffer;
				if (stream.Read (origBlockBuffer, 0, origBlockSize) != origBlockSize) {
					throw Abort ("Read error");
				}
				SampleValue v1 = new SampleValue (origBlockBuffer, 0);
				SampleValue v2 = new SampleValue (origBlockBuffer, 4);
				*pBuffer++ = v1.Value;
				*pBuffer++ = v2.Value;
				for (int i = 8; i < origBlockSize; i += 8) {
					for (int j = 0; j < 4; j++) {
						*pBuffer++ = v1.Next (origBlockBuffer [i + j] & 0xf);
						*pBuffer++ = v2.Next (origBlockBuffer [i + j + 4] & 0xf);
					}
					for (int j = 0; j < 4; j++) {
						*pBuffer++ = v1.Next (origBlockBuffer [i + j] >> 4);
						*pBuffer++ = v2.Next (origBlockBuffer [i + j + 4] >> 4);
					}
				}
			}
		}

		Lime.Exception Abort (string message)
		{
			Dispose ();
			return new Lime.Exception (message);
		}

		byte[] ReadBytes (int length)
		{
			var ret = new byte[length];
			if (length > 0)
				stream.Read (ret, 0, length);
			return ret;
		}

		string ReadID ()
		{
			return Encoding.UTF8.GetString (ReadBytes (4), 0, 4);
		}

		int ReadInt32 ()
		{
			return BitConverter.ToInt32 (ReadBytes (4), 0);
		}

		uint ReadUInt32 ()
		{
			return BitConverter.ToUInt32 (ReadBytes (4), 0);
		}

		ushort ReadUInt16 ()
		{
			return BitConverter.ToUInt16 (ReadBytes (2), 0);
		}
	}

	struct SampleValue
	{
		public short Value;
		public int Index;

		public SampleValue (byte[] value, int startIndex)
		{
			Value = BitConverter.ToInt16 (value, startIndex);
			Index = value [startIndex + 2];
		}

		static int[] StepTable = new []
		{
			7, 8, 9, 10, 11, 12, 13, 14,
			16, 17, 19, 21, 23, 25, 28, 31,
			34, 37, 41, 45, 50, 55, 60, 66,
			73, 80, 88, 97, 107, 118, 130, 143,
			157, 173, 190, 209, 230, 253, 279, 307,
			337, 371, 408, 449, 494, 544, 598, 658,
			724, 796, 876, 963, 1060, 1166, 1282, 1411,
			1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
			3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
			7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
			15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
			32767
		};
			
		static int[] IndexTable = new []
		{
			-1, -1, -1, -1, 2, 4, 6, 8,
			-1, -1, -1, -1, 2, 4, 6, 8
		};

		public short Next (int v)
		{
			int d = 0;
			int s = StepTable [Index];
			if ((v & 4) != 0)
				d += s;
			if ((v & 2) != 0)
				d += s >> 1;
			if ((v & 1) != 0)
				d += s >> 2;
			d += s >> 3;
			if ((v & 8) != 0)
				d = -d;
			int val = ((int)Value) + d;
			if (val > short.MaxValue)
				val = short.MaxValue;
			if (val < short.MinValue)
				val = short.MinValue;

			int idx = Index + IndexTable [v];
			if (idx >= StepTable.Length)
				idx = StepTable.Length - 1;
			if (idx < 0)
				idx = 0;

			Value = (short)val;
			Index = idx;
			return Value;
		}
	}
}

#if MMM
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lime
{
	public enum WAVE_FORMAT
	{
		UNKNOWN,
		PCM,
		ADPCM,
		IMA_ADPCM = 0x11
	}

	public class IMA_ADPCM //: Stream
	{
		Stream fs;
		int length, position;
		public int BlockLen;
		public ushort Channels;
		public int SamplesPerSec;
		public ushort CompressedBlockLen;
		int DataPosition, DataSize;
		int block;
		//private byte[] Header;

		public IMA_ADPCM (string path)
			: this(new FileStream(path, FileMode.Open))
		{
		}

		public IMA_ADPCM (Stream stream)
		{
			fs = stream;
			if (ReadID () != "RIFF")
				throw Abort ("Invalid RIFF header");
			ReadInt32 ();
			if (ReadID () != "WAVE")
				throw Abort ("Wave type is expected");
			int fmtsize = 0;
			DataSize = 0;
			while (fs.Position < fs.Length) {
				switch (ReadID ()) {
				case "fmt ":
					fmtsize = ReadInt32 ();
					if (ReadUInt16 () != (ushort)WAVE_FORMAT.IMA_ADPCM)
						throw Abort ("Not IMA ADPCM");
					Channels = ReadUInt16 ();
					SamplesPerSec = ReadInt32 ();
					ReadInt32 ();
					CompressedBlockLen = ReadUInt16 ();
					if (ReadUInt16 () != 4)
						throw Abort ("Not 4-bit format");
					ReadBytes (fmtsize - 16);
					break;
				case "data":
					DataSize = ReadInt32 ();
					DataPosition = (int)fs.Position;
					fs.Position += DataSize;
					break;
				default:
					var size = ReadInt32 ();
					fs.Position += size;
					break;
				}
			}
			if (fmtsize == 0)
				throw Abort ("No format information");
			else if (DataSize == 0)
				throw Abort ("No data");

			int blocks = (int)(DataSize / CompressedBlockLen);
			BlockLen = (CompressedBlockLen - Channels * 4) * 4 + Channels * 2;
			int datalen = blocks * BlockLen;
			length = datalen + 44;
			Cache = new short [BlockLen / 2];
			/*
			var ms = new MemoryStream ();
			var bw = new BinaryWriter (ms);
			bw.Write (Encoding.UTF8.GetBytes ("RIFF"));
			bw.Write (length - 8);
			bw.Write (Encoding.UTF8.GetBytes ("WAVE"));
			bw.Write (Encoding.UTF8.GetBytes ("fmt "));
			bw.Write (16);
			bw.Write ((ushort)WAVE_FORMAT.PCM); // FormatTag
			bw.Write (Channels);
			bw.Write (SamplesPerSec);
			bw.Write (SamplesPerSec * Channels * 2); // AvgBytesPerSec
			bw.Write ((ushort)(Channels * 2)); // BlockAlign
			bw.Write ((ushort)16); // BitsPerSample
			bw.Write (Encoding.UTF8.GetBytes ("data"));
			bw.Write (datalen);
			Header = ms.ToArray ();
			bw.Close ();
			ms.Close ();*/
		}

		private int CacheNo = -1;
		private short[] Cache;
		/*
		public void Decode (string path)
		{
			using (var fs = new FileStream(path, FileMode.Create)) {
				position = 0;
				fs.Write (Header, 0, Header.Length);
				int blocks = DataSize / BlockAlign;
				for (int i = 0; i < blocks; i++) {
					var block = DecodeBlock (i);
					fs.Write (block, 0, block.Length);
				}
			}
		}*/
		/*
		public IEnumerable<byte[]> DecodeSamples ()
		{
			position = 0;
			int blocks = DataSize / CompressedBlockLen;
			for (int i = 0; i < blocks; i++) {
				var block = DecodeBlock (i);
				yield return block;
			}
		}*/

		bool DecodeBlock (IntPtr buffer, int n)
		{
			//if (n >= DataSize / CompressedBlockLen)
			//	return false;
			//int pos = DataPosition + n * CompressedBlockLen;
			//if (pos >= fs.Length)
			//	return null;

			// fs.Position = pos;
			var data = ReadBytes (compressedBlockSize);
			var ms = new MemoryStream ();
			var bw = new BinaryWriter (ms);
			var v = new SampleValue[Channels];
			for (int ch = 0; ch < Channels; ch++) {
				v [ch] = new SampleValue (data, ch * 4);
				bw.Write (v [ch].Value);
			}
			int ch4 = Channels * 4;
			for (int i = ch4; i < CompressedBlockLen; i += ch4) {
				for (int j = 0; j < 4; j++) {
					for (int ch = 0; ch < Channels; ch++)
						bw.Write (v [ch].Next (data [i + j + ch * 4] & 0xf));
					for (int ch = 0; ch < Channels; ch++)
						bw.Write (v [ch].Next (data [i + j + ch * 4] >> 4));
				}
			}
			CacheNo = n;
			Cache = ms.ToArray ();
			bw.Close ();
			ms.Close ();
			return Cache;
		}

		protected override void Dispose (bool disposing)
		{
			if (fs != null) {
				fs.Close ();
				fs = null;
			}
			base.Dispose (disposing);
		}

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return true; } }

		public override bool CanWrite { get { return false; } }

		public override void Flush ()
		{
		}

		public override long Length { get { return length; } }

		public override long Position {
			get { return position; }
			set {
				position = (int)value;
				if (position < 0)
					position = 0;
				if (position > length)
					position = length;
			}
		}

		public int ReadBlocks (IntPtr buffer, int startIndex, int blockCount)
		{
			return 0;
		}
		/*
		public override int Read (byte[] buffer, int offset, int count)
		{
			var pp = new IntPtr (10);
			unsafe {
				int* x = (int*)pp;
				for (int i = 0; i < 10; i++) {
					*x = 123;
					x++;
				}
			}
			int totalRead = 0;
			while (count > 0) {

				int l = Math.Min (count, BlockLen);
				var p = DecodeStereoBlock (block++);
				int t = l / 2;
				for (int i = 0; i < p.Length && i < t; i++) {
					short x = p [i];
					buffer [totalRead++] = (byte)(x >> 8);
					buffer [totalRead++] = (byte)(x & 255);
				}
				totalRead += l;
				count -= l;
			}
			return totalRead;
		}*/

		/*
		public override long Seek (long offset, SeekOrigin origin)
		{
			switch (origin) {
			case SeekOrigin.Begin:
				Position = offset;
				break;
			case SeekOrigin.Current:
				Position += offset;
				break;
			case SeekOrigin.End:
				Position = length + (int)origin;
				break;
			}
			return Position;
		}*/
	}
}
#endif