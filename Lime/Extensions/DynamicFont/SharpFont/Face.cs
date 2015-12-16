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
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SharpFont.Internal;

namespace SharpFont
{
	/// <summary>
	/// FreeType root face class structure. A face object models a typeface in a font file.
	/// </summary>
	/// <remarks>
	/// Fields may be changed after a call to <see cref="AttachFile"/> or <see cref="AttachStream"/>.
	/// </remarks>
	public sealed class Face : IDisposable
	{
		#region Fields

		private IntPtr reference;
		private FaceRec rec;

		private bool disposed;

		private GCHandle memoryFaceHandle;

		private Library parentLibrary;
		private List<FTSize> childSizes;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Face"/> class from a file that's already loaded into memory.
		/// </summary>
		/// <param name="library">The parent library.</param>
		/// <param name="file">The loaded file.</param>
		/// <param name="faceIndex">The index of the face to take from the file.</param>
		public unsafe Face(Library library, byte[] file, int faceIndex)
			: this(library)
		{
			memoryFaceHandle = GCHandle.Alloc(file, GCHandleType.Pinned);
			IntPtr reference;
			Error err = FT.FT_New_Memory_Face(library.Reference, memoryFaceHandle.AddrOfPinnedObject(), file.Length, faceIndex, out reference);

			if (err != Error.Ok)
				throw new FreeTypeException(err);

			Reference = reference;
		}

		private Face(Library parent)
		{
			childSizes = new List<FTSize>();

			if (parent != null)
			{
				parentLibrary = parent;
				parentLibrary.AddChildFace(this);
			}
			else
			{
				//if there's no parent, this is a marshalled duplicate.
				FT.FT_Reference_Face(Reference);
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="Face"/> class.
		/// </summary>
		~Face()
		{
			Dispose(false);
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the face is disposed.
		/// </summary>
		public event EventHandler Disposed;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get
			{
				return disposed;
			}
		}

		/// <summary>
		/// Gets the number of faces in the font file. Some font formats can have multiple faces in a font file.
		/// </summary>
		public int FaceCount
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("FaceCount", "Cannot access a disposed object.");

				return (int)rec.num_faces;
			}
		}

		/// <summary>
		/// Gets the index of the face in the font file. It is set to 0 if there is only one face in the font file.
		/// </summary>
		public int FaceIndex
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("FaceIndex", "Cannot access a disposed object.");

				return (int)rec.face_index;
			}
		}

		/// <summary><para>
		/// Gets the number of glyphs in the face. If the face is scalable and has sbits (see ‘num_fixed_sizes’), it is
		/// set to the number of outline glyphs.
		/// </para><para>
		/// For CID-keyed fonts, this value gives the highest CID used in the font.
		/// </para></summary>
		public int GlyphCount
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("GlyphCount", "Cannot access a disposed object.");

				return (int)rec.num_glyphs;
			}
		}

		/// <summary>
		/// Gets the face's family name. This is an ASCII string, usually in English, which describes the typeface's
		/// family (like ‘Times New Roman’, ‘Bodoni’, ‘Garamond’, etc). This is a least common denominator used to list
		/// fonts. Some formats (TrueType &amp; OpenType) provide localized and Unicode versions of this string.
		/// Applications should use the format specific interface to access them. Can be NULL (e.g., in fonts embedded
		/// in a PDF file).
		/// </summary>
		public string FamilyName
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("FamilyName", "Cannot access a disposed object.");

				return Marshal.PtrToStringAnsi(rec.family_name);
			}
		}

		/// <summary>
		/// Gets the face's style name. This is an ASCII string, usually in English, which describes the typeface's
		/// style (like ‘Italic’, ‘Bold’, ‘Condensed’, etc). Not all font formats provide a style name, so this field
		/// is optional, and can be set to NULL. As for ‘family_name’, some formats provide localized and Unicode
		/// versions of this string. Applications should use the format specific interface to access them.
		/// </summary>
		public string StyleName
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("StyleName", "Cannot access a disposed object.");

				return Marshal.PtrToStringAnsi(rec.style_name);
			}
		}

		/// <summary>
		/// Gets the number of bitmap strikes in the face. Even if the face is scalable, there might still be bitmap
		/// strikes, which are called ‘sbits’ in that case.
		/// </summary>
		public int FixedSizesCount
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("FixedSizesCount", "Cannot access a disposed object.");

				return rec.num_fixed_sizes;
			}
		}

		/// <summary>
		/// Gets the number of charmaps in the face.
		/// </summary>
		public int CharmapsCount
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("CharmapsCount", "Cannot access a disposed object.");

				return rec.num_charmaps;
			}
		}

		/// <summary><para>
		/// Gets the font bounding box. Coordinates are expressed in font units (see ‘units_per_EM’). The box is large
		/// enough to contain any glyph from the font. Thus, ‘bbox.yMax’ can be seen as the ‘maximal ascender’, and
		/// ‘bbox.yMin’ as the ‘minimal descender’. Only relevant for scalable formats.
		/// </para><para>
		/// Note that the bounding box might be off by (at least) one pixel for hinted fonts. See FT_Size_Metrics for
		/// further discussion.
		/// </para></summary>
		public BBox BBox
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("BBox", "Cannot access a disposed object.");

				return rec.bbox;
			}
		}

		/// <summary>
		/// Gets the number of font units per EM square for this face. This is typically 2048 for TrueType fonts, and
		/// 1000 for Type 1 fonts. Only relevant for scalable formats.
		/// </summary>
		[CLSCompliant(false)]
		public ushort UnitsPerEM
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("UnitsPerEM", "Cannot access a disposed object.");

				return rec.units_per_EM;
			}
		}

		/// <summary>
		/// Gets the typographic ascender of the face, expressed in font units. For font formats not having this
		/// information, it is set to ‘bbox.yMax’. Only relevant for scalable formats.
		/// </summary>
		public short Ascender
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Ascender", "Cannot access a disposed object.");

				return rec.ascender;
			}
		}

		/// <summary>
		/// Gets the typographic descender of the face, expressed in font units. For font formats not having this
		/// information, it is set to ‘bbox.yMin’.Note that this field is usually negative. Only relevant for scalable
		/// formats.
		/// </summary>
		public short Descender
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Descender", "Cannot access a disposed object.");

				return rec.descender;
			}
		}

		/// <summary>
		/// Gets the height is the vertical distance between two consecutive baselines, expressed in font units. It is
		/// always positive. Only relevant for scalable formats.
		/// </summary>
		public short Height
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Height", "Cannot access a disposed object.");

				return rec.height;
			}
		}

		/// <summary>
		/// Gets the maximal advance width, in font units, for all glyphs in this face. This can be used to make word
		/// wrapping computations faster. Only relevant for scalable formats.
		/// </summary>
		public short MaxAdvanceWidth
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("MaxAdvanceWidth", "Cannot access a disposed object.");

				return rec.max_advance_width;
			}
		}

		/// <summary>
		/// Gets the maximal advance height, in font units, for all glyphs in this face. This is only relevant for
		/// vertical layouts, and is set to ‘height’ for fonts that do not provide vertical metrics. Only relevant for
		/// scalable formats.
		/// </summary>
		public short MaxAdvanceHeight
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("MaxAdvanceHeight", "Cannot access a disposed object.");

				return rec.max_advance_height;
			}
		}

		/// <summary>
		/// Gets the position, in font units, of the underline line for this face. It is the center of the underlining
		/// stem. Only relevant for scalable formats.
		/// </summary>
		public short UnderlinePosition
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("UnderlinePosition", "Cannot access a disposed object.");

				return rec.underline_position;
			}
		}

		/// <summary>
		/// Gets the thickness, in font units, of the underline for this face. Only relevant for scalable formats.
		/// </summary>
		public short UnderlineThickness
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("UnderlineThickness", "Cannot access a disposed object.");

				return rec.underline_thickness;
			}
		}

		/// <summary>
		/// Gets the face's associated glyph slot(s).
		/// </summary>
		public GlyphSlot Glyph
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Glyph", "Cannot access a disposed object.");

				return new GlyphSlot(rec.glyph, this, parentLibrary);
			}
		}

		/// <summary>
		/// Gets the current active size for this face.
		/// </summary>
		public FTSize Size
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Size", "Cannot access a disposed object.");

				return new FTSize(rec.size, false, this);
			}
		}

		/// <summary>
		/// Gets or sets ser data to identify this instance. Ignored by both FreeType and SharpFont.
		/// </summary>
		/// <remarks>This is a replacement for FT_Generic in FreeType.</remarks>
		public object Tag { get; set; }

		internal IntPtr Reference
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

				return reference;
			}

			set
			{
				if (disposed)
					throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

				reference = value;
				rec = PInvokeHelper.PtrToStructure<FaceRec>(reference);
			}
		}

		#endregion

		#region Methods
		/// <summary>
		/// This function calls <see cref="RequestSize"/> to request the nominal size (in pixels).
		/// </summary>
		/// <param name="width">The nominal width, in pixels.</param>
		/// <param name="height">The nominal height, in pixels</param>
		[CLSCompliant(false)]
		public void SetPixelSizes(uint width, uint height)
		{
			if (disposed)
				throw new ObjectDisposedException("face", "Cannot access a disposed object.");

			Error err = FT.FT_Set_Pixel_Sizes(Reference, width, height);

			if (err != Error.Ok)
				throw new FreeTypeException(err);
		}

		/// <summary>
		/// A function used to load a single glyph into the glyph slot of a face object.
		/// </summary>
		/// <remarks><para>
		/// The loaded glyph may be transformed. See <see cref="SetTransform()"/> for the details.
		/// </para><para>
		/// For subsetted CID-keyed fonts, <see cref="Error.InvalidArgument"/> is returned for invalid CID values (this
		/// is, for CID values which don't have a corresponding glyph in the font). See the discussion of the
		/// <see cref="SharpFont.FaceFlags.CidKeyed"/> flag for more details.
		/// </para></remarks>
		/// <param name="glyphIndex">
		/// The index of the glyph in the font file. For CID-keyed fonts (either in PS or in CFF format) this argument
		/// specifies the CID value.
		/// </param>
		/// <param name="flags">
		/// A flag indicating what to load for this glyph. The <see cref="LoadFlags"/> constants can be used to control
		/// the glyph loading process (e.g., whether the outline should be scaled, whether to load bitmaps or not,
		/// whether to hint the outline, etc).
		/// </param>
		/// <param name="target">The target to OR with the flags.</param>
		[CLSCompliant(false)]
		public void LoadGlyph(uint glyphIndex, LoadFlags flags, LoadTarget target)
		{
			if (disposed)
				throw new ObjectDisposedException("face", "Cannot access a disposed object.");

			Error err = FT.FT_Load_Glyph(Reference, glyphIndex, (int)flags | (int)target);

			if (err != Error.Ok)
				throw new FreeTypeException(err);
		}

		/// <summary>
		/// Return the kerning vector between two glyphs of a same face.
		/// </summary>
		/// <remarks>
		/// Only horizontal layouts (left-to-right &amp; right-to-left) are supported by this method. Other layouts, or
		/// more sophisticated kernings, are out of the scope of this API function -- they can be implemented through
		/// format-specific interfaces.
		/// </remarks>
		/// <param name="leftGlyph">The index of the left glyph in the kern pair.</param>
		/// <param name="rightGlyph">The index of the right glyph in the kern pair.</param>
		/// <param name="mode">Determines the scale and dimension of the returned kerning vector.</param>
		/// <returns>
		/// The kerning vector. This is either in font units or in pixels (26.6 format) for scalable formats, and in
		/// pixels for fixed-sizes formats.
		/// </returns>
		[CLSCompliant(false)]
		public FTVector26Dot6 GetKerning(uint leftGlyph, uint rightGlyph, KerningMode mode)
		{
			if (disposed)
				throw new ObjectDisposedException("face", "Cannot access a disposed object.");

			FTVector26Dot6 kern;
			Error err = FT.FT_Get_Kerning(Reference, leftGlyph, rightGlyph, (uint)mode, out kern);

			if (err != Error.Ok)
				throw new FreeTypeException(err);

			return kern;
		}

		/// <summary>
		/// Return the glyph index of a given character code. This function uses a charmap object to do the mapping.
		/// </summary>
		/// <remarks>
		/// If you use FreeType to manipulate the contents of font files directly, be aware that the glyph index
		/// returned by this function doesn't always correspond to the internal indices used within the file. This is
		/// done to ensure that value 0 always corresponds to the ‘missing glyph’.
		/// </remarks>
		/// <param name="charCode">The character code.</param>
		/// <returns>The glyph index. 0 means ‘undefined character code’.</returns>
		[CLSCompliant(false)]
		public uint GetCharIndex(uint charCode)
		{
			if (disposed)
				throw new ObjectDisposedException("face", "Cannot access a disposed object.");

			return FT.FT_Get_Char_Index(Reference, charCode);
		}

		/// <summary>
		/// Disposes the Face.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		internal void AddChildSize(FTSize child)
		{
			childSizes.Add(child);
		}

		internal void RemoveChildSize(FTSize child)
		{
			childSizes.Remove(child);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;
				
				foreach (FTSize s in childSizes)
					s.Dispose();

				childSizes.Clear();

				Error err = FT.FT_Done_Face(reference);

				if (err != Error.Ok)
					throw new FreeTypeException(err);

				// removes itself from the parent Library, with a check to prevent this from happening when Library is
				// being disposed (Library disposes all it's children with a foreach loop, this causes an
				// InvalidOperationException for modifying a collection during enumeration)
				if (!parentLibrary.IsDisposed)
					parentLibrary.RemoveChildFace(this);

				reference = IntPtr.Zero;

				if (memoryFaceHandle.IsAllocated)
					memoryFaceHandle.Free();

				EventHandler handler = Disposed;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
