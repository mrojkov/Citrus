using System;
using System.IO;
using csvorbis;
using OpenTK.Audio.OpenAL;

namespace Lime
{
	public interface IAudioDecoder : IDisposable
	{
		ALFormat Format { get; }
		int Frequency { get; }
		float TotalTime { get; }
		/// <summary>
		/// Total length of audio clip in samples
		/// </summary>
		int TotalLength { get; }
		int CompressedSize { get; }
		float CurrentTime { get; }
		int CurrentPosition { get; }
		int BytesPerSample { get; }
		int CurrentCompressedPosition { get; }
		bool SeekingSupported { get; }
		int ReadAudioData (Byte [] output, int amount);
		bool SetPosition (int position);
		bool Seek(float seconds, bool relative);
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

		Stream stream;
		VorbisFile vorbis;
		VorbisFileInstance instance;
		Info info;
	}
}
