using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TriggerPropertyEditor : CommonPropertyEditor<string>
	{
		private ComboBox comboBox;

		public TriggerPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			comboBox = new ThemedComboBox { LayoutCell = new LayoutCell(Alignment.Center) };
			EditorContainer.AddNode(comboBox);
			EditorContainer.AddNode(Spacer.HStretch());
			comboBox.Changed += ComboBox_Changed;
			foreach (var obj in editorParams.Objects) {
				var node = (Node)obj;
				foreach (var a in node.Animations) {
					foreach (var m in a.Markers.Where(i => i.Action != MarkerAction.Jump && !string.IsNullOrEmpty(i.Id))) {
						var id = a.Id != null ? m.Id + '@' + a.Id : m.Id;
						if (!comboBox.Items.Any(i => i.Text == id)) {
							comboBox.Items.Add(new DropDownList.Item(id));
						}
					}
				}
			}
			comboBox.AddChangeWatcher(CoalescedPropertyValue(), v => comboBox.Text = v);
		}

		void ComboBox_Changed(DropDownList.ChangedEventArgs args)
		{
			if (!args.ChangedByUser)
				return;
			var newTrigger = (string)args.Value;
			var currentTriggers = CoalescedPropertyValue().GetValue();
			if (string.IsNullOrWhiteSpace(currentTriggers) || args.Index < 0) {
				// Keep existing and remove absent triggers after hand input.
				var availableTriggers = new HashSet<string>(comboBox.Items.Select(item => item.Text));
				var setTrigger = string.Join(
					",",
					newTrigger.
						Split(',').
						Select(el => el.Trim()).
						Where(el => availableTriggers.Contains(el)).
						Distinct(new TriggerStringComparer())
				);

				SetProperty(setTrigger.Length == 0 ? null : setTrigger);
				if (setTrigger != newTrigger) {
					comboBox.Text = setTrigger;
				}
				return;
			}
			var triggers = new List<string>();
			var added = false;
			string newMarker, newAnimation;
			SplitTrigger(newTrigger, out newMarker, out newAnimation);
			foreach (var trigger in currentTriggers.Split(',').Select(i => i.Trim())) {
				string marker, animation;
				SplitTrigger(trigger, out marker, out animation);
				if (animation == newAnimation) {
					if (!added) {
						added = true;
						triggers.Add(newTrigger);
					}
				} else {
					triggers.Add(trigger);
				}
			}
			if (!added) {
				triggers.Add(newTrigger);
			}
			var newValue = string.Join(",", triggers);
			SetProperty(newValue);
			comboBox.Text = newValue;
		}

		private static void SplitTrigger(string trigger, out string markerId, out string animationId)
		{
			if (!trigger.Contains('@')) {
				markerId = trigger;
				animationId = null;
			} else {
				var t = trigger.Split('@');
				markerId = t[0];
				animationId = t[1];
			}
		}

		private class TriggerStringComparer : IEqualityComparer<string>
		{

			public bool Equals(string x, string y)
			{
				string xMarker;
				string yMarker;
				string xAnimation;
				string yAnimation;
				SplitTrigger(x, out xMarker, out xAnimation);
				SplitTrigger(y, out yMarker, out yAnimation);
				return xAnimation == yAnimation;
			}

			public int GetHashCode(string obj)
			{
				string marker;
				string animation;
				SplitTrigger(obj, out marker, out animation);
				return animation == null ? 0 : animation.GetHashCode();
			}
		}

	}
}
