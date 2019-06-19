using Lime;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingStatusBar : Widget
	{
		private readonly ThemedSimpleText simpleText;

		public new string Text
		{
			get => simpleText.Text;
			set => simpleText.Text = value;
		}

		public RemoteScriptingStatusBar()
		{
			simpleText = new ThemedSimpleText {
				HAlignment = HAlignment.Left,
				VAlignment = VAlignment.Center,
				OverflowMode = TextOverflowMode.Default,
				WordSplitAllowed = false,
				Padding = new Thickness(5, 2),
			};
			PushNode(simpleText);
			simpleText.ExpandToContainerWithAnchors();
		}
	}
}
