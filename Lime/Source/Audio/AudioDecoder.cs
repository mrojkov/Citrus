using System;
#if !UNITY
using OpenTK.Audio.OpenAL;
#endif
using System.IO;
using System.Runtime.InteropServices;

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

	public class PreloadingAudioDecoder : IAudioDecoder
	{
		IAudioDecoder decoder;
		bool disposed;

		static AudioCache soundCache = new AudioCache();
		static byte[] zeroBlock = new byte[256];

		public PreloadingAudioDecoder(string path)
		{
			soundCache.OpenStreamAsync(path, (stream) => {
				if (disposed) {
					stream.Dispose();
				} else {
					decoder = AudioDecoderFactory.CreateDecoder(stream);
				}
			});
		}

		public void Dispose()
		{
			disposed = true;
			if (decoder != null) {
				decoder.Dispose();
			}
		}

		public AudioFormat GetFormat()
		{
			return (decoder != null) ? decoder.GetFormat() : AudioFormat.Mono16;
		}

		public int GetFrequency()
		{
			return (decoder != null) ? decoder.GetFrequency() : 22050;
		}

		public int GetCompressedSize()
		{
			return (decoder != null) ? decoder.GetCompressedSize() : 0;
		}

		public void Rewind()
		{
			if (decoder != null) {
				decoder.Rewind();
			}
		}

		public int GetBlockSize()
		{
			return (decoder != null) ? decoder.GetBlockSize() : zeroBlock.Length;
		}

		public int ReadBlocks(IntPtr buffer, int startIndex, int blockCount)
		{
			if (decoder != null) {
				return decoder.ReadBlocks(buffer, startIndex, blockCount);
			} else {
				Marshal.Copy(zeroBlock, 0, buffer, zeroBlock.Length);
				return 1;
			}
		}
	}

	internal class AudioDecoderFactory
	{
		public static IAudioDecoder CreateDecoder(Stream stream)
		{
#if UNITY
			throw new NotImplementedException();
#else
			if (WaveIMA4Decoder.IsWaveStream(stream)) {
				return new WaveIMA4Decoder(stream);
			} else if (OggDecoder.IsOggStream(stream)) {
				return new OggDecoder(stream);
			} else {
				throw new Lime.Exception("Unsupported audio format");
			}
#endif
		}
	};
}
