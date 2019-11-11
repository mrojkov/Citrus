//Copyright (c) 2014-2015 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
//Licensed under the MIT License - https://raw.github.com/Robmaister/SharpFont.HarfBuzz/master/LICENSE

using System;
using System.Runtime.InteropServices;

namespace SharpFont.HarfBuzz
{
	public static partial class HB
	{
#if MAC
		private const string HarfBuzzDll = "libharfbuzz-0.dylib";
#else
		private const string HarfBuzzDll = "libharfbuzz-0.dll";
#endif
		private const CallingConvention CallConvention = CallingConvention.Cdecl;

#region hb-ft
		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern IntPtr hb_ft_font_create(IntPtr ft_face, IntPtr destroy);

#endregion

#region hb-version

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern void hb_version(out uint major, out uint minor, out uint micro);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern bool hb_version_atleast(uint major, uint minor, uint micro);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern IntPtr hb_version_string();

#endregion

#region Unfinished Entry Points

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern void hb_buffer_add_utf8(IntPtr buffer, byte[] text, int text_length, int item_offset, int item_length);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern IntPtr hb_buffer_create();

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern IntPtr hb_buffer_get_glyph_infos(IntPtr buf, out int length);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern IntPtr hb_buffer_get_glyph_positions(IntPtr buf, out int length);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern int hb_buffer_get_length(IntPtr buf);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern void hb_buffer_set_direction(IntPtr buffer, Direction direction);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern void hb_buffer_set_script(IntPtr ptr, Script script);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern int hb_font_get_glyph_h_kerning(IntPtr font, uint left_glyph, uint right_glyph);

		[DllImport(HarfBuzzDll, CallingConvention = CallConvention)]
		internal static extern void hb_shape(IntPtr font, IntPtr buffer, IntPtr features, int num_features);

#endregion
	}
}
