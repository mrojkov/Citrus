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
			ContainerWidget.AddNode(editor);
			editor.Submitted += text => SetComponent(text);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v?.Id);
		}

		void SetComponent(string text)
		{
			SetProperty(new NodeReference<T>(text));
		}

		public override void Submit()
		{
			SetComponent(editor.Text);
		}

	}
}
