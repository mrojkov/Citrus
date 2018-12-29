using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ShortcutPropertyEditor : CommonPropertyEditor<Shortcut>
	{
		private EditBox editor;
		private Modifiers modifiers;
		private Key main;

		private WidgetFlatFillPresenter flatFillPresenter;

		public Action PropertyChanged { get; set; }

		private void SetValue(Shortcut value)
		{
			var oldValue = CoalescedPropertyValue().GetValue().Value;
			SetProperty(value);
			if (value != oldValue) {
				PropertyChanged?.Invoke();
			}
		}

		public ShortcutPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.Updating += Updating;
			editor.Updated += Updated;
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => {
				var text = v.Value.ToString();
				editor.Text = v.IsDefined ? v.Value.Main != Key.Unknown ? text : text.Replace("Unknown", "") : ManyValuesText;
			});
			editor.IsReadOnly = true;
			editor.TextWidget.Tasks.Clear();
			editor.TextWidget.Position = new Vector2(0, editor.MinHeight / 2);
			editor.TextWidget.Padding = new Thickness(5, 0);
			editor.Gestures.Add(new ClickGesture(() => editor.SetFocus()));
			editor.Gestures.Add(new ClickGesture(1, () => {
				main = Key.Unknown;
				modifiers = Modifiers.None;
				SetValue(new Shortcut(modifiers, main));
			}));
			editor.AddToNode(EditorContainer);

			PropertyLabel.Tasks.Clear();
			PropertyLabel.Tasks.Add(ManageLabelFocus());
			ContainerWidget.Tasks.Add(ManageFocusTask());

			var value = CoalescedPropertyValue().GetValue();
			main = value.Value.Main;
			modifiers = value.Value.Modifiers;
			flatFillPresenter = new WidgetFlatFillPresenter(Theme.Colors.GrayBackground);
			ContainerWidget.CompoundPresenter.Add(flatFillPresenter);
		}

		private void PressModifier(Modifiers modifier, Key key)
		{
			var input = editor.Input;
			if (input.IsKeyPressed(key)) {
				modifiers |= modifier;
			}
		}

		IEnumerator<object> ManageLabelFocus()
		{
			while (true) {
				if (PropertyLabel.Input.WasMousePressed()) {
					PropertyLabel.SetFocus();
				}
				yield return null;
			}
		}

		IEnumerator<object> ManageFocusTask()
		{
			while (true) {
				if (PropertyLabel.IsFocused()) {
					editor.SetFocus();
				}
				flatFillPresenter.Color = editor.IsFocused() ? Theme.Colors.SelectedBackground : Theme.Colors.GrayBackground;
				yield return null;
			}
		}

		private void Updating(float dt)
		{
			if (!editor.IsFocused())
				return;
			var input = editor.Input;
			var keys = Key.Enumerate().Where(k => input.WasKeyPressed(k));
			if (!keys.Any())
				return;
			modifiers = Modifiers.None;

			PressModifier(Modifiers.Alt, Key.Alt);
			PressModifier(Modifiers.Shift, Key.Shift);
			PressModifier(Modifiers.Control, Key.Control);
			PressModifier(Modifiers.Win, Key.Win);
			foreach (var key in keys) {
				if (!key.IsModifier() && !key.IsMouseKey() && Shortcut.ValidateMainKey(key)) {
					main = key;
					SetValue(new Shortcut(modifiers, main));
					return;
				}
			}
		}

		private void Updated(float dt)
		{
			if (!editor.IsFocused())
				return;
			var input = editor.Input;
			input.ConsumeKeys(Key.Enumerate().Where(
				k => input.WasKeyRepeated(k) || input.WasKeyPressed(k) || input.WasKeyReleased(k)));
			Command.ConsumeRange(Command.Editing);
		}
	}
}
