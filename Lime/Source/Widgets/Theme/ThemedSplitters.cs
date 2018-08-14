#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedHSplitter : HSplitter
	{
		public override bool IsNotDecorated() => false;

		public ThemedHSplitter()
		{
			SeparatorColor = Theme.Colors.SeparatorColor;
			SeparatorWidth = 1;
			SeparatorActiveAreaWidth = 4;
		}
	}

	[YuzuDontGenerateDeserializer]
	public class ThemedVSplitter : VSplitter
	{
		public override bool IsNotDecorated() => false;

		public ThemedVSplitter()
		{
			SeparatorColor = Theme.Colors.SeparatorColor;
			SeparatorWidth = 1;
			SeparatorActiveAreaWidth = 4;
		}
	}
}
#endif
