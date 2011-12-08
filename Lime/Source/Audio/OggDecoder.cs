using System;
using System.IO;
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;

namespace Lime
{
	public class OggDecoder : IAudioDecoder
	{
		Stream stream;
		IntPtr oggFile;
		Lemon.FileSystem fileSystem;
		int bitstream;
		int handle;
		static StreamMap streamMap = new StreamMap ();
		
		public OggDecoder (Stream stream)
		{
			this.stream = stream;
			fileSystem = new Lemon.FileSystem { 
				ReadFunc = OggRead, CloseFunc = OggClose,
				SeekFunc = OggSeek, TellFunc = OggTell
			};
			handle = streamMap.Allocate (stream);
			oggFile = Lemon.OggCreate ();
			if (Lemon.OggOpen (handle, oggFile, fileSystem) < 0) {
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
			if (oggFile != IntPtr.Zero) {
				Lemon.OggDispose (oggFile);
				oggFile = IntPtr.Zero;
			}
			if (handle != 0) {
				streamMap.Release (handle);
				handle = 0;
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

		public void Rewind ()
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
		
#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.ReadCallback))]
#endif
		public static uint OggRead (IntPtr buffer, uint size, uint nmemb, int handle)
		{
			byte [] block = new byte [1024 * 16];
			int actualCount = 0;
			int requestCount = (int)(size * nmemb);
			while (true) {
				var stream = streamMap [handle];
				int read = stream.Read (block, 0, Math.Min (block.Length, requestCount - actualCount));
				if (read == 0)
					break;
				Marshal.Copy (block, 0, (IntPtr)(buffer.ToInt64 () + actualCount), read);
				actualCount += read;
			}
			return (uint)actualCount;
		}

#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.TellCallback))]
#endif
		public static int OggTell (int handle)
		{
			var stream = streamMap [handle];
			return (int)stream.Position;
		}
		
#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.SeekCallback))]
#endif
		public static int OggSeek (int handle, long offset, SeekOrigin whence)
		{
			var stream = streamMap [handle];
			return (int)stream.Seek (offset, whence);
		}
		
#if iOS
		[MonoTouch.MonoPInvokeCallback(typeof(Lemon.CloseCallback))]
#endif
		public static int OggClose (int handle)
		{
			var stream = streamMap [handle];
			stream.Close ();
			return 0;
		}
	}

	class StreamMap
	{
		Stream [] map = new Stream [64];

		public int Allocate (Stream stream)
		{
			lock (map) {
				for (int i = 1; i < map.Length; i++) {
					if (map [i] == null) {
						map [i] = stream;
						return i;
					}
				}
			}
			throw new Lime.Exception ("Too many opened streams");
		}

		public void Release (int slot)
		{
			lock (map) {
				map [slot] = null;
			}
		}

		public Stream this [int slot]
		{
			get {
				return map [slot];
			}
		}
	}
}