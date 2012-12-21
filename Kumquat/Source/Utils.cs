using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Kumquat
{
	public class Utils
	{
		public static void Reparent(Widget widget, Widget parent)
		{
			widget.Basis = widget.CalcBasisInSpaceOf(parent);
			
			if (widget.Parent != null)
				widget.Unlink();

			parent.Nodes.Insert(0, widget);
		}

		public static int Clamp(int value, int min, int max)
		{
			return Math.Min(Math.Max(value, min), max);
		}

		public static bool IsCloseUp(string locationName)
		{
			return locationName[3] != '_';
		}

		public static string[] GetLocations()
		{
			var prefix = "Location";
			var arr = AssetsBundle.Instance.EnumerateFiles();
			arr = arr.Where(x =>
				x.Substring(0, prefix.Length) == prefix &&
				Path.GetExtension(x) == ".scene"
			).ToArray();
			return arr;
		}

	}
}