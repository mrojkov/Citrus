using System;
using System.IO;
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;
#if iOS
using MonoTouch.AudioToolbox;
#elif MAC
using MonoMac.AudioToolbox;
#endif

namespace Lime
{
	public interface IAudioDecoder : IDisposable
	{
		ALFormat GetFormat ();
		int GetFrequency ();
		int GetCompressedSize ();
		void Rewind ();
		int GetBlockSize ();
		int ReadBlocks (IntPtr buffer, int startIndex, int blockCount);
	}

#if XiOS || XMAC
	class CustomAudioSource : AudioSource
	{
		static Stream streamForInitWith = null;
		
		Stream stream = streamForInitWith;
		byte [] tempBuffer = new byte [1024 * 4];
		
		public static CustomAudioSource Create (Stream stream)
		{
			streamForInitWith = stream;
			return new CustomAudioSource ();
		}
		
		CustomAudioSource ()
			: base (AudioFileType.CAF)
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			stream.Dispose ();
		}

		public override long Size {
			get {
				return stream.Length;
			}
			set {
				stream.SetLength (value);
			}
		}

		public override bool Read (long position, int requestCount, IntPtr buffer, out int actualCount)
		{
			stream.Seek (position, SeekOrigin.Begin);
			actualCount = 0;
			while (true) {
				int read = stream.Read (tempBuffer, 0, Math.Min (tempBuffer.Length, requestCount - actualCount));
				if (read == 0)
					break;
				if (read < 0)
					return false;
				Marshal.Copy (tempBuffer, 0, (IntPtr)(buffer.ToInt64 () + actualCount), read);
				actualCount += read;
			}
			return true;
		}

		public override bool Write (long position, int requestCount, IntPtr buffer, out int actualCount)
		{
			throw new NotImplementedException ();
		}
	}

	public class CAFDecoder : IAudioDecoder
	{
		CustomAudioSource source;
		int position;

		public CAFDecoder (Stream stream)
		{
			source = CustomAudioSource.Create (stream);
		}
		
		public bool Reset ()
		{
			position = 0;
			return true;
		}

		public int ReadAudioData (byte[] output, int amount)
		{
			int read = source.Read (position, output, 0, amount, true);
			position += read;
			return read;
		}
		
		public int BytesPerSample {
			get {
				return 2;
			}
		}

		public int CompressedSize {
			get {
				return (int)source.Size;
			}
		}

		public ALFormat Format {
			get {
				var channels = source.StreamBasicDescription.ChannelsPerFrame;
				var fmt = source.StreamBasicDescription.Format;
				var bpc = source.StreamBasicDescription.BitsPerChannel;
				if (fmt == AudioFormatType.AppleIMA4)
					return (channels == 2) ? ALFormat.StereoIma4Ext : ALFormat.MonoIma4Ext;
				else if (fmt == AudioFormatType.LinearPCM) {
					if (bpc == 16)
						return (channels == 2) ? ALFormat.Stereo16 : ALFormat.Mono16;
					else if (bpc == 8)
						return (channels == 2) ? ALFormat.Stereo8 : ALFormat.Mono8;
				}
				Console.WriteLine ("WARNING! Unknown audio format!");
				return ALFormat.Mono8;
			}
		}

		public int Frequency {
			get {
				int frequency = (int)source.StreamBasicDescription.SampleRate;
				return frequency;
			}
		}

		void IDisposable.Dispose ()
		{
			source.Dispose ();
		}
	}
#endif
}
