#region MIT License
/*Copyright (c) 2012-2014 Robert Rouhani <robert.rouhani@gmail.com>

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

namespace SharpFont
{
	/// <summary><para>
	/// A handle to a FreeType library instance. Each ‘library’ is completely independent from the others; it is the
	/// ‘root’ of a set of objects like fonts, faces, sizes, etc.
	/// </para><para>
	/// It also embeds a memory manager (see <see cref="Memory"/>), as well as a scan-line converter object (see
	/// <see cref="Raster"/>).
	/// </para><para>
	/// For multi-threading applications each thread should have its own <see cref="Library"/> object.
	/// </para></summary>
	public sealed class Library : IDisposable
	{
		#region Fields

		private IntPtr reference;

		private bool customMemory;
		private bool disposed;

		private List<Face> childFaces;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Library"/> class.
		/// </summary>
		/// <remarks>
		/// SharpFont assumes that you have the correct version of FreeType for your operating system and processor
		/// architecture. If you get a <see cref="BadImageFormatException"/> here on Windows, there's a good chance
		/// that you're trying to run your program as a 64-bit process and have a 32-bit version of FreeType or vice
		/// versa. See the SharpFont.Examples project for how to handle this situation.
		/// </remarks>
		public Library()
			: this(false)
		{
			IntPtr libraryRef;
			Error err = FT.FT_Init_FreeType(out libraryRef);

			if (err != Error.Ok)
				throw new FreeTypeException(err);

			Reference = libraryRef;
		}

		private Library(bool duplicate)
		{
			childFaces = new List<Face>();
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="Library"/> class.
		/// </summary>
		~Library()
		{
			Dispose(false);
		}

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
		/// Gets the version of the FreeType library being used.
		/// </summary>
		public Version Version
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Version", "Cannot access a disposed object.");

				int major, minor, patch;
				FT.FT_Library_Version(Reference, out major, out minor, out patch);
				return new Version(major, minor, patch);
			}
		}

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
			}
		}

		#endregion

		#region Methods

		#region Base Interface
		/// <summary>
		/// This function calls <see cref="OpenFace"/> to open a font which has been loaded into memory.
		/// </summary>
		/// <remarks>
		/// You must not deallocate the memory before calling <see cref="Face.Dispose()"/>.
		/// </remarks>
		/// <param name="file">A pointer to the beginning of the font data.</param>
		/// <param name="faceIndex">The index of the face within the font. The first face has index 0.</param>
		/// <returns>
		/// A handle to a new face object. If ‘faceIndex’ is greater than or equal to zero, it must be non-NULL.
		/// </returns>
		/// <see cref="OpenFace"/>
		public Face NewMemoryFace(byte[] file, int faceIndex)
		{
			if (disposed)
				throw new ObjectDisposedException("Library", "Cannot access a disposed object.");

			return new Face(this, file, faceIndex);
		}

		#endregion

		/// <summary>
		/// Disposes the Library.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		internal void AddChildFace(Face child)
		{
			childFaces.Add(child);
		}

		internal void RemoveChildFace(Face child)
		{
			childFaces.Remove(child);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;

				//dipose all the children before disposing the library.
				foreach (Face f in childFaces)
					f.Dispose();

				childFaces.Clear();

				Error err = customMemory ? FT.FT_Done_Library(reference) : FT.FT_Done_FreeType(reference);

				if (err != Error.Ok)
					throw new FreeTypeException(err);

				reference = IntPtr.Zero;
			}
		}

		#endregion
	}
}
