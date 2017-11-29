#if OPENAL
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Lime.OpenALSoft
{
	public enum ALCapability : int
	{
		Invalid = -1,
	}

	public enum ALSourcef : int
	{
		ReferenceDistance = 0x1020,
		MaxDistance = 0x1023,
		RolloffFactor = 0x1021,
		Pitch = 0x1003,
		Gain = 0x100A,
		MinGain = 0x100D,
		MaxGain = 0x100E,
		ConeInnerAngle = 0x1001,
		ConeOuterAngle = 0x1002,
		ConeOuterGain = 0x1022,
		SecOffset = 0x1024,
		EfxAirAbsorptionFactor = 0x20007,
		EfxRoomRolloffFactor = 0x20008,
		EfxConeOuterGainHighFrequency = 0x20009,

	}

	public enum ALSource3f : int
	{
		Position = 0x1004,
		Velocity = 0x1006,
		Direction = 0x1005,
	}

	public enum ALSourceb : int
	{
		SourceRelative = 0x202,
		Looping = 0x1007,
		EfxDirectFilterGainHighFrequencyAuto = 0x2000A,
		EfxAuxiliarySendFilterGainAuto = 0x2000B,
		EfxAuxiliarySendFilterGainHighFrequencyAuto = 0x2000C,
	}

	public enum ALSourcei : int
	{
		ByteOffset = 0x1026,
		SampleOffset = 0x1025,
		Buffer = 0x1009,
		SourceType = 0x1027,
		EfxDirectFilter = 0x20005,
	}

	public enum ALSource3i : int
	{
		EfxAuxiliarySendFilter = 0x20006,
	}

	public enum ALGetSourcei : int
	{
		ByteOffset = 0x1026,
		SampleOffset = 0x1025,
		Buffer = 0x1009,
		SourceState = 0x1010,
		BuffersQueued = 0x1015,
		BuffersProcessed = 0x1016,
		SourceType = 0x1027,
	}

	public enum ALSourceState : int
	{
		Initial = 0x1011,
		Playing = 0x1012,
		Paused = 0x1013,
		Stopped = 0x1014,
	}

	public enum ALSourceType : int
	{
		Static = 0x1028,
		Streaming = 0x1029,
		Undetermined = 0x1030,
	}

	public enum ALFormat : int
	{
		Mono8 = 0x1100,
		Mono16 = 0x1101,
		Stereo8 = 0x1102,
		Stereo16 = 0x1103,
		MonoALawExt = 0x10016,
		StereoALawExt = 0x10017,
		MonoMuLawExt = 0x10014,
		StereoMuLawExt = 0x10015,
		VorbisExt = 0x10003,
		Mp3Ext = 0x10020,
		MonoIma4Ext = 0x1300,
		StereoIma4Ext = 0x1301,
		MonoFloat32Ext = 0x10010,
		StereoFloat32Ext = 0x10011,
		MonoDoubleExt = 0x10012,
		StereoDoubleExt = 0x10013,
		Multi51Chn16Ext = 0x120B,
		Multi51Chn32Ext = 0x120C,
		Multi51Chn8Ext = 0x120A,
		Multi61Chn16Ext = 0x120E,
		Multi61Chn32Ext = 0x120F,
		Multi61Chn8Ext = 0x120D,
		Multi71Chn16Ext = 0x1211,
		Multi71Chn32Ext = 0x1212,
		Multi71Chn8Ext = 0x1210,
		MultiQuad16Ext = 0x1205,
		MultiQuad32Ext = 0x1206,
		MultiQuad8Ext = 0x1204,
		MultiRear16Ext = 0x1208,
		MultiRear32Ext = 0x1209,
		MultiRear8Ext = 0x1207,
	}

	public enum ALGetBufferi : int
	{
		Frequency = 0x2001,
		Bits = 0x2002,
		Channels = 0x2003,
		Size = 0x2004,
	}

	public enum ALBufferState : int
	{
		Unused = 0x2010,
		Pending = 0x2011,
		Processed = 0x2012,
	}

	public enum ALError : int
	{
		NoError = 0,
		InvalidName = 0xA001,
		IllegalEnum = 0xA002,
		InvalidEnum = 0xA002,
		InvalidValue = 0xA003,
		IllegalCommand = 0xA004,
		InvalidOperation = 0xA004,
		OutOfMemory = 0xA005,
	}

	public enum ALGetString : int
	{
		Vendor = 0xB001,
		Version = 0xB002,
		Renderer = 0xB003,
		Extensions = 0xB004,
	}

	public enum ALGetFloat : int
	{
		DopplerFactor = 0xC000,
		DopplerVelocity = 0xC001,
		SpeedOfSound = 0xC003,
	}

	public enum ALGetInteger : int
	{
		DistanceModel = 0xD000,
	}

	public static class AL
	{
		internal const string Lib = "__Internal";
		private const CallingConvention Style = CallingConvention.Cdecl;

		[DllImport(Lib, EntryPoint = "ALMOB_alGetString", ExactSpelling = true, CallingConvention = Style, CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity()]
		private static extern IntPtr GetStringPrivate(ALGetString param); // accepts the enums AlError, AlContextString

		public static string Get(ALGetString param)
		{
			return Marshal.PtrToStringAnsi(GetStringPrivate(param));
		}

		public static string GetErrorString(ALError param)
		{
			return Marshal.PtrToStringAnsi(GetStringPrivate((ALGetString)param));
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alGetInteger", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern int Get(ALGetInteger param);

		[DllImport(Lib, EntryPoint = "ALMOB_alGetFloat", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern float Get(ALGetFloat param);

		[DllImport(Lib, EntryPoint = "ALMOB_alGetError", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern ALError GetError();

		[DllImport(Lib, EntryPoint = "ALMOB_alGenSources", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		unsafe private static extern void GenSources(int n, out int sources);

		public static int GenSource()
		{
			int temp;
			GenSources(1, out temp);
			return temp;
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alDeleteSources", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void DeleteSources(int n, ref int sources);

		public static void DeleteSource(int source)
		{
			DeleteSources(1, ref source);
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alIsSource", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern bool IsSource(int sid);

		[DllImport(Lib, EntryPoint = "ALMOB_alSourcef", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void Source(int sid, ALSourcef param, float value);

		[DllImport(Lib, EntryPoint = "ALMOB_alSource3f", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void Source(int sid, ALSource3f param, float value1, float value2, float value3);

		[DllImport(Lib, EntryPoint = "ALMOB_alSourcei", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void Source(int sid, ALSourcei param, int value);
			
		public static void Source(int sid, ALSourceb param, bool value)
		{
			Source(sid, (ALSourcei)param, (value) ? 1 : 0);
		}

		public static void BindBufferToSource(int source, int buffer)
		{
			Source(source, ALSourcei.Buffer, buffer);
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alSource3i", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void Source(int sid, ALSource3i param, int value1, int value2, int value3);

		[DllImport(Lib, EntryPoint = "ALMOB_alGetSourcef", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void GetSource(int sid, ALSourcef param, [Out] out float value);

		[DllImport(Lib, EntryPoint = "ALMOB_alGetSource3f", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void GetSource(int sid, ALSource3f param, [Out] out float value1, [Out] out float value2, [Out] out float value3);

		[DllImport(Lib, EntryPoint = "ALMOB_alGetSourcei", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void GetSource(int sid, ALGetSourcei param, [Out] out int value);

		public static void GetSource(int sid, ALSourceb param, out bool value)
		{
			int result;
			GetSource(sid, (ALGetSourcei)param, out result);
			value = result != 0;
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alSourcePlay", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void SourcePlay(int sid);

		[DllImport(Lib, EntryPoint = "ALMOB_alSourceStop", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void SourceStop(int sid);

		[DllImport(Lib, EntryPoint = "ALMOB_alSourcePause", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void SourcePause(int sid);

		[DllImport(Lib, EntryPoint = "ALMOB_alSourceQueueBuffers"), SuppressUnmanagedCodeSecurity]
		unsafe public static extern void SourceQueueBuffers(uint sid, int numEntries, [In] ref int bids);

		public static void SourceQueueBuffer(int source, int buffer)
		{
			AL.SourceQueueBuffers((uint)source, 1, ref buffer);
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alSourceUnqueueBuffers"), SuppressUnmanagedCodeSecurity]
		unsafe public static extern void SourceUnqueueBuffers(int sid, int numEntries, [Out] out int bids);

		public static int SourceUnqueueBuffer(int sid)
		{
			int buf;
			SourceUnqueueBuffers(sid, 1, out buf);
			return buf;
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alGenBuffers", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity]
		unsafe public static extern void GenBuffers(int n, [Out] out int buffers);

		public static int GenBuffer()
		{
			int temp;
			GenBuffers(1, out temp);
			return temp;
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alDeleteBuffers", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		unsafe public static extern void DeleteBuffers(int n, [In] ref int buffers);

		public static void DeleteBuffer(int buffer)
		{
			DeleteBuffers(1, ref buffer);
		}

		[DllImport(Lib, EntryPoint = "ALMOB_alBufferData", ExactSpelling = true, CallingConvention = Style), SuppressUnmanagedCodeSecurity()]
		public static extern void BufferData(int bid, ALFormat format, IntPtr buffer, int size, int freq);

		public static ALSourceState GetSourceState(int sid)
		{
			int temp;
			GetSource(sid, ALGetSourcei.SourceState, out temp);
			return (ALSourceState)temp;
		}

		public static ALSourceType GetSourceType(int sid)
		{
			int temp;
			GetSource(sid, ALGetSourcei.SourceType, out temp);
			return (ALSourceType)temp;
		}
	}
}
#endif