using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class NodeReferencePropertyEditor<T> : CommonPropertyEditor<NodeReference<T>> where T : Node
	{
		private EditBox editor;

		public NodeReferencePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			var propName = editorParams.PropertyName;
			if (propName.EndsWith("Ref")) {
				PropertyLabel.Text = propName.Substring(0, propName.Length - 3);
			}
			editor = editorParams.EditBoxFactory();
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			EditorContainer.AddNode(editor);
			editor.Submitted += SetComponent;
			var current = CoalescedPropertyValue();
			editor.AddChangeLateWatcher(current, v => {
				editor.Text = v.IsDefined ? v.Value?.Id: ManyValuesText;
			});
			ManageManyValuesOnFocusChange(editor, current);
		}

		void SetComponent(string text)
		{
			SetProperty(new NodeReference<T>(text));
		}

		public override void Submit()
		{
			SetComponent(editor.Text);
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editor.Enabled = Enabled;
		}
	}
}
