using Lime;

namespace Tangerine.UI
{
	public class RenderTargetPropertyEditor : EnumPropertyEditor<RenderTarget>
	{
		private const string SmallTexDesc = " (256x256)";
		private const string MiddleTexDesc = " (512x512)";
		private const string LargeTexDesc = " (1024x1024)";

		public RenderTargetPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			Selector.Items[1].Text += SmallTexDesc;
			Selector.Items[2].Text += SmallTexDesc;
			Selector.Items[3].Text += MiddleTexDesc;
			Selector.Items[4].Text += LargeTexDesc;
			Selector.Items[5].Text += LargeTexDesc;
			Selector.Items[6].Text += LargeTexDesc;
			Selector.Items[7].Text += LargeTexDesc;
		}
	}
}
