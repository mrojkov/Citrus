using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Lemon
{
	public class Api
	{
#if iOS
		const string Dll = "__Internal";
#else
		const string Dll = "Lemon";
#endif
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate uint ReadCallback(IntPtr ptr, uint size, uint nmemb, int datasource);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int SeekCallback(int datasource, long offset, System.IO.SeekOrigin whence);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int CloseCallback(int datasource);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int TellCallback(int datasource);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct FileSystem
		{
			public ReadCallback ReadFunc;
			public SeekCallback SeekFunc;
			public CloseCallback CloseFunc;
			public TellCallback TellFunc;
		}
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr OggCreate();
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern void OggDispose(IntPtr vorbisFile);
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggOpen(int datasource, IntPtr vorbisFile, FileSystem callbacks);
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggRead(IntPtr vorbisFile, IntPtr buffer, int length, ref int bitstream);
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern void OggResetToBeginning(IntPtr vorbisFile);
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggGetFrequency(IntPtr vorbisFile);
		
		[DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
		public static extern int OggGetChannels(IntPtr vorbisFile);
	}
}

