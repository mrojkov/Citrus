#if OPENAL
using System;
using System.IO;
using System.Runtime.InteropServices;

#if !MONOMAC
using OpenTK.Audio.OpenAL;
#else
using MonoMac.OpenAL;
#endif

namespace Lime
{
	public enum AudioFormat
	{
		Stereo16,
		Mono16
	}
	
	public interface IAudioDecoder : IDisposable
	{
		AudioFormat GetFormat();

		int GetFrequency();

		int GetCompressedSize();

		void Rewind();

		int GetBlockSize();

		int ReadBlocks(IntPtr buffer, int startIndex, int blockCount);
	}

	public class AudioDecoderFactory
	{
		public static IAudioDecoder CreateDecoder(Stream stream)
		{
			if (WaveIMA4Decoder.IsWaveStream(stream)) {
				return new WaveIMA4Decoder(stream);
			} else if (OggDecoder.IsOggStream(stream)) {
				return new OggDecoder(stream);
			} else {
				throw new Lime.Exception("Unsupported audio format");
			}
		}
	};
}
#endif