using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Lime
{
	public class Lemon
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate uint ReadCallback (IntPtr ptr, uint size, uint nmemb, Stream stream);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate int SeekCallback (Stream stream, long offset, System.IO.SeekOrigin whence);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate int CloseCallback (Stream stream);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate int TellCallback (Stream stream);

		[StructLayout (LayoutKind.Sequential)]
		public struct FileSystem
		{
			public ReadCallback ReadFunc;
			public SeekCallback SeekFunc;
			public CloseCallback CloseFunc;
			public TellCallback TellFunc;
		}

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr OggCreate ();

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern void OggDispose (IntPtr vorbisFile);

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggOpen (Stream stream, IntPtr vorbisFile, FileSystem callbacks);

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggRead (IntPtr vorbisFile, IntPtr buffer, int length, ref int bitstream);

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern void OggResetToBeginning (IntPtr vorbisFile);

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggGetFrequency (IntPtr vorbisFile);

		[DllImport ("Lemon", CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggGetChannels (IntPtr vorbisFile);
	}
}