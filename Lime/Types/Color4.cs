using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	[System.Diagnostics.DebuggerStepThrough]
	public struct Color4 : IEquatable<Color4>
	{		
		// OpenGL order of color components
		[ProtoMember(1)]
		public byte R;

		[ProtoMember(2)]
		public byte G;
		
		[ProtoMember(3)]
		public byte B;
		
		[ProtoMember(4)]
		public byte A;
		
		public static readonly Color4 Red = new Color4 (0xFF0000FF);
		public static readonly Color4 Green = new Color4 (0x00FF00FF);
		public static readonly Color4 Blue = new Color4 (0x0000FFFF);
		public static readonly Color4 White = new Color4 (0xFFFFFFFF);
		public static readonly Color4 Black = new Color4 (0x000000FF);

		public Color4 (UInt32 rgba)
		{
			A = (byte)(rgba & 0xFF);
			B = (byte)((rgba >> 8) & 0xFF);
			G = (byte)((rgba >> 16) & 0xFF);
			R = (byte)((rgba >> 24) & 0xFF);
		}
		
		public Color4 (byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public static Color4 operator * (Color4 lhs, Color4 rhs)
		{
			if (lhs.Equals (White))
				return rhs;
			if (rhs.Equals (White))
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
			if (a.Equals (b))
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
			return A == other.A && R == other.R && G == other.G && B == other.B;
		}

	}
}