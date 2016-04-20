using System;
using Lime;
using ProtoBuf;

namespace EmptyProject.Application
{
	[ProtoContract]
	public struct Display : IEquatable<Display>
	{
		[ProtoMember(1)]
		public string Name;

		[ProtoMember(2)]
		public IntVector2 Resolution;

		[ProtoMember(3)]
		public int DPI;

		public Display(string name, int width, int height, int dpi)
			: this()
		{
			Name = name;
			Resolution = new IntVector2(width, height);
			DPI = dpi;
		}

		public bool Equals(Display other)
		{
			return other.Name == Name;
		}
		public Vector2 PhysicalSize
		{
			get { return (Vector2)Resolution / DPI; }
		}

		public bool IsTablet
		{
			get
			{
				// Treat all devices with extra-large screens as tablets
				// http://developer.android.com/guide/practices/screens_support.html
				var ps = PhysicalSize;
				if (ps.Y > ps.X)
				{
					Toolbox.Swap(ref ps.Y, ref ps.X);
				}
				return ps.X >= 6 && ps.Y >= 4.5f;
			}
		}
	}
}