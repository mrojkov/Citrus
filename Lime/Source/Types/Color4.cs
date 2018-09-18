using System;
using System.Runtime.InteropServices;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of 4-byte color (RGBA).
	/// </summary>
	[YuzuCompact]
	[System.Diagnostics.DebuggerStepThrough]
	[StructLayout(LayoutKind.Explicit)]
	public struct Color4 : IEquatable<Color4>
	{
		[FieldOffset(0)]
		public byte R;

		[FieldOffset(1)]
		public byte G;

		[FieldOffset(2)]
		public byte B;

		[FieldOffset(3)]
		public byte A;

		[YuzuMember("0")]
		[FieldOffset(0)]
		public uint ABGR;

		public static readonly Color4 Red = new Color4(255, 0, 0, 255);
		public static readonly Color4 Green = new Color4(0, 255, 0, 255);
		public static readonly Color4 Blue = new Color4(0, 0, 255, 255);
		public static readonly Color4 White = new Color4(255, 255, 255, 255);
		public static readonly Color4 Black = new Color4(0, 0, 0, 255);
		public static readonly Color4 Gray = new Color4(128, 128, 128, 255);
		public static readonly Color4 DarkGray = new Color4(64, 64, 64, 255);
		public static readonly Color4 Yellow = new Color4(255, 255, 0, 255);
		public static readonly Color4 Orange = new Color4(255, 128, 0, 255);
		public static readonly Color4 Transparent = new Color4(255, 255, 255, 0);
		public static readonly Color4 Zero = new Color4(0, 0, 0, 0);

		public Color4(uint abgr)
		{
			R = G = B = A = 0;
			ABGR = abgr;
		}

		public Color4(byte r, byte g, byte b, byte a = 255)
		{
			ABGR = 0;
			R = r;
			G = g;
			B = b;
			A = a;
		}

		/// <summary>
		/// Darken the color with the specified amount.
		/// Amount 1 turns the color into black,
		/// Amount 0 remains the color unchanged.
		/// </summary>
		public Color4 Darken(float amount)
		{
			var c = Black;
			c.A = A;
			return Lerp(amount, this, c);
		}

		/// <summary>
		/// Lighten the color with the specified amount.
		/// Amount 1 turns the color into white,
		/// Amount 0 remains the color unchanged.
		/// </summary>
		public Color4 Lighten(float amount)
		{
			var c = White;
			c.A = A;
			return Lerp(amount, this, c);
		}

		/// <summary>
		/// Change the color transparency with the specified amount.
		/// Amount 1 turns the color into fully transparent,
		/// Amount 0 remains the color unchanged.
		/// </summary>
		public Color4 Transparentify(float amount)
		{
			var c = this;
			c.A = 0;
			return Lerp(amount, this, c);
		}

		/// <summary>
		/// Gets the alpha component from a specified ARGB color-int.
		/// </summary>
		/// <param name="color">The color as integer value in ARGB format.</param>
		/// <returns>Alpha.</returns>
		public static byte GetAlphaComponent(int color)
		{
			return (byte)((uint)color >> 24);
		}

		/// <summary>
		/// Gets the red component from a specified ARGB color-int.
		/// </summary>
		/// <param name="color">The color as integer value in ARGB format.</param>
		/// <returns>Red.</returns>
		public static byte GetRedComponent(int color)
		{
			return (byte)((color >> 16) & 0xFF);
		}

		/// <summary>
		/// Gets the green component from a specified ARGB color-int.
		/// </summary>
		/// <param name="color">The color as integer value in ARGB format.</param>
		/// <returns>Green.</returns>
		public static byte GetGreenComponent(int color)
		{
			return (byte)((color >> 8) & 0xFF);
		}

		/// <summary>
		/// Gets the blue component from a specified ARGB color-int.
		/// </summary>
		/// <param name="color">The color as integer value in ARGB format.</param>
		/// <returns>Blue.</returns>
		public static byte GetBlueComponent(int color)
		{
			return (byte)(color & 0xFF);
		}

		/// <summary>
		/// Creates an ARGB color-int from a specified alpha, red, green and blue components.
		/// </summary>
		/// <param name="alpha">Alpha component of the color.</param>
		/// <param name="red">Red component of the color.</param>
		/// <param name="green">Green component of the color.</param>
		/// <param name="blue">Blue component of the color.</param>
		/// <returns>ARGB color-int.</returns>
		public static int CreateArgb(byte alpha, byte red, byte green, byte blue)
		{
			return (alpha << 24) | (red << 16) | (green << 8) | blue;
		}

		// Unity mono compiler doesn't allow another constructor because of ambiguity
		public static Color4 FromFloats(float r, float g, float b, float a = 1)
		{
			return new Color4 {
				R = (byte)(Mathf.Clamp(r, 0, 1) * 255),
				G = (byte)(Mathf.Clamp(g, 0, 1) * 255),
				B = (byte)(Mathf.Clamp(b, 0, 1) * 255),
				A = (byte)(Mathf.Clamp(a, 0, 1) * 255)
			};
		}

		/// <summary>
		/// Componentwise multiplication of two colors.
		/// </summary>
		public static Color4 operator *(Color4 lhs, Color4 rhs)
		{
			if (lhs.ABGR == 0xFFFFFFFF)
				return rhs;
			if (rhs.ABGR == 0xFFFFFFFF)
				return lhs;
			return new Color4
			{
				R = (byte) ((rhs.R * ((lhs.R << 8) + lhs.R) + 255) >> 16),
				G = (byte) ((rhs.G * ((lhs.G << 8) + lhs.G) + 255) >> 16),
				B = (byte) ((rhs.B * ((lhs.B << 8) + lhs.B) + 255) >> 16),
				A = (byte) ((rhs.A * ((lhs.A << 8) + lhs.A) + 255) >> 16)
			};
		}

		/// <summary>
		/// Multiplies every component of color with its alpha.
		/// </summary>
		public static Color4 PremulAlpha(Color4 color)
		{
			int a = color.A;
			if (a >= 255) {
				return color;
			}
			a = (a << 8) + a;
			color.R = (byte)((color.R * a + 255) >> 16);
			color.G = (byte)((color.G * a + 255) >> 16);
			color.B = (byte)((color.B * a + 255) >> 16);
			return color;
		}

		/// <summary>
		/// Creates a new <see cref="Color4"/> that contains
		/// linear interpolation of the specified colors.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first color.</param>
		/// <param name="value2">The second color.</param>
		public static Color4 Lerp(float amount, Color4 value1, Color4 value2)
		{
			if (value1.ABGR == value2.ABGR) {
				return value1;
			}
			int x, z;
			x = (int)(amount * 255);
			x = (x < 0) ? 0 : ((x > 255) ? 255 : x);
			var r = value1;

			// optimization for opacity 0 to 1 animation
			if (value1.ABGR == 0x00FFFFFF && value2.ABGR == 0xFFFFFFFF) {
				r.A = (byte)x;
				return r;
			}
			if (value1.ABGR == 0xFFFFFFFF && value2.ABGR == 0x00FFFFFF) {
				r.A = (byte)(255 - x);
				return r;
			}

			z = (value1.R << 8) - value1.R + (value2.R - value1.R) * x;
			r.R = (byte)(((z << 8) + z + 255) >> 16);
			z = (value1.G << 8) - value1.G + (value2.G - value1.G) * x;
			r.G = (byte)(((z << 8) + z + 255) >> 16);
			z = (value1.B << 8) - value1.B + (value2.B - value1.B) * x;
			r.B = (byte)(((z << 8) + z + 255) >> 16);
			z = (value1.A << 8) - value1.A + (value2.A - value1.A) * x;
			r.A = (byte)(((z << 8) + z + 255) >> 16);
			return r;
		}

		public bool Equals(Color4 other)
		{
			return ABGR == other.ABGR;
		}

		public static bool operator == (Color4 lhs, Color4 rhs)
		{
			return lhs.ABGR == rhs.ABGR;
		}

		public static bool operator != (Color4 lhs, Color4 rhs)
		{
			return lhs.ABGR != rhs.ABGR;
		}

		public Vector4 ToVector4()
		{
			return new Vector4(R / 255f, G / 255f, B / 255f, A / 255f);
		}

		public enum StringPresentation
		{
			Hex,
			Dec
		}

		public string ToString(StringPresentation presentation)
		{
			if (presentation == StringPresentation.Hex) {
				return string.Format("#{0:X2}.{1:X2}.{2:X2}.{3:X2}", R, G, B, A);
			} else {
				return string.Format("{0}. {1}. {2}. {3}", R, G, B, A);
			}
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Color4"/>
		/// in the format: "#R.G.B.A", where R, G, B, A are represented by hexademical numbers.
		/// </summary>
		public override string ToString()
		{
			return ToString(StringPresentation.Hex);
		}

		public static bool TryParse(string s, out Color4 result)
		{
			result = new Color4();
			var style = System.Globalization.NumberStyles.Integer;
			if (s.Length > 0 && s[0] == '#') {
				s = s.Substring(1);
				style = System.Globalization.NumberStyles.HexNumber;
			}
			var t = s.Split('.');
			if (t.Length != 4) {
				return false;
			}
			return
				byte.TryParse(t[0], style, null, out result.R) &&
				byte.TryParse(t[1], style, null, out result.G) &&
				byte.TryParse(t[2], style, null, out result.B) &&
				byte.TryParse(t[3], style, null, out result.A);
		}

		public static Color4 Parse(string s)
		{
			Color4 result;
			if (!TryParse(s, out result)) {
				throw new ArgumentException();
			}
			return result;
		}

		public override int GetHashCode()
		{
			return ABGR.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return (Color4)obj == this;
		}
	}
}
