#if !ANDROID && !iOS
using System;

namespace Lime
{
	public class ThemedHSplitter : HSplitter
	{
		protected override bool IsNotDecorated() => false;

		public ThemedHSplitter()
		{
			SeparatorColor = Theme.Colors.SeparatorColor;
			SeparatorWidth = 1;
			SeparatorActiveAreaWidth = 4;
		}
	}

	public class ThemedVSplitter : VSplitter
	{
		protected override bool IsNotDecorated() => false;

		public ThemedVSplitter()
		{
			SeparatorColor = Theme.Colors.SeparatorColor;
			SeparatorWidth = 1;
			SeparatorActiveAreaWidth = 4;
		}
	}
}
#endif