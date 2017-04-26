using System;

namespace Lime
{
	public class DefaultTheme : Theme
	{
		public static readonly ITheme Instance = new DefaultTheme();

		public DefaultTheme()
		{
			Decorators[typeof(SimpleText)] = DecorateSimpleText;
			Decorators[typeof(RichText)] = DecorateRichText;
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
			text.SpriteListElementHandler = Lime.ShaderPrograms.ColorfulTextShaderProgram.HandleSimpleTextSprite;
		}

		private void DecorateRichText(Widget widget)
		{
			var text = (RichText)widget;
			text.Localizable = true;
			text.TrimWhitespaces = true;
			text.Text = "";
			text.SpriteListElementHandler = Lime.ShaderPrograms.ColorfulTextShaderProgram.HandleRichTextSprite;
		}
	}
}

