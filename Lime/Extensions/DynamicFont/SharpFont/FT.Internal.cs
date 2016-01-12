#region MIT License
/*Copyright (c) 2012-2015 Robert Rouhani <robert.rouhani@gmail.com>

SharpFont based on Tao.FreeType, Copyright (c) 2003-2007 Tao Framework Team

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/
#endregion

using System;
using System.Runtime.InteropServices;

using SharpFont.Internal;

namespace SharpFont
{
	/// <content>
	/// This file contains all the raw FreeType2 function signatures.
	/// </content>
	public static partial class FT
	{
		/// <summary>
		/// Defines the location of the FreeType DLL. Update SharpFont.dll.config if you change this!
		/// </summary>
#if iOS
		private const string FreetypeDll = "__Internal";
#else
		private const string FreetypeDll = "freetype6";
#endif

		/// <summary>
		/// Defines the calling convention for P/Invoking the native freetype methods.
		/// </summary>
		private const CallingConvention CallConvention = CallingConvention.Cdecl;

		#region Core API

		#region FreeType Version

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern void FT_Library_Version(IntPtr library, out int amajor, out int aminor, out int apatch);

		#endregion

		#region Base Interface
		
		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Init_FreeType(out IntPtr alibrary);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Done_FreeType(IntPtr library);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_New_Memory_Face(IntPtr library, IntPtr file_base, int file_size, int face_index, out IntPtr aface);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Reference_Face(IntPtr face);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Done_Face(IntPtr face);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Set_Pixel_Sizes(IntPtr face, uint pixel_width, uint pixel_height);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Load_Glyph(IntPtr face, uint glyph_index, int load_flags);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Render_Glyph(IntPtr slot, RenderMode render_mode);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Get_Kerning(IntPtr face, uint left_glyph, uint right_glyph, uint kern_mode, out FTVector26Dot6 akerning);

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern uint FT_Get_Char_Index(IntPtr face, uint charcode);

		#endregion

		#region Size Management

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Done_Size(IntPtr size);

		#endregion

		#endregion

		#region Support API

		#region Bitmap Handling

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Bitmap_Done(IntPtr library, IntPtr bitmap);

		#endregion

		#region Module Management

		[DllImport(FreetypeDll, CallingConvention = CallConvention)]
		internal static extern Error FT_Done_Library(IntPtr library);

		#endregion

		#endregion
	}
}
