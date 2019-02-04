using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TriggerPropertyEditor : CommonPropertyEditor<string>
	{
		public TriggerPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			var button = new ThemedButton {
				Text = "...",
				MinMaxWidth = 20,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			EditorContainer.AddNode(button);
			EditorContainer.AddNode(Spacer.HStretch());
			button.Clicked += () => {
				var triggers = new Dictionary<string, HashSet<string>>();
				foreach (var obj in editorParams.Objects) {
					var node = (Node)obj;
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
				}
				var current = new HashSet<string>(
					(CoalescedPropertyValue().GetValue().Value ?? "").Split(',').Select(el => el.Trim()).ToList()
				);
				var window = new TriggerSelectionDialog(triggers, current, s => SetProperty(s));
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
