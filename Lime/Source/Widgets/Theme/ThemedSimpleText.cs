#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedSimpleText : SimpleText
	{
		public override bool IsNotDecorated() => false;

		public ThemedSimpleText()
		{
			Decorate(this);
		}

		public ThemedSimpleText(string text) : this()
		{
			Text = text;
		}

		internal static void Decorate(SimpleText text)
		{
			text.ForceUncutText = true;
			text.Localizable = false;
			text.TextColor = Color4.White;
			text.Color = Theme.Colors.BlackText;
			text.Font = new SerializableFont();
			text.FontHeight = Theme.Metrics.TextHeight;
			text.HAlignment = HAlignment.Left;
			text.VAlignment = VAlignment.Top;
			text.OverflowMode = TextOverflowMode.Ellipsis;
			text.TrimWhitespaces = true;
			text.Size = text.MinSize;
		}
	}
}
#endif
