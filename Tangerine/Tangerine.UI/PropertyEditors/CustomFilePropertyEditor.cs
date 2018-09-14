using Lime;

namespace Tangerine.UI
{
	public sealed class CustomFilePropertyEditor<T> : FilePropertyEditor<T>
	{
		private TangerineFilePropertyAttribute filePropertyAttribute;
		public CustomFilePropertyEditor(IPropertyEditorParams editorParams, TangerineFilePropertyAttribute attribute) : base(editorParams, attribute.AllowedFileTypes)
		{
			this.filePropertyAttribute = attribute;
		}

		protected override string ValueToStringConverter(T value) => filePropertyAttribute?.ValueToStringConverter(EditorParams.Type, value) ?? "";

		protected override T StringToValueConverter(string path) => filePropertyAttribute != null ? filePropertyAttribute.StringToValueConverter<T>(EditorParams.Type, path) : default;

		protected override void AssignAsset(string path)
		{
			SetProperty(StringToValueConverter(path));
		}
	}
}
