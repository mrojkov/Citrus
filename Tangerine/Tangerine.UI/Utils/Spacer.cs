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
		public static Spacer HStretch() => new Spacer { LayoutCell = new LayoutCell { StretchX = 1E-5f } };
		public static Spacer VStretch() => new Spacer { LayoutCell = new LayoutCell { StretchY = 1E-5f } };
	}
}
