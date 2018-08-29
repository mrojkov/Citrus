using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class AudioSamplePropertyEditor : FilePropertyEditor<SerializableSample>
	{
		public AudioSamplePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string[] { "ogg" })
		{
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void AssignAsset(string path)
		{
			SetProperty(new SerializableSample(path));
		}

		protected override string StringValueGetter(SerializableSample obj) {
			return obj.SerializationPath ?? "";
		}

		protected override SerializableSample StringValueSetter(string path) {
			return new SerializableSample(path);
		}
	}
}
