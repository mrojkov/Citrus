using Lime;

namespace Tangerine.UI
{
	public class TexturePropertyEditor<T> : FilePropertyEditor<T> where T: ITexture
	{
		public TexturePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string[] { "png" })
		{ }

		protected override void AssignAsset(string path)
		{
			SetProperty(new SerializableTexture(path));
		}

		protected override string ValueToStringConverter(T obj) {
			return (obj as SerializableTexture)?.SerializationPath ?? "";
		}

		protected override T StringToValueConverter(string path) {
			return (T)(ITexture)new SerializableTexture(path);
		}

		protected override bool IsValid(string path)
		{
			return TangerineDefaultCharsetAttribute.IsValidPath(path, out var message) == ValidationResult.Ok;
		}
	}
}
