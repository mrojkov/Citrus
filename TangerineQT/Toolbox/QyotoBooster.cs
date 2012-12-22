using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	/// <summary>
	/// Этот класс содержит функции, ускоряющие отрисовку через Qyoto.
	/// По результатам бенчмарка оверхед на вызов функции уменьшается в 2 раза.
	/// </summary>
	public class QyotoBooster
	{
		//[DllImport("qyoto-qtcore-native", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		//static extern ModuleIndex FindMethodId(string className, string mungedName, string signature);

		//[DllImport("qyoto-qtcore-native", CharSet = CharSet.Ansi, EntryPoint = "CallSmokeMethod", CallingConvention = CallingConvention.Cdecl)]
		//static extern void CallSmokeMethod(IntPtr smoke, int methodId, IntPtr target, IntPtr sp, int items);

		//ModuleIndex drawLineIndex;

		//private void LinedrawingBenchmark(QPainter ptr)
		//{
		//	long t = System.Environment.TickCount;
		//	for (int i = 0; i < 30000; i++) {
		//		ptr.DrawLine(0, 0, 0, 0);
		//	}
		//	t = System.Environment.TickCount - t;
		//	Console.WriteLine("T1: " + t);
		//	t = System.Environment.TickCount;
		//	GCHandle instanceHandle = GCHandle.Alloc(ptr);
		//	unsafe {
		//		StackItem* si = stackalloc StackItem[5];
		//		for (int i = 0; i < 30000; i++) {
		//			if (drawLineIndex.index == 0) {
		//				drawLineIndex = FindMethodId("QPainter", "drawLine$$$$", "drawLine(int, int, int, int)");
		//			}
		//			si[1].s_int = 0;
		//			si[2].s_int = 0;
		//			si[3].s_int = 0;
		//			si[4].s_int = 0;
		//			CallSmokeMethod(drawLineIndex.smoke, drawLineIndex.index, (IntPtr)instanceHandle, (IntPtr)si, 4);//(IntPtr)stackPtr, stack.Length);
		//		}
		//	}
		//	t = System.Environment.TickCount - t;
		//	Console.WriteLine("T2: " + t);
		//	instanceHandle.Free();
		//}
	}
}
