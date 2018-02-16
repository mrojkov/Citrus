using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	public class PcmDecoder : IAudioDecoder
	{
		private const int BlockSize = 1;
		private Stream stream;

		public PcmDecoder(Stream stream)
		{
			this.stream = stream;
		}

		public void Dispose()
		{
			
		}

		public int GetBlockSize()
		{
			return BlockSize;
		}

		public int GetCompressedSize()
		{
			throw new NotImplementedException();
		}

		public AudioFormat GetFormat()
		{
			return AudioFormat.Stereo16;
		}

		public int GetFrequency()
		{
			return 44100;
		}

		byte[] readBuffer = new byte[1];
		public int ReadBlocks(IntPtr buffer, int startIndex, int blockCount)
		{
			buffer = (IntPtr)(buffer.ToInt64() + BlockSize * startIndex);
			int c = (int)Math.Min(blockCount, stream.Length - stream.Position); //(int) be careful
			if (readBuffer.LongLength < blockCount) {
				readBuffer = new byte[blockCount];
			}
			if (c < blockCount) {
				Array.Clear(readBuffer, c, blockCount - c);
			}
			stream.Read(readBuffer, startIndex, c);
			Marshal.Copy(readBuffer, 0, buffer, blockCount);
			return blockCount;
		}

		public void Rewind()
		{
			throw new NotImplementedException();
		}
	}
}
