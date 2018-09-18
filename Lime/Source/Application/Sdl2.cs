#if WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Lime
{
	class Sdl2
	{
		[Flags]
		public enum SystemFlags : uint
		{
			Default = 0,
			TIMER = 0x00000001,
			AUDIO = 0x00000010,
			VIDEO = 0x00000020,
			JOYSTICK = 0x00000200,
			HAPTIC = 0x00001000,
			GAMECONTROLLER = 0x00002000,
			NOPARACHUTE = 0x00100000,
			EVERYTHING = TIMER | AUDIO | VIDEO |
				JOYSTICK | HAPTIC | GAMECONTROLLER
		}

		public enum ContextAttribute
		{
			RED_SIZE,
			GREEN_SIZE,
			BLUE_SIZE,
			ALPHA_SIZE,
			BUFFER_SIZE,
			DOUBLEBUFFER,
			DEPTH_SIZE,
			STENCIL_SIZE,
			ACCUM_RED_SIZE,
			ACCUM_GREEN_SIZE,
			ACCUM_BLUE_SIZE,
			ACCUM_ALPHA_SIZE,
			STEREO,
			MULTISAMPLEBUFFERS,
			MULTISAMPLESAMPLES,
			ACCELERATED_VISUAL,
			RETAINED_BACKING,
			CONTEXT_MAJOR_VERSION,
			CONTEXT_MINOR_VERSION,
			CONTEXT_EGL,
			CONTEXT_FLAGS,
			CONTEXT_PROFILE_MASK,
			SHARE_WITH_CURRENT_CONTEXT
		}

		const string lib = "SDL2.dll";

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_Init", ExactSpelling = true)]
		public static extern int Init(SystemFlags flags);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_SetAttribute", ExactSpelling = true)]
		public static extern int SetAttribute(ContextAttribute attr, int value);
	}
}
#endif