#if OPENAL
using System;
using System.IO;
using Lemon;
#if iOS
using ObjCRuntime;
#endif
using System.Runtime.InteropServices;

#if !MONOMAC
using OpenTK.Audio.OpenAL;
#else
using MonoMac.OpenAL;
#endif

namespace Lime
{
	public class OggDecoder : IAudioDecoder
	{
		Stream stream;
		IntPtr oggFile;
		int bitstream;
		int handle;
		readonly Lemon.Api.FileSystem fileSystem;
		static readonly StreamMap streamMap = new StreamMap();

		public static int GetCurrentStreamsCount()
		{
			return streamMap.GetCurrentStreamsCount();
		}

		public OggDecoder(Stream stream)
		{
			this.stream = stream;
			fileSystem = new Lemon.Api.FileSystem {
				ReadFunc = OggRead, CloseFunc = OggClose,
				SeekFunc = OggSeek, TellFunc = OggTell
			};
			handle = streamMap.Allocate(stream);
			oggFile = Lemon.Api.OggCreate();
			if (Lemon.Api.OggOpen(handle, oggFile, fileSystem) < 0) {
				throw new Lime.Exception("Failed to open OGG/Vorbis file");
			}
			if (Lemon.Api.OggGetChannels(oggFile) > 2) {
				throw new Lime.Exception("Channel count must be either 1 or 2");
			}
		}

		public static bool IsOggStream(Stream stream)
		{
			bool result = stream.ReadByte() == 'O' &&
				stream.ReadByte() == 'g' &&
				stream.ReadByte() == 'g' &&
				stream.ReadByte() == 'S';
			stream.Seek(-4, SeekOrigin.Current);
			return result;
		}

		public AudioFormat GetFormat()
		{
			ThrowIfDisposed();
			int channels = Lemon.Api.OggGetChannels(oggFile);
			return channels == 1 ? AudioFormat.Mono16 : AudioFormat.Stereo16;
		}

		public int GetFrequency()
		{
			ThrowIfDisposed();
			return Lemon.Api.OggGetFrequency(oggFile);
		}

		public int GetCompressedSize()
		{
			ThrowIfDisposed();
			return (int)stream.Length;
		}

		public void Rewind()
		{
			ThrowIfDisposed();
			Lemon.Api.OggResetToBeginning(oggFile);
		}

		public int GetBlockSize()
		{
			return 1;
		}

		public int ReadBlocks(IntPtr buffer,int startIndex,int blockCount)
		{
			ThrowIfDisposed();
			int actualCount = 0;
			int requestCount = blockCount;
			while (true) {
				var p = new IntPtr(buffer.ToInt64() + startIndex + actualCount);
				int read = Lemon.Api.OggRead(oggFile, p, requestCount - actualCount, ref bitstream);
				if (read < 0) {
					throw new Lime.Exception("Read error");
				}
				if (read == 0) {
					return actualCount;
				}
				actualCount += read;
			}
		}

		[ThreadStatic]
		static byte[] block;

#if iOS
		[MonoPInvokeCallback(typeof(Lemon.Api.ReadCallback))]
#endif
		public static uint OggRead(IntPtr buffer, uint size, uint nmemb, int handle)
		{
			if (block == null) {
				block = new byte[1024 * 16];
			}
			int actualCount = 0;
			int requestCount = (int)(size * nmemb);
			while (true) {
				var stream = streamMap[handle];
				int read = stream.Read(block, 0, Math.Min(block.Length, requestCount - actualCount));
				if (read == 0)
					break;
				Marshal.Copy(block, 0, (IntPtr)(buffer.ToInt64() + actualCount), read);
				actualCount += read;
			}
			return (uint)actualCount;
		}

#if iOS
		[MonoPInvokeCallback(typeof(Lemon.Api.TellCallback))]
#endif
		public static int OggTell(int handle)
		{
			var stream = streamMap[handle];
			return (int)stream.Position;
		}

#if iOS
		[MonoPInvokeCallback(typeof(Lemon.Api.SeekCallback))]
#endif
		public static int OggSeek(int handle, long offset,SeekOrigin whence)
		{
			var stream = streamMap[handle];
			return (int)stream.Seek(offset, whence);
		}

#if iOS
		[MonoPInvokeCallback(typeof(Lemon.Api.CloseCallback))]
#endif
		public static int OggClose(int handle)
		{
			var stream = streamMap[handle];
			stream.Close();
			return 0;
		}

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					if (stream != null) {
						stream.Dispose();
					}
				}

				if (oggFile != IntPtr.Zero) {
					Lemon.Api.OggDispose(oggFile);
					oggFile = IntPtr.Zero;
				}
				if (handle != 0) {
					streamMap.Release(handle);
					handle = 0;
				}

				disposedValue = true;
			}
		}

		~OggDecoder()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void ThrowIfDisposed()
		{
			if (disposedValue) {
				throw new ObjectDisposedException(GetType().Name);
			}
		}
		#endregion
	}

	class StreamMap
	{
		Stream[] map = new Stream[64];

		public int Allocate(Stream stream)
		{
			lock (map) {
				for (int i = 1; i < map.Length; i++) {
					if (map[i] == null) {
						map[i] = stream;
						return i;
					}
				}
			}
			throw new Lime.Exception("Too many opened streams");
		}

		public void Release(int slot)
		{
			lock (map) {
				map[slot] = null;
			}
		}

		public int GetCurrentStreamsCount()
		{
			int count = 0;
			lock (map) {
				for (int i = 1; i < map.Length; i++) {
					if (map[i] != null) {
						count++;
					}
				}
			}
			return count;
		}

		public Stream this[int slot] {
			get {
				lock (map) {
					return map[slot];
				}
			}
		}
	}
}
#endif
