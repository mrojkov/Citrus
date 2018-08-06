using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class FontPropertyEditor : CommonPropertyEditor<SerializableFont>
	{
		private DropDownList selector;

		public FontPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			selector = editorParams.DropDownListFactory();
			selector.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(selector);
			var propType = editorParams.PropertyInfo.PropertyType;
			var items = AssetBundle.Current.EnumerateFiles("Fonts").
				Where(i => i.EndsWith(".fnt") || i.EndsWith(".tft")).
				Select(i => new DropDownList.Item(Path.ChangeExtension(Path.GetFileName(i), null)));
			foreach (var i in items) {
				selector.Items.Add(i);
			}
			selector.Text = GetFontName(CoalescedPropertyValue().GetValue());
			selector.Changed += a => {
				SetProperty(new SerializableFont((string)a.Value));
			};
			selector.AddChangeWatcher(CoalescedPropertyValue(), i => {
				selector.Text = GetFontName(i);
			});
		}

		private static string GetFontName(SerializableFont i)
		{
			return string.IsNullOrEmpty(i?.Name) ? "Default" : i.Name;
		}
	}
}