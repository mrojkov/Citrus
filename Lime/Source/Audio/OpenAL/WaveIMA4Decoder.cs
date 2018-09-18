#if OPENAL
using System;
using System.IO;
using System.Text;

#if !MONOMAC
using OpenTK.Audio.OpenAL;
#else
using MonoMac.OpenAL;
#endif

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
		ushort channels;
		int samplesPerSec;
		int blockSize;
		ushort origBlockSize;
		int dataPosition;
		int dataSize;
		int currentBlock;
		int totalBlocks;
		byte[] origBlockBuffer;

		public static bool IsWaveStream(Stream stream)
		{
			var position = stream.Position;
			bool result = stream.ReadByte() == 'R' &&
				stream.ReadByte() == 'I' &&
				stream.ReadByte() == 'F' &&
				stream.ReadByte() == 'F';
			stream.Seek(position, SeekOrigin.Begin);
			return result;
		}

		public WaveIMA4Decoder(Stream stream)
		{
			this.stream = stream;
			if (ReadID() != "RIFF")
				throw Abort("Invalid RIFF header");
			ReadInt32();
			if (ReadID() != "WAVE")
				throw Abort("Wave type is expected");
			int fmtSize = 0;
			dataSize = 0;
			while (stream.Position < stream.Length) {
				switch(ReadID()) {
				case "fmt ":
					fmtSize = ReadInt32();
					uint fmt = ReadUInt16();
					if (fmt != (ushort)WaveFormat.IMA_ADPCM)
						throw Abort("Not IMA ADPCM");
					channels = ReadUInt16();
					samplesPerSec = ReadInt32();
					ReadInt32();
					origBlockSize = ReadUInt16();
					if (ReadUInt16() != 4)
						throw Abort("Not 4-bit format");
					ReadBytes(fmtSize - 16);
					break;
				case "data":
					dataSize = ReadInt32();
					dataPosition = (int)stream.Position;
					stream.Position += dataSize;
					break;
				default:
					var size = ReadInt32();
					stream.Position += size;
					break;
				}
			}
			if (fmtSize == 0)
				throw Abort("No format information");
			else if (dataSize == 0)
				throw Abort("No data");
			totalBlocks = (int)(dataSize / origBlockSize);
			blockSize = (origBlockSize - channels * 4) * 4 + channels * 2;
			origBlockBuffer = new byte[origBlockSize];
			Rewind();
		}

		public void Rewind()
		{
			currentBlock = 0;
			stream.Seek(dataPosition, SeekOrigin.Begin);
		}

		public int ReadBlocks(IntPtr buffer, int startIndex, int blockCount)
		{
			buffer = (IntPtr)(buffer.ToInt64() + blockSize * startIndex);
			int c = Math.Min(blockCount, totalBlocks - currentBlock);
			for (int i = 0; i < c; i++) {
				if (stream.Read(origBlockBuffer, 0, origBlockSize) != origBlockSize) {
					throw Abort("Read error");
				}
				if (channels == 2) {
					DecodeStereoBlock(buffer);
				}  else if (channels == 1) {
					DecodeMonoBlock(buffer);
				} else {
					throw Abort("Unsupported number of channels");
				}
				buffer = (IntPtr)(buffer.ToInt64() + blockSize);
			}
			currentBlock += c;
			return c;
		}

		public int GetCompressedSize()
		{
			return (int)dataSize;
		}

		public int GetBlockSize()
		{
			return blockSize;
		}

		public AudioFormat GetFormat()
		{
			return channels == 2 ? AudioFormat.Stereo16 : AudioFormat.Mono16;
		}

		public int GetFrequency()
		{
			return samplesPerSec;
		}

		public void Dispose()
		{
			if (stream != null) {
				stream.Dispose();
				stream = null;
			}
		}

		void DecodeMonoBlock (IntPtr buffer)
		{
			unsafe {
				short* pBuffer = (short*)buffer;
				DecoderState state = new DecoderState(origBlockBuffer, 0);
				*pBuffer++ = state.Value;
				for (int i = 4; i < origBlockBuffer.Length; i++) {
					var b = origBlockBuffer[i];
					*pBuffer++ = state.Next(b & 0xf);
					*pBuffer++ = state.Next(b >> 4);
				}
			}
		}

		void DecodeStereoBlock(IntPtr buffer)
		{
			unsafe {
				short* pBuffer = (short*)buffer;
				DecoderState channel1 = new DecoderState(origBlockBuffer, 0);
				DecoderState channel2 = new DecoderState(origBlockBuffer, 4);
				*pBuffer++ = channel1.Value;
				*pBuffer++ = channel2.Value;
				for (int i = 8; i < origBlockSize; i += 8) {
					for (int j = 0; j < 4; j++) {
						int a = origBlockBuffer[i + j];
						int b = origBlockBuffer[i + j + 4];
						*pBuffer++ = channel1.Next(a & 0xf);
						*pBuffer++ = channel2.Next(b & 0xf);
						*pBuffer++ = channel1.Next(a >> 4);
						*pBuffer++ = channel2.Next(b >> 4);
					}
				}
			}
		}

		Lime.Exception Abort(string message)
		{
			Dispose();
			return new Lime.Exception(message);
		}

		byte[] ReadBytes(int length)
		{
			var ret = new byte[length];
			if (length > 0)
				stream.Read(ret, 0, length);
			return ret;
		}

		string ReadID()
		{
			return Encoding.UTF8.GetString(ReadBytes(4), 0, 4);
		}

		int ReadInt32()
		{
			return BitConverter.ToInt32(ReadBytes(4), 0);
		}

		uint ReadUInt32()
		{
			return BitConverter.ToUInt32(ReadBytes(4), 0);
		}

		ushort ReadUInt16()
		{
			return BitConverter.ToUInt16(ReadBytes(2), 0);
		}
	}

	struct DecoderState
	{
		public short Value;
		public int Index;

		public DecoderState(byte[] value, int startIndex)
		{
			Value = BitConverter.ToInt16(value, startIndex);
			Index = value[startIndex + 2];
		}

		static int[] StepTable = new[]
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
			
		static int[] IndexTable = new[]
		{
			-1, -1, -1, -1, 2, 4, 6, 8,
			-1, -1, -1, -1, 2, 4, 6, 8
		};

		public short Next(int v)
		{
			int s = StepTable[Index];
			int d = 0;
			if ((v & 4) != 0)
				d += s;
			if ((v & 2) != 0)
				d += s >> 1;
			if ((v & 1) != 0)
				d += s >> 2;
			d += s >> 3;
			if ((v & 8) != 0)
				d = -d;
			// Another version, uses integer multiplication. Perhaps faster on modern CPU.
			// d = (s * (v & 7) / 4) + (s / 8);
			// if ((v & 8) != 0)
			//	d = -d;
			int val = ((int)Value) + d;
			if (val > short.MaxValue)
				val = short.MaxValue;
			if (val < short.MinValue)
				val = short.MinValue;
			int idx = Index + IndexTable[v];
			if (idx > 88)
				idx = 88;
			else if (idx < 0)
				idx = 0;
			Value = (short)val;
			Index = idx;
			return Value;
		}
	}
}
#endif