#if WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Lime
{
    static class Sdl
    {
		const string lib = "SDL2.dll";

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_ShowSimpleMessageBox")]
		public static extern IntPtr ShowSimpleMessageBox(uint flags, string title, string message, IntPtr window);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateColorCursor")]
		public static extern IntPtr CreateColorCursor(IntPtr surface, int hotX, int hotY);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetCursor")]
		public static extern void SetCursor(IntPtr cursor);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateRGBSurfaceFrom")]
		public static extern IntPtr CreateRGBSurfaceFrom(IntPtr pixels,
					int width, int height, int depth, int pitch,
					uint Rmask, uint Gmask, uint Bmask, uint Amask);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_FreeSurface", ExactSpelling = true)]
		public static extern void FreeSurface(IntPtr surface);

		public static string GetError()
		{
			return IntPtrToString(GetErrorInternal());
		}

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetError", ExactSpelling = true)]
		static extern IntPtr GetErrorInternal();
		
		static string IntPtrToString(IntPtr ptr)
		{
			return Marshal.PtrToStringAnsi(ptr);
		}
    }
}
#endif