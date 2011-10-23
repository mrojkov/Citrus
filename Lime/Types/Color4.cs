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
		
		public static readonly Color4 Red = new Color4 (255, 0, 0, 255);
		public static readonly Color4 Green = new Color4 (0, 255, 0, 255);
		public static readonly Color4 Blue = new Color4 (0, 0, 255, 255);
		public static readonly Color4 White = new Color4 (255, 255, 255, 255);
		public static readonly Color4 Black = new Color4 (0, 0, 0, 255);

		public Color4 (UInt32 abgr)
		{
			R = G = B = A = 0;
			ABGR = abgr;
		}
		
		public Color4 (byte r, byte g, byte b, byte a)
		{
			ABGR = 0;
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public static Color4 operator * (Color4 lhs, Color4 rhs)
		{
			if (lhs.ABGR == 0xFFFFFFFF)
				return rhs;
			if (rhs.ABGR == 0xFFFFFFFF)
				return lhs;
			Color4 r = new Color4 ();
			r.R = (byte)(((int)lhs.R * rhs.R) / 255);
			r.G = (byte)(((int)lhs.G * rhs.G) / 255);
			r.B = (byte)(((int)lhs.B * rhs.B) / 255);
			r.A = (byte)(((int)lhs.A * rhs.A) / 255);
			return r;
		}
		
		public static Color4 PremulAlpha (Color4 color)
		{
			if (color.A < 255) {
				Color4 t = color;
				color.R = (t.R == 255) ? t.A : (byte)(t.R * t.A / 255);
				color.G = (t.G == 255) ? t.A : (byte)(t.G * t.A / 255);
				color.B = (t.B == 255) ? t.A : (byte)(t.B * t.A / 255);
			}
			return color;
		}

		public static Color4 Lerp (Color4 a, Color4 b, float t)
		{
			if (a.ABGR == b.ABGR)
				return a;
			t = (t < 0) ? 0 : ((t > 1) ? 1 : t);
			int y = (int)(t * 255);
			int x = 255 - y;
			Color4 r = new Color4 ();
			r.R = (byte)((a.R * x + b.R * y) / 255);
			r.G = (byte)((a.G * x + b.G * y) / 255);
			r.B = (byte)((a.B * x + b.B * y) / 255);
			r.A = (byte)((a.A * x + b.A * y) / 255);
			return r;
		}
        
		public bool Equals (Color4 other)
		{
			return ABGR == other.ABGR;
		}
	}
}