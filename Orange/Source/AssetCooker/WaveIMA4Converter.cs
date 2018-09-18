using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;
using System.Runtime.InteropServices;

namespace Orange
{
	public class WaveIMA4Converter
	{
		public enum WaveFormat
		{
			Unknown,
			PCM,
			ADPCM,
			IMA_ADPCM = 0x11
		}

		public static byte[] Decode(IAudioDecoder input)
		{
			const int bufferSize = 10 * 1024;
			IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
			byte[] decodedSound = new byte[1024 * 64];
			try {
				int blockSize = input.GetBlockSize();
				int needToRead = bufferSize / blockSize;
				int totalRead = 0;
				while (true) {
					int actuallyRead = input.ReadBlocks(buffer, 0, needToRead);
					if (actuallyRead == 0) {
						break;
					}
					totalRead += actuallyRead;
					if (totalRead * blockSize > decodedSound.Length) {
						Array.Resize(ref decodedSound, totalRead * blockSize * 3 / 2);
					}
					Marshal.Copy(buffer, decodedSound, (totalRead - actuallyRead) * blockSize, actuallyRead * blockSize);
				}
				Array.Resize(ref decodedSound, totalRead * blockSize);
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
			return decodedSound;
		}

		public static void Encode(IAudioDecoder input, Stream output)
		{
			byte[] decodedSound = Decode(input);
			int channels = input.GetFormat() == AudioFormat.Stereo16 ? 2 : 1;
			int blockSize = channels * 1024;
			var ima4Encoder = new Ima4Encoder();
			var adpcmSound = ima4Encoder.Encode(decodedSound, channels, blockSize);
			long numFrames = (decodedSound.Length / channels / 2);
			int frequency = input.GetFrequency();
			int averageBytesPerSecond = (int)(adpcmSound.Length * (long)frequency / numFrames);
			int framesPerBlock = (blockSize - 4 * channels) * 8 /(4 * channels) + 1;
			var bw = new BinaryWriter(output);
			// RIFF chunk
			bw.Write(Encoding.UTF8.GetBytes("RIFF"));
			bw.Write(adpcmSound.Length + 44 - 8);
			bw.Write(Encoding.UTF8.GetBytes("WAVE"));
			// Fmt sub-chunk
			bw.Write(Encoding.UTF8.GetBytes("fmt "));
			bw.Write(20); // chunk size
			bw.Write((ushort)WaveFormat.IMA_ADPCM); // format
			bw.Write((ushort)channels);
			bw.Write(frequency);
			bw.Write(averageBytesPerSecond); // average bytes per seconds
			bw.Write((ushort)(blockSize)); // block align
			bw.Write((ushort)4); // bits per sample
			bw.Write((ushort)channels);
			bw.Write((ushort)framesPerBlock);
			// Data sub-chunk
			bw.Write(Encoding.UTF8.GetBytes("data"));
			bw.Write(adpcmSound.Length);
			// Write down the data
			output.Write(adpcmSound, 0, adpcmSound.Length);
		}

		struct Ima4Encoder
		{
			byte[] input;
			byte[] output;
			int inputPosition;
			int outputPosition;
			int framesPerBlock;
			int blockSize;
			int channels;
			EncoderState[] states;

			public byte[] Encode(byte[] input, int channels, int blockSize)
			{
				this.input = input;
				this.blockSize = blockSize;
				this.channels = channels;
				inputPosition = 0;
				states = new EncoderState[channels];
				framesPerBlock = (blockSize - 4 * channels) * 8 / (4 * channels) + 1;
				int numBlocks = input.Length / framesPerBlock / (2 * channels);
				output = new byte[numBlocks * blockSize];
				for (int i = 0; i < numBlocks; i++) {
					EncodeBlock();
				}
				return output;
			}

			short ReadSample()
			{
				if (inputPosition >= input.Length) {
					return 0;
				} else {
					return (short)(input[inputPosition++] | (input[inputPosition++] << 8));
				}
			}

			void EncodeBlock()
			{
				int p = outputPosition;
				for (int i = 0; i < channels; i++) {
					short sample0 = ReadSample();
					inputPosition += (channels - 1) * 2;
					short sample1 = ReadSample();
					inputPosition -= ((channels - 1) * 2 + 2);
					// Using number of iterations to calculate initial stepIndex
					var state = states[i];
					state.StepIndex = 0;
					for (int j = 0; j < 100; j++) {
						state.PrevSample = sample0;
						state.CompressSample(sample1);
					}
					state.PrevSample = sample0;
					output[p++] = (byte)((ushort)sample0 & 255);
					output[p++] = (byte)((ushort)sample0 >> 8);
					output[p++] = (byte)state.StepIndex;
					output[p++] = 0;
					states[i] = state;
				}
				short[,] samples = new short[2, 8];
				for (int i = 0; i < framesPerBlock - 1; i += 8) {
					for (int j = 0; j < 8; j++) {
						for (int c = 0; c < channels; c++) {
							samples[c, j] = ReadSample();
						}
					}
					for (int c = 0; c < channels; c++) {
						for (int j = 0; j < 4; j++) {
							int a = states[c].CompressSample(samples[c, j * 2]);
							int b = states[c].CompressSample(samples[c, j * 2 + 1]);
							output[p++] = (byte)((a & 0xf) | ((b << 4) & 0xf0));
						}
					}
				}
				outputPosition = p;
			}
		}

		struct EncoderState
		{
			public int StepIndex;
			public int PrevSample;

			static int[] StepTable = new[]
			{
				7, 8, 9, 10, 11, 12, 13, 14,
				16, 17, 19, 21, 23, 25, 28, 31,
				34, 37, 41, 45, 50, 55, 60, 66,
				73, 80, 88, 97, 107, 118, 130, 143,
				157, 173, 190, 209, 230, 253, 279, 307,
				337, 371, 408, 449, 494, 544, 598, 658,
				724, 796, 876, 963, 1060, 1166, 1282, 1411,
				1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
				3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
				7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
				15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
				32767
			};
			
			static int[] IndexTable = new[]
			{
				-1, -1, -1, -1, 2, 4, 6, 8,
				-1, -1, -1, -1, 2, 4, 6, 8
			};

			public byte CompressSample(short sample)
			{
				int sign = 0; // sign bit of the nibble(MSB)
				int delta = sample - PrevSample;
				if (delta < 0) {
					sign = 1;
					delta = -delta;
				}
				int stepIndex = StepIndex;
				int nibble = (delta * 4) / StepTable[stepIndex];
				if (nibble > 7)
					nibble = 7;
				int predictedDelta = ((StepTable[stepIndex] * nibble) / 4) + (StepTable[stepIndex] / 8);
				if (sign != 0)
					PrevSample -= predictedDelta;
				else
					PrevSample += predictedDelta;
				stepIndex += IndexTable[nibble];
				if (stepIndex < 0)
					stepIndex = 0;
				if (stepIndex > 88)
					stepIndex = 88;
				// what the decoder will find
				if (PrevSample > short.MaxValue)
					PrevSample = short.MaxValue;
				if (PrevSample < short.MinValue)
					PrevSample = short.MinValue;
				nibble += sign * 8;
				// save back
				StepIndex = stepIndex;
				return (byte)nibble;
			}
		}
	}
}
