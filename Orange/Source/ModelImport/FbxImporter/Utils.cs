using System;
using System.Linq;
using System.Runtime.InteropServices;


namespace Orange.FbxImporter
{
	public static class ImportConfig
	{
		public const string LibName = "FbxSdk.dll";
	}

	public static class IntPtrExtensions
	{
		public static T To<T>(this IntPtr ptr) {
			return ptr == IntPtr.Zero ? default(T) : (T)Marshal.PtrToStructure(ptr, typeof(T));
		}
		public static T ToStruct<T>(this IntPtr ptr) where T: class
		{
			return ptr == IntPtr.Zero ? null : (T)Marshal.PtrToStructure(ptr, typeof(T));
		}

		public static int[] ToIntArray(this IntPtr ptr, int size)
		{
			if (size == 0)
				return null;
			int[] result = new int[size];
			Marshal.Copy(ptr, result, 0 ,size);
			return result;
		}

		public static double[] ToDoubleArray(this IntPtr ptr, int size)
		{
			if (size == 0)
				return null;
			double[] result = new double[size];
			Marshal.Copy(ptr, result, 0, size);
			return result;
		}

		public static T[] ToStructArray<T>(this IntPtr ptr, int size)
		{
			T[] result = new T[size];
			var strucSize = Marshal.SizeOf(typeof(T));
			for (int i = 0; i < size; i++) {
				var structPtr = new IntPtr(ptr.ToInt64() + strucSize * i);
				result[i] = (T)Marshal.PtrToStructure(structPtr, typeof(T));
			}
			return result;
		}

		public static T[] ToArrayStruct<T, TStruct>(this IntPtr ptr, int size)
		{
			T[] result = new T[size];
			Marshal.Copy(ptr, result as IntPtr[], 0, size);
			return result;
		}
	}

	public static class Utils
	{
		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void ReleaseMemory(IntPtr ptr);

		public static void ReleaseNative(IntPtr ptr) {
			ReleaseMemory(ptr);
		}
	}
}
