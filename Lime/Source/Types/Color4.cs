using System;
using ProtoBuf;
using System.Runtime.InteropServices;

namespace Lime
{
	[ProtoContract]
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
		
		[ProtoMember(1)]
		[FieldOffset(0)]
		public UInt32 ABGR;
		
		public static readonly Color4 Red = new Color4(255, 0, 0, 255);
		public static readonly Color4 Green = new Color4(0, 255, 0, 255);
		public static readonly Color4 Blue = new Color4(0, 0, 255, 255);
		public static readonly Color4 White = new Color4(255, 255, 255, 255);
		public static readonly Color4 Black = new Color4(0, 0, 0, 255);
		public static readonly Color4 Gray = new Color4(128, 128, 128, 255);
		public static readonly Color4 Transparent = new Color4(255, 255, 255, 0);

		public Color4(UInt32 abgr)
		{
			R = G = B = A = 0;
			ABGR = abgr;
		}

		public Color4(byte r, byte g, byte b, byte a)
		{
			ABGR = 0;
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public static Color4 operator *(Color4 lhs, Color4 rhs)
		{
			if (lhs.ABGR == 0xFFFFFFFF)
				return rhs;
			if (rhs.ABGR == 0xFFFFFFFF)
				return lhs;
			Color4 c = new Color4();
			c.R = (byte)((rhs.R *((lhs.R << 8) + lhs.R) + 255) >> 16);
			c.G = (byte)((rhs.G *((lhs.G << 8) + lhs.G) + 255) >> 16);
			c.B = (byte)((rhs.B *((lhs.B << 8) + lhs.B) + 255) >> 16);
			c.A = (byte)((rhs.A *((lhs.A << 8) + lhs.A) + 255) >> 16);
			return c;
		}
		
		public static Color4 PremulAlpha(Color4 color)
		{
			int a = color.A;
			if (a < 255) {
				a = (a << 8) + a;
				color.R = (byte)((color.R * a + 255) >> 16);
				color.G = (byte)((color.G * a + 255) >> 16);
				color.B = (byte)((color.B * a + 255) >> 16); 
			}
			return color;
		}

		public static Color4 Lerp(Color4 a, Color4 b, float t)
		{
			if (a.ABGR == b.ABGR)
				return a;
			int x, z;
			x = (int)(t * 255);
			x = (x < 0) ? 0 :((x > 255) ? 255 : x);
			Color4 r = new Color4();
			z = (a.R << 8) - a.R + (b.R - a.R) * x;
			r.R = (byte)(((z << 8) + z + 255) >> 16);
			z = (a.G << 8) - a.G + (b.G - a.G) * x;
			r.G = (byte)(((z << 8) + z + 255) >> 16);
			z = (a.B << 8) - a.B + (b.B - a.B) * x;
			r.B = (byte)(((z << 8) + z + 255) >> 16);
			z = (a.A << 8) - a.A + (b.A - a.A) * x;
			r.A = (byte)(((z << 8) + z + 255) >> 16);
			return r;
		}

		public bool Equals(Color4 other)
		{
			return ABGR == other.ABGR;
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", R, G, B, A);
		}
	}
}