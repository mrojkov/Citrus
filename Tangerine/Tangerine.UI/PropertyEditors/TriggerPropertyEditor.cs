using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TriggerPropertyEditor : CommonPropertyEditor<string>
	{
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
			var button = new ThemedButton {
				Text = "...",
				MinMaxWidth = 20,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			EditorContainer.AddNode(button);
			EditorContainer.AddNode(Spacer.HStretch());
			button.Clicked += () => {
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
				var window = new TriggerSelectionDialog(triggers, new HashSet<string>(CurrentTriggers), s => SetProperty(s));
			};
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
			comboBox.Enabled = Enabled;
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
