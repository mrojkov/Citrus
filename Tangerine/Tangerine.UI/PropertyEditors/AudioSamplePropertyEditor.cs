using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class AudioSamplePropertyEditor : FilePropertyEditor<SerializableSample>
	{
		public AudioSamplePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string[] { "ogg" })
		{ }

		protected override void AssignAsset(string path)
		{
			SetProperty(new SerializableSample(path));
		}

		protected override string ValueToStringConverter(SerializableSample obj) {
			return obj?.SerializationPath ?? "";
		}

		protected override SerializableSample StringToValueConverter(string path) {
			return new SerializableSample(path);
		}
	}
}
