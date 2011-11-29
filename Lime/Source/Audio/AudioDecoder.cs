using System;
using System.IO;
using csvorbis;
using OpenTK.Audio.OpenAL;
#if iOS
using MonoTouch.AudioToolbox;
#elif MAC
using MonoMac.AudioToolbox;
#endif
using System.Runtime.InteropServices;

namespace Lime
{
	public interface IAudioDecoder : IDisposable
	{
		ALFormat Format { get; }
		int Frequency { get; }
		//float TotalTime { get; }
		/// <summary>
		/// Total length of audio clip in samples
		/// </summary>
		//int TotalLength { get; }
		int CompressedSize { get; }
		//float CurrentTime { get; }
		//int CurrentPosition { get; }
		//int BytesPerSample { get; }
		//int CurrentCompressedPosition { get; }
		//bool SeekingSupported { get; }
		int ReadAudioData (Byte [] output, int amount);
		bool Reset ();
		//bool SetPosition (int position);
		//bool Seek(float seconds, bool relative);
	}

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

	public class WavDecoder : IAudioDecoder
	{
		IMA_ADPCM source;

		public WavDecoder (Stream stream)
		{
			source = new IMA_ADPCM (stream);
		}
		
		public bool Reset ()
		{
			source.Seek (0, SeekOrigin.Begin);
			return true;
		}

		public int ReadAudioData (byte[] output, int amount)
		{
			int read = source.Read (output, 0, amount);
			return read;
		}
		
		public int BytesPerSample {
			get {
				return 2;
			}
		}

		public int CompressedSize {
			get {
				return (int)source.Length;
			}
		}

		public ALFormat Format {
			get {
				return source.Channels == 2 ? ALFormat.Mono16 : ALFormat.Mono8;
			}
		}

		public int Frequency {
			get {
				int frequency = source.SamplesPerSec * 2;
				return frequency;
			}
		}

		void IDisposable.Dispose ()
		{
			source.Dispose ();
		}
	}
	

	public class OggDecoder : IAudioDecoder
	{
		public OggDecoder (Stream stream)
		{
			this.stream = stream;
			vorbis = new VorbisFile (stream);
			instance = vorbis.makeInstance ();
			info = vorbis.getInfo () [0];
		}

		void IDisposable.Dispose ()
		{
			vorbis.Dispose ();
		}

		public float TotalTime { get { return vorbis.time_total (-1); } }

		public int TotalLength { get { return (int)vorbis.pcm_total (-1); } }

		public int CompressedSize { get { return (int)vorbis.raw_total (-1); } }

		public float CurrentTime { get { return instance.time_tell (); } }

		public int CurrentPosition { get { return (int)instance.pcm_tell (); } }

		public int CurrentCompressedPosition { get { return (int)instance.pcm_tell (); } }

		public int BytesPerSample { get { return 2; } }

		public int Channels { get { return info.channels; } }

		public ALFormat Format 
		{
			get {
				if (info.channels == 1)
					return ALFormat.Mono16;
				else
					return ALFormat.Stereo16;
			}
		}

		public bool SeekingSupported { get { return vorbis.seekable (); } }

		public int Frequency { get { return info.rate; } }

		public int ReadAudioData (Byte [] output, int amount)
		{
			return instance.read (output, amount, 0, 2, 1, null);
		}

		public bool SetPosition (int position)
		{
			if (vorbis.seekable ()) {
				return instance.raw_seek (position) == 0;
			}
			return false;
		}

		public bool Seek (float seconds, bool relative)
		{
			if (vorbis.seekable ()) {
				return instance.time_seek (seconds) == 0;
			}
			return false;
		}
		
		public bool Reset ()
		{
			return instance.raw_seek (0) == 0;
		}

		Stream stream;
		VorbisFile vorbis;
		VorbisFileInstance instance;
		Info info;
	}
}
