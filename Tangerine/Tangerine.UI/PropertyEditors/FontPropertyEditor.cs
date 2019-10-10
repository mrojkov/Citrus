using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class FontPropertyEditor : CommonPropertyEditor<SerializableFont>
	{
		private DropDownList selector;
		private static string defaultFontDirectory = "Fonts/";

		public FontPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			selector = editorParams.DropDownListFactory();
			selector.LayoutCell = new LayoutCell(Alignment.Center);
			EditorContainer.AddNode(selector);
			var propType = editorParams.PropertyInfo.PropertyType;
			var items = AssetBundle.Current.EnumerateFiles(defaultFontDirectory).
				Where(i => i.EndsWith(".fnt") || i.EndsWith(".tft") || i.EndsWith(".cft")).
				Select(i => new DropDownList.Item(FontPool.ExtractFontNameFromPath(i, defaultFontDirectory)));
			foreach (var i in items) {
				selector.Items.Add(i);
			}

			var current = CoalescedPropertyValue().GetValue();
			selector.Text = current.IsDefined ? GetFontName(current.Value) : ManyValuesText;
			selector.Changed += a => {
				SetProperty(new SerializableFont((string)a.Value));
			};
			selector.AddChangeLateWatcher(CoalescedPropertyValue(), i => {
				selector.Text = i.IsDefined ? GetFontName(i.Value): ManyValuesText;
			});
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			selector.Enabled = Enabled;
		}

		private static string GetFontName(SerializableFont i)
		{
			return string.IsNullOrEmpty(i?.Name) ? "Default" : (i.Name.EndsWith(".fnt") || i.Name.EndsWith(".tft") || i.Name.EndsWith(".cft")) ? UpdateFontName(i.Name) : i.Name;
		}

		private static string GetFontNameWithoutExtension(SerializableFont i)
		{
			return GetFontNameWithoutExtension(GetFontName(i));
		}

		private static string GetFontNameWithoutExtension(string s)
		{
			return Path.ChangeExtension(s, null);
		}

		private static string UpdateFontName(string s)
		{
			return
				FontPool.TryGetOrUpdateBundleFontPath(s, out var path, ".fnt", ".tft", ".cft") ?
				FontPool.ExtractFontNameFromPath(path, defaultFontDirectory) :
				"Default";
		}
	}
}
