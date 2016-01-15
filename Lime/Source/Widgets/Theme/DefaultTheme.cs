using System;

namespace Lime
{
	public class DefaultTheme : Theme
	{
		public DefaultTheme()
		{
			Decorators[typeof(SimpleText)] = DecorateSimpleText;
		}

		private void DecorateSimpleText(Widget widget)
		{
			var text = (SimpleText)widget;
			text.Font = new SerializableFont();
			text.FontHeight = 15;
			text.TextColor = Color4.White;
			text.AutoSizeConstraints = true;
			text.Localizable = true;
			text.TrimWhitespaces = true;
			text.Text = "";
		}
	}
}

