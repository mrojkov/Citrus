using System.Collections.Generic;
using Lime;

namespace Tangerine.UI
{
	public class BlendingPropertyEditor : EnumPropertyEditor<Blending>
	{
		private static readonly Dictionary<string, string> blendingToPhotoshopAnalog = new Dictionary<string, string> {
			{Blending.Alpha.ToString(), "Normal"},
			{Blending.Add.ToString(), "Linear Dodge"},
			{Blending.Glow.ToString(), "Normal with Brightness"},
			{Blending.Modulate.ToString(), "Multiply without Transparency"},
			{Blending.Burn.ToString(), "Multiply"},
			{Blending.Darken.ToString(), "Normal with Darkness"},
			{Blending.Opaque.ToString(), "Normal without Transparency"},
		};

		public BlendingPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			foreach (var item in Selector.Items) {
				string photoshopAnalog;
				if (blendingToPhotoshopAnalog.TryGetValue(item.Text, out photoshopAnalog)) {
					item.Text += $" ({photoshopAnalog})";
				}
			}
		}
	}
}
