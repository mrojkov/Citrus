using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus.GtkBackend
{
	public class AcceleratorConverter
	{
		public static Gtk.AccelKey FromString(string accel)
		{
			var result = new Gtk.AccelKey() {
				AccelFlags = Gtk.AccelFlags.Visible
			};
			foreach (var key in accel.Split('+')) {
				if (key == "Ctrl") {
					result.AccelMods |= Gdk.ModifierType.ControlMask;
				} else if (key == "Shift") {
					result.AccelMods |= Gdk.ModifierType.ShiftMask;
				} else if (key == "Alt") {
					result.AccelMods |= Gdk.ModifierType.Mod1Mask;
				} else {
					uint k;
					Gdk.ModifierType m;
					Gtk.Accelerator.Parse(key, out k, out m);
					result.Key = (Gdk.Key)k;
					if (result.Key == Gdk.Key.VoidSymbol) {
						throw new ArgumentException("Invalid accelerator: {0}", accel);
					}
				}
			}
			return result;
		}
	}
}
