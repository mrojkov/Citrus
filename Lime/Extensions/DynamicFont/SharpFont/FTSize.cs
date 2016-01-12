#region MIT License
/*Copyright (c) 2012-2013, 2015 Robert Rouhani <robert.rouhani@gmail.com>

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
	/// <summary>
	/// FreeType root size class structure. A size object models a face object at a given size.
	/// </summary>
	public sealed class FTSize : IDisposable
	{
		#region Fields

		private bool userAlloc;
		private bool disposed;
		private bool duplicate;

		private IntPtr reference;
		private SizeRec rec;

		private Face parentFace;

		#endregion

		#region Constructors

		internal FTSize(IntPtr reference, bool userAlloc, Face parentFace)
		{
			Reference = reference;

			this.userAlloc = userAlloc;

			if (parentFace != null)
			{
				this.parentFace = parentFace;
				parentFace.AddChildSize(this);
			}
			else
			{
				duplicate = true;
			}
		}

		/// <summary>
		/// Finalizes an instance of the FTSize class.
		/// </summary>
		~FTSize()
		{
			Dispose(false);
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the size is disposed.
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
		/// Gets a handle to the parent face object.
		/// </summary>
		public Face Face
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Face", "Cannot access a disposed object.");

				return parentFace;
			}
		}

		/// <summary>
		/// Gets metrics for this size object. This field is read-only.
		/// </summary>
		public SizeMetrics Metrics
		{
			get
			{
				if (disposed)
					throw new ObjectDisposedException("Metrics", "Cannot access a disposed object.");

				return new SizeMetrics(rec.metrics);
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
				this.rec = PInvokeHelper.PtrToStructure<SizeRec>(reference);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Diposes the FTSize.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Private Methods

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;

				//only dispose the user allocated sizes that are not duplicates.
				if (userAlloc && !duplicate)
				{
					Error err = FT.FT_Done_Size(reference);

					if (err != Error.Ok)
						throw new FreeTypeException(err);
				}

				// removes itself from the parent Face, with a check to prevent this from happening when Face is
				// being disposed (Face disposes all it's children with a foreach loop, this causes an
				// InvalidOperationException for modifying a collection during enumeration)
				if (parentFace != null && !parentFace.IsDisposed)
					parentFace.RemoveChildSize(this);

				reference = IntPtr.Zero;

				EventHandler handler = Disposed;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
