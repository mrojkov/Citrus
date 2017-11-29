#if OPENAL
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Lime.OpenALSoft
{
	public enum AlcError : int
	{
		NoError = 0,
		InvalidDevice = 0xA001,
		InvalidContext = 0xA002,
		InvalidEnum = 0xA003,
		InvalidValue = 0xA004,
		OutOfMemory = 0xA005,
	}

	public static class Alc
	{
		private const string Lib = "__Internal";
		private const CallingConvention Style = CallingConvention.Cdecl;

		[DllImport(Lib, EntryPoint = "ALMOB_alcCreateContext", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity]
		public unsafe static extern IntPtr CreateContext([In] IntPtr device, [In] int* attrlist);

		[DllImport(Lib, EntryPoint = "ALMOB_alcMakeContextCurrent", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern bool MakeContextCurrent(IntPtr context);

		[DllImport(Lib, EntryPoint = "ALMOB_alcProcessContext", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void ProcessContext(IntPtr context);

		[DllImport(Lib, EntryPoint = "ALMOB_alcSuspendContext", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void SuspendContext(IntPtr context);

		[DllImport(Lib, EntryPoint = "ALMOB_alcDestroyContext", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void DestroyContext(IntPtr context);

		[DllImport(Lib, EntryPoint = "ALMOB_alcGetCurrentContext", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern IntPtr GetCurrentContext();

		[DllImport(Lib, EntryPoint = "ALMOB_alcGetContextsDevice", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern IntPtr GetContextsDevice(IntPtr context);

		[DllImport(Lib, EntryPoint = "ALMOB_alcOpenDevice", ExactSpelling = true, CallingConvention = Style, CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity()]
		public static extern IntPtr OpenDevice([In] string devicename);

		[DllImport(Lib, EntryPoint = "ALMOB_alcCloseDevice", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern bool CloseDevice([In] IntPtr device);

		[DllImport(Lib, EntryPoint = "ALMOB_alcGetError", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern AlcError GetError([In] IntPtr device);
	}
}
#endif