using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class Spacer : Widget
	{
		public static Spacer HSpacer(float width) => new Spacer { MinMaxWidth = width };
		public static Spacer VSpacer(float height) => new Spacer { MinMaxHeight = height };
		public static Spacer HStretch() => new Spacer { LayoutCell = new LayoutCell { StretchX = 0 } };
		public static Spacer VStretch() => new Spacer { LayoutCell = new LayoutCell { StretchY = 0 } };
		public static Spacer HFill() => new Spacer { LayoutCell = new LayoutCell { StretchX = float.MaxValue } };
		public static Spacer VFill() => new Spacer { LayoutCell = new LayoutCell { StretchY = float.MaxValue } };
	}
}
