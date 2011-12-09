using System;
using OpenTK.Audio.OpenAL;
using System.IO;

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

	public class AudioDecoderFactory
	{
		public static IAudioDecoder CreateDecoder (Stream stream)
		{
			if (WaveIMA4Decoder.IsWaveStream (stream)) {
				return new WaveIMA4Decoder (stream);
			} else if (OggDecoder.IsOggStream (stream)) {
				return new OggDecoder (stream);
			} else {
				throw new Lime.Exception ("Unknown audio format");
			}
		}
	};
}
