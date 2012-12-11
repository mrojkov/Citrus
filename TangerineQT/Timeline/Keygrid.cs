using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Runtime.InteropServices;

namespace Tangerine
{
	public class Keygrid : QWidget
	{
		int ActiveRow { get { return The.Timeline.ActiveRow; } set { The.Timeline.ActiveRow = value; } }
		int RowHeight { get { return The.Timeline.RowHeight; } set { The.Timeline.RowHeight = value; } }
		int ColWidth { get { return The.Timeline.ColWidth; } set { The.Timeline.ColWidth = value; } }

		public Keygrid()
		{
			Paint += Keygrid_Paint;
		}

		struct Keymark
		{
			public QColor Color;
			public int Line;
			public int Length;
			public int Frame;
		}

		List<Keymark> GetKeyMarks(Lime.Node node)
		{
			var marks = new List<Keymark>();
			int line = 0;
			foreach (var ani in node.Animators) {
				var prop = node.GetType().GetProperty(ani.TargetProperty);
				var attrs = prop.GetCustomAttributes(typeof(Lime.TangerinePropertyAttribute), false);
				if (attrs.Length == 0) {
					continue;
				}
				var attr = attrs[0] as Lime.TangerinePropertyAttribute;
				for (int i = 0; i < ani.Frames.Length; i++) {
					var key0 = ani[i];
					var mark = new Keymark() {
						Line = line,
						Color = new QColor(attr.ColorName),
						Frame = key0.Frame
					};
					if (i < ani.Frames.Length - 1) {
						var key1 = ani[i + 1];
						if (!key0.Value.Equals(key1.Value)) {
							mark.Length = key1.Frame - mark.Frame;
						}
					}
					marks.Add(mark);
				}
				line++;
			}
			return marks;
		}

		void Keygrid_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numRows = Size.Height / RowHeight + 1;
			int numCols = Size.Width / ColWidth + 1;
			var ptr = new QPainter(this);
			DrawGrid(numRows, numCols, ptr);
			LinedrawingBenchmark(ptr);
			var nodes = The.Document.RootNode.Nodes;
			for (int i = 0; i < nodes.Count; i++) {
				var node = nodes[i];
				if (i < numRows) {
					DrawRow(GetKeyMarks(node), i, 0, numCols, ptr);
				}
			}
			ptr.End();
		}

		private void DrawRow(List<Keymark> keyMarks, int row, int startFrame, int endFrame, QPainter ptr)
		{
			for (int i = 0; i < keyMarks.Count; i++) {
				var m = keyMarks[i];
				if (m.Frame >= startFrame && m.Frame < endFrame) {
					int x = ColWidth * (m.Frame - startFrame) + 5;
					int y = RowHeight * row + m.Line * 6 + 7;
					DrawKey(ptr, m, x, y);
				}
			}
		}

		//private static void CalcKeymarksInCell(List<KeyMark> keyMarks, int i)
		//{
		//	int f = keyMarks[i].Frame;
		//	int c = 0;
		//	for (int j = i; j < keyMarks.Count; j++) {
		//		if (keyMarks[j].Frame != keyMarks[i].Frame) {
		//			break;
		//		}
		//		c++;
		//	}
		//	return c;
		//}

		[DllImport("qyoto-qtcore-native", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		static extern ModuleIndex FindMethodId(string className, string mungedName, string signature);

		[DllImport("qyoto-qtcore-native", CharSet = CharSet.Ansi, EntryPoint = "CallSmokeMethod", CallingConvention = CallingConvention.Cdecl)]
		static extern void CallSmokeMethod(IntPtr smoke, int methodId, IntPtr target, IntPtr sp, int items);

		ModuleIndex drawLineIndex;

		private void DrawKey(QPainter ptr, Keymark m, int x, int y)
		{
			//ptr.Pen = new QPen(new QColor("Darkgray"), 4);
			//ptr.DrawArc(x - 4, y - 4, 8, 8, 0, 360 * 16);
			//ptr.Pen = new QPen(new QColor("Darkgray"), 5);
			//ptr.DrawLine(x, y, x + m.Length * ColWidth, y);
			//ptr.Pen = new QPen(m.Color, 2);// new QColor(m.Color.R, m.Color.G, m.Color.B), 3);
			//ptr.DrawArc(x - 3, y - 3, 6, 6, 0, 360 * 16);
			//ptr.DrawLine(x, y, x + m.Length * ColWidth, y);
			//LinedrawingBenchmark(ptr);
			//Console.Write("!");
			//ptr.SmokeObject
		}

		private void LinedrawingBenchmark(QPainter ptr)
		{
			long t = System.Environment.TickCount;
			for (int i = 0; i < 30000; i++) {
				ptr.DrawLine(0, 0, 30, 0);
			}
			t = System.Environment.TickCount - t;
			Console.WriteLine("T1: " + t);
			t = System.Environment.TickCount;
			GCHandle instanceHandle = GCHandle.Alloc(ptr);
			unsafe {
				StackItem* si = stackalloc StackItem[5];
				for (int i = 0; i < 30000; i++) {
					if (drawLineIndex.index == 0) {
						drawLineIndex = FindMethodId("QPainter", "drawLine$$$$", "drawLine(int, int, int, int)");
					}
					si[1].s_int = 0;
					si[2].s_int = 0;
					si[3].s_int = 30;
					si[4].s_int = 0;
					CallSmokeMethod(drawLineIndex.smoke, drawLineIndex.index, (IntPtr)instanceHandle, (IntPtr)si, 4);//(IntPtr)stackPtr, stack.Length);
				}
			}
			t = System.Environment.TickCount - t;
			Console.WriteLine("T2: " + t);
			instanceHandle.Free();
		}

		private void DrawGrid(int numRows, int numCols, QPainter ptr)
		{
			ptr.FillRect(Rect, GlobalColor.white);
			ptr.Pen = new QPen(GlobalColor.darkGray);
			var line = new QLine(0, 0, Size.Width, 0);
			for (int i = 0; i < numRows; i++) {
				line.Translate(0, RowHeight);
				ptr.DrawLine(line);
			}
			ptr.Pen = new QPen(GlobalColor.lightGray);
			line = new QLine(0, 0, 0, Size.Height);
			for (int i = 0; i < numCols / 5; i++) {
				line.Translate(ColWidth * 5, 0);
				ptr.DrawLine(line);
			}
		}
	}
}
