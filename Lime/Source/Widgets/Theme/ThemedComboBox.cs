#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedComboBox : ComboBox
	{
		public override bool IsNotDecorated() => false;

		public ThemedComboBox()
		{
			MinSize = Theme.Metrics.DefaultButtonSize;
			MaxHeight = Theme.Metrics.DefaultButtonSize.Y;
			CompoundPresenter.Add(new ThemedDropDownList.DropDownListPresenter(this));
			var editBox = new ThemedEditBox { Id = "TextWidget" };
			AddNode(editBox);
			editBox.ExpandToContainerWithAnchors();
			editBox.Width -= ThemedDropDownList.DropDownListPresenter.IconWidth;
		}
	}
}
#endif
