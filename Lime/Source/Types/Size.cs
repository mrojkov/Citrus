using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of width and height.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[YuzuCompact]
	public struct Size : IEquatable<Size>
	{
		[YuzuMember("0")]
		public int Width;

		[YuzuMember("1")]
		public int Height;

		/// <summary>
		/// Returns a size with zero width and height.
		/// </summary>
		public static readonly Size Zero = new Size(0, 0);

		public Size(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public static explicit operator Vector2(Size size)
		{
			return new Vector2(size.Width, size.Height);
		}

		public static explicit operator IntVector2(Size size)
		{
			return new IntVector2(size.Width, size.Height);
		}

		public static bool operator ==(Size lhs, Size rhs)
		{
			return lhs.Width == rhs.Width && lhs.Height == rhs.Height;
		}

		public static bool operator !=(Size lhs, Size rhs)
		{
			return lhs.Width != rhs.Width || lhs.Height != rhs.Height;
		}

		public bool Equals(Size rhs)
		{
			return Width == rhs.Width && Height == rhs.Height;
		}

		public override bool Equals(object o)
		{
			return o is Size && Equals((Size)o);
		}

		public override int GetHashCode()
		{
			unchecked {
				return (Width * 397) ^ Height;
			}
		}

		/// <summary>
		/// Returns string representation of this <see cref="Size"/>
		/// in the format: "Width, Height".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}", Width, Height);
		}
	}
}
