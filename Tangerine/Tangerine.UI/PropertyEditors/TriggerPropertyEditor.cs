using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TriggerPropertyEditor : CommonPropertyEditor<string>
	{
		private readonly EditBox editBox;
		private readonly Button button;

		private List<string> CurrentTriggers
		{
			get
			{
				return (CoalescedPropertyValue().GetValue().Value ?? "").
					Split(',').
					Select(el => el.Trim()).
					ToList();
			}
		}

		public TriggerPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			if (EditorParams.Objects.Skip(1).Any()) {
				EditorContainer.AddNode(CreateWarning("Edit of triggers isn't supported for multiple selection."));
				return;
			}
			var node = (Node)editorParams.Objects.First();
			if (!EnsureMarkersAvailable(node)) {
				EditorContainer.AddNode(CreateWarning("No markers to select from."));
				return;
			}
			button = new ThemedButton {
				Text = "...",
				MinMaxWidth = 20,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			EditorContainer.AddNode(editBox = editorParams.EditBoxFactory());
			EditorContainer.AddNode(Spacer.HSpacer(4));
			EditorContainer.AddNode(button);
			EditorContainer.AddNode(Spacer.HStretch());
			editBox.Text = CoalescedPropertyValue().GetValue().Value;
			editBox.Submitted += text => {
				EditBox_Submitted(text, node);
			};
			button.Clicked += () => {
				var window = new TriggerSelectionDialog(
					GetAvailableTriggers(node),
					new HashSet<string>(CurrentTriggers),
					s => {
						SetProperty(s);
						editBox.Text = CoalescedPropertyValue().GetValue().Value;
					}
				);
			};
		}

		private Dictionary<string, HashSet<string>> GetAvailableTriggers(Node node)
		{
			var triggers = new Dictionary<string, HashSet<string>>();
			foreach (var a in node.Animations) {
				foreach (var m in a.Markers.Where(i => i.Action != MarkerAction.Jump && !string.IsNullOrEmpty(i.Id))) {
					var id = a.Id != null ? m.Id + '@' + a.Id : m.Id;
					var key = a.Id ?? "Primary";
					if (!triggers.Keys.Contains(key)) {
						triggers[key] = new HashSet<string>();
					}
					if (!triggers[key].Contains(id)) {
						triggers[key].Add(id);
					}
				}
			}
			return triggers;
		}

		private bool EnsureMarkersAvailable(Node node)
		{
			foreach (var a in node.Animations) {
				if (a.Markers.Where(i => i.Action != MarkerAction.Jump && !string.IsNullOrEmpty(i.Id)).ToList().Count > 0) {
					return true;
				}
			}
			return false;
		}

		private Widget CreateWarning(string message)
		{
			return new Widget() {
				Layout = new HBoxLayout(),
				Nodes = {
					new ThemedSimpleText {
						Text = message,
						Padding = Theme.Metrics.ControlsPadding,
						LayoutCell = new LayoutCell(Alignment.Center),
						VAlignment = VAlignment.Center,
						ForceUncutText = false
					}
				},
				Presenter = new WidgetFlatFillPresenter(Theme.Colors.WarningBackground)
			};
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editBox.Enabled = button.Enabled = Enabled;
		}

		private void EditBox_Submitted(string text, Node node)
		{
			var newValue = "";
			var triggersToSet = text.Split(',').ToList();
			var triggers = GetAvailableTriggers(node);
			var newTriggers = new Dictionary<string, string>();
			foreach (var key in triggers.Keys) {
				foreach (var trigger in triggersToSet) {
					if (triggers[key].Contains(trigger.Trim(' '))) {
						newValue += $"{trigger.Trim(' ')},";
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(newValue)) {
				newValue = newValue.Trim(',');
			}
			editBox.Text = newValue;
			SetProperty(newValue);
		}

		protected static void SplitTrigger(string trigger, out string markerId, out string animationId)
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
				SplitTrigger(x, out _, out var xAnimation);
				SplitTrigger(y, out _, out var yAnimation);
				return xAnimation == yAnimation;
			}

			public int GetHashCode(string obj)
			{
				SplitTrigger(obj, out _, out var animation);
				return animation == null ? 0 : animation.GetHashCode();
			}
		}
	}
}
