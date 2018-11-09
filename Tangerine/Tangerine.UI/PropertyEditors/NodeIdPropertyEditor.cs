using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class NodeIdPropertyEditor : CommonPropertyEditor<string>
	{
		private EditBox editor;

		public NodeIdPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Editor.EditorParams.MaxLines = 1;
			EditorContainer.AddNode(editor);
			bool textValid = true;
			editor.AddChangeWatcher(() => editor.Text, text => textValid = IsValid(text));
			editor.CompoundPostPresenter.Add(new SyncDelegatePresenter<EditBox>(editBox => {
				if (!textValid) {
					editBox.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, editBox.Size, Color4.Red.Transparentify(0.8f));
				}
			}));
			editor.Submitted += SetValue;
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v.IsUndefined ? v.Value : ManyValuesText);
		}

		private void SetValue(string value)
		{
			if (!IsValid(value)) {
				AlertDialog.Show($"Field contains characters other than latin letters and digits.");
			} else {
				SetProperty(editor.Text);
			}
			editor.Text = SameValues() ? PropertyValue(EditorParams.Objects.First()).GetValue() : ManyValuesText;
		}

		protected virtual bool IsValid(string value)
		{
			foreach (var c in value) {
				if (!validChars.Contains(c)) {
					return false;
				}
			}
			return true;
		}

		private static readonly char [] validSpecialSymbols = { '_', '.', '>', '@', '[', ']', '#' };
		private static readonly List<char> validChars =
			Enumerable.Range(1, 128).Select(i => (char)i).
			Where(c => char.IsLetterOrDigit(c) || validSpecialSymbols.Contains(c)).ToList();
	}
}
