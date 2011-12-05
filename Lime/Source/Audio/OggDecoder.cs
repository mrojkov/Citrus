using System;
using System.IO;
using System.Text;
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;

namespace Lime
{
	public class OggDecoder : IAudioDecoder
	{
		Stream stream;
		IntPtr oggFile;
		Lemon.FileSystem fs;
		int bitstream;

		public OggDecoder (Stream stream)
		{
			this.stream = stream;
			fs = new Lemon.FileSystem { 
				ReadFunc = OggRead, CloseFunc = OggClose,
				SeekFunc = OggSeek, TellFunc = OggTell
			};
			oggFile = Lemon.OggCreate ();
			if (Lemon.OggOpen (stream, oggFile, fs) < 0) {
				throw new Lime.Exception ("Failed to open OGG/Vorbis file");
			}
			if (Lemon.OggGetChannels (oggFile) > 2) {
				throw new Lime.Exception ("Channel count must be either 1 or 2");
			}
		}

		public void Dispose ()
		{
			if (stream != null) {
				stream.Dispose ();
				stream = null;
			}
			if (oggFile != null) {
				Lemon.OggDispose (oggFile);
				oggFile = IntPtr.Zero;
			}
		}

		public ALFormat GetFormat ()
		{
			int channels = Lemon.OggGetChannels (oggFile);
			return channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
		}

		public int GetFrequency ()
		{
			return Lemon.OggGetFrequency (oggFile);
		}

		public int GetCompressedSize ()
		{
			return (int)stream.Length;
		}

		public void ResetToBeginning ()
		{
			Lemon.OggResetToBeginning (oggFile);
		}

		public int GetBlockSize ()
		{
			return 1;
		}

		public int ReadBlocks (IntPtr buffer, int startIndex, int blockCount)
		{
			int actualCount = 0;
			int requestCount = blockCount;
			while (true) {
				var p = new IntPtr (buffer.ToInt64 () + startIndex + actualCount);
				int read = Lemon.OggRead (oggFile, p, requestCount - actualCount, ref bitstream);
				if (read < 0) {
					throw new Lime.Exception ("Read error");
				}
				if (read == 0) {
					return actualCount;
				}
				actualCount += read;
			}
		}

		static uint OggRead (IntPtr buffer, uint size, uint nmemb, Stream stream)
		{
			byte [] block = new byte [1024 * 4];
			int actualCount = 0;
			int requestCount = (int)(size * nmemb);
			while (true) {
				int read = stream.Read (block, 0, Math.Min (block.Length, requestCount - actualCount));
				if (read == 0)
					break;
				Marshal.Copy (block, 0, (IntPtr)(buffer.ToInt64 () + actualCount), read);
				actualCount += read;
			}
			return (uint)actualCount;
		}

		static int OggTell (Stream stream)
		{
			return (int)stream.Position;
		}

		static int OggSeek (Stream stream, long offset, SeekOrigin whence)
		{
			//return -1;
			return (int)stream.Seek (offset, whence);
		}

		static int OggClose (Stream stream)
		{
			stream.Close ();
			return 0;
		}
	}
}
