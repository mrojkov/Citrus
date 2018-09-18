using System;
using System.Runtime.InteropServices;


namespace Orange.FbxImporter
{
	public static class ImportConfig
	{
#if WIN
		public const string LibName = "FbxSdk.dll";
		public const CharSet Charset = CharSet.Ansi;
#elif MAC
		public const string LibName = "FbxSdkUtils";
		public const CharSet Charset = CharSet.Auto;
#endif
		public const int BoneLimit = 4;
	}

	public static class IntPtrExtensions
	{
		public static T ToStruct<T>(this IntPtr ptr)
		{
			try {				return ptr != IntPtr.Zero ? (T)Marshal.PtrToStructure(ptr, typeof(T)) : default(T);
			} finally {
				Utils.ReleaseNative(ptr);
			}
		}

		public static int[] ToIntArray(this IntPtr ptr, int size)
		{
			try {
				var result = new int[size];
				if (size == 0)
					return result;
				Marshal.Copy(ptr, result, 0, size);
				return result;
			} finally {
				Utils.ReleaseNative(ptr);
			}
		}


		public static float[] ToFloatArray(this IntPtr ptr, int size)
		{
			try {
				var result = new float[size];
				if (size == 0)
					return new float[0];
				Marshal.Copy(ptr, result, 0, size);
				return result;
			} finally {
				Utils.ReleaseNative(ptr);
			}
		}

		public static double[] ToDoubleArray(this IntPtr ptr, int size)
		{
			try {
				var result = new double[size];
				if (size == 0)
					return result;
				Marshal.Copy(ptr, result, 0, size);
				return result;
			} finally {
				Utils.ReleaseNative(ptr);
			}
		}

		public static T[] FromArrayOfPointersToStructArrayUnsafe<T>(this IntPtr ptr, int size)
		{
			try {
				if (ptr == IntPtr.Zero) return null;
				var result = new T[size];
				var pointer = ptr;
				for (var i = 0; i < size; i++) {
					var structPtr = Marshal.ReadIntPtr(pointer);
					result[i] = structPtr == IntPtr.Zero ? default(T) : (T)Marshal.PtrToStructure(structPtr, typeof(T));
					Utils.ReleaseNative(structPtr);
					pointer += IntPtr.Size;
				}
				return result;
			} finally {
				Utils.ReleaseNative(ptr);
			}
		}

		public static T[] ToStructArray<T>(this IntPtr ptr, int size)
		{
			try {
				var result = new T[size];
				var tmpPtr = ptr;
				if (ptr == IntPtr.Zero) return result;
				var structSize = Marshal.SizeOf(typeof(T));
				for (var i = 0; i < size; i++) {
					result[i] = (T)Marshal.PtrToStructure(tmpPtr, typeof(T));
					tmpPtr += structSize;
				}
				return result;
			} finally {
				Utils.ReleaseNative(ptr);
			}

		}

		public static string PtrToString(this IntPtr ptr)
		{
			try {
				if (ptr != IntPtr.Zero) {
					return Marshal.PtrToStringAnsi(ptr);
				}
			} finally {
				Utils.ReleaseNative(ptr);
			}
			return null;
		}
	}

	public static class Utils
	{
		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FbxUtilsReleaseMemory(IntPtr ptr);

		public static void ReleaseNative(IntPtr ptr)
		{
			if (ptr != IntPtr.Zero)
				FbxUtilsReleaseMemory(ptr);
		}
	}
}
