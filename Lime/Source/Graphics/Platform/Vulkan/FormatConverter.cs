using System;

namespace Lime.Graphics.Platform.Vulkan
{
	internal static unsafe class FormatConverter
	{
		private static ColorF Decode_R8_UNorm(IntPtr data)
		{
			var p = (byte*)data;
			ColorF color;
			color.R = p[0] / 255.0f;
			color.G = 0.0f;
			color.B = 0.0f;
			color.A = 1.0f;
			return color;
		}

		private static ColorF Decode_R8G8_UNorm(IntPtr data)
		{
			var p = (byte*)data;
			ColorF color;
			color.R = p[0] / 255.0f;
			color.G = p[1] / 255.0f;
			color.B = 0.0f;
			color.A = 1.0f;
			return color;
		}

		private static ColorF Decode_R8G8B8_UNorm(IntPtr data)
		{
			var p = (byte*)data;
			ColorF color;
			color.R = p[0] / 255.0f;
			color.G = p[1] / 255.0f;
			color.B = p[2] / 255.0f;
			color.A = 1.0f;
			return color;
		}

		private static ColorF Decode_R8G8B8A8_UNorm(IntPtr data)
		{
			var p = (byte*)data;
			ColorF color;
			color.R = p[0] / 255.0f;
			color.G = p[1] / 255.0f;
			color.B = p[2] / 255.0f;
			color.A = p[3] / 255.0f;
			return color;
		}

		private static ColorF Decode_R5G6B5_UNorm_Pack16(IntPtr data)
		{
			var packed = *(ushort*)data;
			ColorF color;
			color.R = ((packed >> 11) & 31) / 31.0f;
			color.G = ((packed >> 5) & 63) / 63.0f;
			color.B = (packed & 31) / 31.0f;
			color.A = 1.0f;
			return color;
		}

		private static ColorF Decode_R5G5B5A1_UNorm_Pack16(IntPtr data)
		{
			var packed = *(ushort*)data;
			ColorF color;
			color.R = ((packed >> 11) & 31) / 31.0f;
			color.G = ((packed >> 6) & 31) / 31.0f;
			color.B = ((packed >> 1) & 31) / 31.0f;
			color.A = packed & 1;
			return color;
		}

		private static ColorF Decode_R4G4B4A4_UNorm_Pack16(IntPtr data)
		{
			var packed = *(ushort*)data;
			ColorF color;
			color.R = ((packed >> 12) & 15) / 15.0f;
			color.G = ((packed >> 8) & 15) / 15.0f;
			color.B = ((packed >> 4) & 15) / 15.0f;
			color.A = (packed & 15) / 15.0f;
			return color;
		}

		private static void Encode_R8_UNorm(IntPtr data, ColorF color)
		{
			var p = (byte*)data;
			p[0] = (byte)(color.R * 255);
		}

		private static void Encode_R8G8_UNorm(IntPtr data, ColorF color)
		{
			var p = (byte*)data;
			p[0] = (byte)(color.R * 255);
			p[1] = (byte)(color.G * 255);
		}

		private static void Encode_R8G8B8_UNorm(IntPtr data, ColorF color)
		{
			var p = (byte*)data;
			p[0] = (byte)(color.R * 255);
			p[1] = (byte)(color.G * 255);
			p[2] = (byte)(color.B * 255);
		}

		private static void Encode_R8G8B8A8_UNorm(IntPtr data, ColorF color)
		{
			var p = (byte*)data;
			p[0] = (byte)(color.R * 255);
			p[1] = (byte)(color.G * 255);
			p[2] = (byte)(color.B * 255);
			p[3] = (byte)(color.A * 255);
		}

		private static void Encode_R5G6B5_UNorm_Pack16(IntPtr data, ColorF color)
		{
			*(ushort*)data = (ushort)(
				((int)(color.R * 31) << 11) |
				((int)(color.G * 63) << 5) |
				((int)(color.B * 31)));
		}

		private static void Encode_R5G5B5A1_UNorm_Pack16(IntPtr data, ColorF color)
		{
			*(ushort*)data = (ushort)(
				((int)(color.R * 31) << 11) |
				((int)(color.G * 31) << 6) |
				((int)(color.B * 31) << 1) |
				((int)(color.A)));
		}

		private static void Encode_R4G4B4A4_UNorm_Pack16(IntPtr data, ColorF color)
		{
			*(ushort*)data = (ushort)(
				((int)(color.R * 15) << 12) |
				((int)(color.G * 15) << 8) |
				((int)(color.B * 15) << 4) |
				((int)(color.A * 15)));
		}

		public static Func<IntPtr, ColorF> GetDecoder(Format format)
		{
			switch (format) {
				case Format.R8_UNorm:
					return Decode_R8_UNorm;
				case Format.R8G8_UNorm:
					return Decode_R8G8_UNorm;
				case Format.R8G8B8_UNorm:
					return Decode_R8G8B8_UNorm;
				case Format.R8G8B8A8_UNorm:
					return Decode_R8G8B8A8_UNorm;
				case Format.R5G6B5_UNorm_Pack16:
					return Decode_R5G6B5_UNorm_Pack16;
				case Format.R5G5B5A1_UNorm_Pack16:
					return Decode_R5G5B5A1_UNorm_Pack16;
				case Format.R4G4B4A4_UNorm_Pack16:
					return Decode_R4G4B4A4_UNorm_Pack16;
				default:
					throw new NotSupportedException();
			}
		}

		public static Action<IntPtr, ColorF> GetEncoder(Format format)
		{
			switch (format) {
				case Format.R8_UNorm:
					return Encode_R8_UNorm;
				case Format.R8G8_UNorm:
					return Encode_R8G8_UNorm;
				case Format.R8G8B8_UNorm:
					return Encode_R8G8B8_UNorm;
				case Format.R8G8B8A8_UNorm:
					return Encode_R8G8B8A8_UNorm;
				case Format.R5G6B5_UNorm_Pack16:
					return Encode_R5G6B5_UNorm_Pack16;
				case Format.R5G5B5A1_UNorm_Pack16:
					return Encode_R5G5B5A1_UNorm_Pack16;
				case Format.R4G4B4A4_UNorm_Pack16:
					return Encode_R4G4B4A4_UNorm_Pack16;
				default:
					throw new NotSupportedException();
			}
		}
	}

	internal struct ColorF
	{
		public float R;
		public float G;
		public float B;
		public float A;
	}
}
