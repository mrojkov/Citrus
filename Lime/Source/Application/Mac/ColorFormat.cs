#if MAC || MONOMAC
using System;

namespace Lime.Platform
{
	public struct ColorFormat : IComparable<ColorFormat>, IEquatable<ColorFormat>
	{
		private byte red, green, blue, alpha;

		public ColorFormat(int bpp)
		{
			if (bpp < 0) {
				throw new ArgumentOutOfRangeException("bpp", "Must be greater or equal to zero.");
			}
			red = green = blue = alpha = 0;
			BitsPerPixel = bpp;
			IsIndexed = false;
			switch (bpp) {
			case 32:
				Red = Green = Blue = Alpha = 8;
				break;
			case 24:
				Red = Green = Blue = 8;
				break;
			case 16:
				Red = Blue = 5;
				Green = 6;
				break;
			case 15:
				Red = Green = Blue = 5;
				break;
			case 8:
				Red = Green = 3;
				Blue = 2;
				IsIndexed = true;
				break;
			case 4:
				Red = Green = 2;
				Blue = 1;
				IsIndexed = true;
				break;
			case 1:
				IsIndexed = true;
				break;
			default:
				Red = Blue = Alpha = (byte)(bpp / 4);
				Green = (byte)((bpp / 4) + (bpp % 4));
				break;
			}
		}

		public ColorFormat(int red, int green, int blue, int alpha)
		{
			if (red < 0 || green < 0 || blue < 0 || alpha < 0) {
				throw new ArgumentOutOfRangeException("Arguments must be greater or equal to zero.");
			}
			this.red = (byte)red;
			this.green = (byte)green;
			this.blue = (byte)blue;
			this.alpha = (byte)alpha;
			this.BitsPerPixel = red + green + blue + alpha;
			this.IsIndexed = false;
			this.IsIndexed |= this.BitsPerPixel < 15 && this.BitsPerPixel != 0;
		}

		public int Red { get { return red; } private set { red = (byte)value; } }

		public int Green { get { return green; } private set { green = (byte)value; } }

		public int Blue { get { return blue; } private set { blue = (byte)value; } }

		public int Alpha { get { return alpha; } private set { alpha = (byte)value; } }

		public bool IsIndexed { get; private set; }

		public int BitsPerPixel { get; private set; }

		public static readonly ColorFormat Empty = new ColorFormat(0);

		public static implicit operator ColorFormat(int bpp)
		{
			return new ColorFormat(bpp);
		}

		public int CompareTo(ColorFormat other)
		{
			int result = BitsPerPixel.CompareTo(other.BitsPerPixel);
			if (result != 0)
				return result;
			result = IsIndexed.CompareTo(other.IsIndexed);
			if (result != 0)
				return result;
			result = Alpha.CompareTo(other.Alpha);
			return result;
		}

		public bool Equals(ColorFormat other)
		{
			return Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha;
		}

		public override bool Equals(object obj)
		{
			return (obj is ColorFormat) && this.Equals((ColorFormat)obj);
		}

		public static bool operator ==(ColorFormat left, ColorFormat right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorFormat left, ColorFormat right)
		{
			return !(left == right);
		}

		public static bool operator >(ColorFormat left, ColorFormat right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator >=(ColorFormat left, ColorFormat right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static bool operator <(ColorFormat left, ColorFormat right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator <=(ColorFormat left, ColorFormat right)
		{
			return left.CompareTo(right) <= 0;
		}

		public override int GetHashCode()
		{
			return Red ^ Green ^ Blue ^ Alpha;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", BitsPerPixel, (IsIndexed ? " indexed" : Red.ToString() + Green.ToString() + Blue.ToString() + Alpha.ToString()));
		}
	}
}
#endif

