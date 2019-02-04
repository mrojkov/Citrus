using System;
using System.Runtime.InteropServices;

namespace Lime
{
	internal unsafe class Etc2Decoder
	{
		const Int32 ETC2_MODE_ALLOWED_ALL = 0x1F;

		const string Dll = "Etc2Decoder";

		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		static extern Int32 draw_block4x4_etc2_eac(byte* bitstring, uint* image_buffer, Int32 flags);

		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		static extern Int32 draw_block4x4_etc2_rgb8(byte* bitstring, uint* image_buffer, Int32 flags);

		public static void Decode(byte[] etcData, IntPtr rgba8Data, int width, int height, Format format)
		{
			if (format != Format.ETC2_R8G8B8A8_UNorm_Block &&
				format != Format.ETC2_R8G8B8_UNorm_Block &&
				format != Format.ETC1_R8G8B8_UNorm_Block
			) {
				throw new ArgumentException("Invalid format");
			}
			if ((width & 3) != 0 || (height & 3) != 0) {
				throw new ArgumentException("Texture dimensions should be multiple of 4");
			}
			var bytesPerBlock = format.GetSize();
			uint* decodedBlock = stackalloc uint[16];  
			fixed (byte* fixedEtcData = &etcData[0]) {
				var etcDataPtr = (IntPtr)fixedEtcData;
				for (int i = 0; i < height / 4; i++) {
					for (int j = 0; j < width / 4; j++) {
						if (format == Format.ETC2_R8G8B8A8_UNorm_Block) {
							draw_block4x4_etc2_eac((byte*)etcDataPtr, decodedBlock, ETC2_MODE_ALLOWED_ALL);
						} else {
							draw_block4x4_etc2_rgb8((byte*)etcDataPtr, decodedBlock, ETC2_MODE_ALLOWED_ALL);
						}
						int t = 0;
						uint* rgba8DataPtr = (uint*)rgba8Data + (i * 4 * width + j * 4);
						for (int y = 0; y < 4; y++) {
							*rgba8DataPtr++ = decodedBlock[t++];
							*rgba8DataPtr++ = decodedBlock[t++];
							*rgba8DataPtr++ = decodedBlock[t++];
							*rgba8DataPtr++ = decodedBlock[t++];
							rgba8DataPtr += width - 4;
						}
						etcDataPtr += bytesPerBlock;
					}
				}
			}
		}
	}
}
