using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI.Docking;
using Yuzu;

namespace Tangerine.UI.SceneView
{
	public class VisualHint
	{
		[YuzuRequired]
		public string Title { get; set; }
		[YuzuRequired]
		public bool Enabled { get; set; } = true;
		[YuzuRequired]
		public SortedDictionary<string, VisualHint> SubHints { get; private set; } = new SortedDictionary<string, VisualHint>();

		public Func<VisualHint, bool> HideRule;
		public ICommand Command { get; set; } = null;
		public bool Hidden { get; set; } = true;

		public VisualHint()
		{
		}

		public VisualHint(string title)
		{
			Title = title;
		}

		public bool ShouldBeHidden()
		{
			return HideRule?.Invoke(this) ?? true;
		}

		public static IEnumerable<string> GetNames(string path)
		{
			var names = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var name in names) {
				yield return name;
			}
		}
	}

	public class VisualHintsRegistry
	{
		public static VisualHintsRegistry Instance => SceneUserPreferences.Instance.VisualHintsRegister;

		[YuzuRequired]
		public VisualHint RootHint { get; set; } = new VisualHint("");

		public VisualHint DisplayAll { get; set; }
		public VisualHint DisplayInvisible { get; set; }

		private readonly Dictionary<Type, VisualHint> typeHintMap = new Dictionary<Type, VisualHint>();

		public readonly VisualHint EmptyHint = new VisualHint("");
		public readonly HashSet<VisualHint> AlwaysVisible = new HashSet<VisualHint>();

		public static class HideRules
		{
			public readonly static Func<VisualHint, bool> TypeRule = hint => !Instance.typeHintMap.ContainsValue(hint);
			public readonly static Func<VisualHint, bool> AlwaysVisible = hint => false;
			public readonly static Func<VisualHint, bool> AlwaysHidden = hint => true;
			public readonly static Func<VisualHint, bool> VisibleIfProjectOpened = hint => Project.Current == Project.Null;
		}

		public void EnforceVisible(VisualHint hint)
		{
			AlwaysVisible.Add(hint);
		}

		public VisualHint Register(Type type, ICommand command = null)
		{
			return Register(type, command, HideRules.TypeRule);
		}

		public VisualHint Register(Type type, ICommand command, Func<VisualHint, bool> hideRule)
		{
			if (!(type.GetCustomAttributes(typeof(TangerineVisualHintGroupAttribute), false).FirstOrDefault()
					is TangerineVisualHintGroupAttribute attribute)) {
				return EmptyHint;
			}
			var hint = Register($"{attribute.Group.TrimEnd(' ', '/')}/{attribute.AliasTypeName ?? type.Name}", command, hideRule);
			if (typeHintMap.ContainsKey(type)) {
				return typeHintMap[type] = hint;
			}
			typeHintMap.Add(type, hint);
			return hint;
		}

		public VisualHint FindHint(Type type)
		{
			if (typeHintMap.ContainsKey(type)) {
				return typeHintMap[type];
			}
			return EmptyHint;
		}

		private VisualHint FindHint(string path, bool createNew)
		{
			var hint = RootHint;
			foreach (var value in VisualHint.GetNames(path)) {
				if (!hint.SubHints.ContainsKey(value)) {
					if (!createNew) {
						return EmptyHint;
					}
					var subHint = new VisualHint(value);
					hint.SubHints.Add(value, subHint);
					hint = subHint;
					continue;
				}
				hint = hint.SubHints[value];
			}
			return hint;
		}

		public VisualHint FindHint(string path)
		{
			return FindHint(path, false);
		}

		public VisualHint Register(string path, ICommand command = null, Func<VisualHint, bool> hideRule = null)
		{
			var hint = FindHint(path, true);
			hint.Command = command;
			hint.HideRule = hideRule;
			return hint;
		}

		public bool DisplayCondition(Widget widget)
		{
			var hint = FindHint(widget.GetType());
			return hint.Enabled && (widget.GloballyVisible || DisplayInvisible.Enabled);
		}

		public bool DisplayCondition(Node3D node)
		{
			var hint = FindHint(node.GetType());
			return hint.Enabled && (node.GloballyVisible || DisplayInvisible.Enabled);
		}

		public void Clean()
		{
			RootHint = new VisualHint("");
		}

		public void DoEnforceVisible()
		{
			foreach (var hint in AlwaysVisible) {
				hint.Hidden = false;
			}
		}

		public void RefreshTypeHints()
		{
			typeHintMap.Clear();
			foreach (var type in Project.Current.RegisteredNodeTypes) {
				Register(type);
			}
			RefreshHidden(RootHint);
			DoEnforceVisible();
		}

		private bool RefreshHidden(VisualHint hint)
		{
			bool hidden = hint.ShouldBeHidden();
			foreach (var subHint in hint.SubHints.Values) {
				hidden &= RefreshHidden(subHint);
			}
			hint.Hidden = hidden;
			return hidden;
		}
	}

	public class VisualHintsPanel : IDocumentView
	{
		public static VisualHintsPanel Instance { get; private set; }

		private readonly Panel panel;
		private readonly ThemedScrollView rootWidget;

		public VisualHintsPanel(Panel panel)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = this;
			this.panel = panel;
			rootWidget = new ThemedScrollView();
			rootWidget.Content.Layout = new VBoxLayout { Spacing = 6 };
		}

		public static void Refresh() => Instance?.RefreshEditors();

		private void RefreshEditors()
		{
			rootWidget.Content.Nodes.Clear();
			VisualHintsRegistry.Instance.RefreshTypeHints();
			foreach (var hint in VisualHintsRegistry.Instance.RootHint.SubHints.Values) {
				if (!hint.Hidden) {
					rootWidget.Content.AddNode(new VisualHintEditor(hint));
				}
			}
		}

		public void Detach()
		{
			rootWidget.Unlink();
		}

		public void Attach()
		{
			panel.ContentWidget.AddNode(rootWidget);
		}

		private class BooleanEditor : Widget
		{
			public ThemedCheckBox CheckBox { get; private set; }

			public event Action<CheckBox.ChangedEventArgs> Changed {
				add => CheckBox.Changed += value;
				remove => CheckBox.Changed -= value;
			}

			public bool Checked {
				get => CheckBox.Checked;
				set => CheckBox.Checked = value;
			}

			public event Action CommandIssued;

			public BooleanEditor(string text)
			{
				Layout = new HBoxLayout() { Spacing = 6 };
				Padding = new Thickness { Left = 5 };
				LayoutCell = new LayoutCell(Alignment.LeftCenter, 1);
				CheckBox = new ThemedCheckBox() {
					LayoutCell = new LayoutCell(Alignment.LeftCenter)
				};
				AddNode(CheckBox);
				AddNode(new ThemedSimpleText {
					Text = text,
					LayoutCell = new LayoutCell(Alignment.LeftCenter)
				});
				AddNode(new Widget());
			}

			public BooleanEditor(ICommand command) : this(command.Text)
			{
				AddNode(new ThemedSimpleText(command.Shortcut.ToString().Replace("Unknown", "")) {
					Padding = new Thickness { Right = 10 },
					LayoutCell = new LayoutCell(Alignment.RightCenter)
				});
				CommandHandlerList.Global.Disconnect(command);
				CommandHandlerList.Global.Connect(command, Issued);
			}

			private void Issued()
			{
				Checked = !Checked;
				CommandIssued?.Invoke();
			}
		}

		private class VisualHintEditor : Widget
		{
			public BooleanEditor BooleanEditor { get; private set; }

			private static readonly Widget offsetWidget = new Widget { MinMaxSize = new Vector2(19, 29) };

			private readonly Widget container;
			private readonly ToolbarButton button;
			private readonly VisualHint hint;
			private VisualHintEditor parent;

			public VisualHintEditor(VisualHint hint, float leftOffset = 0f)
			{
				this.hint = hint;
				Layout = new VBoxLayout();

				if (hint.Command != null) {
					BooleanEditor = new BooleanEditor(hint.Command);
				} else {
					BooleanEditor = new BooleanEditor(hint.Title);
				}
				BooleanEditor.Changed += e => CheckHandle(e);
				BooleanEditor.Checked = hint.Enabled;
				BooleanEditor.CommandIssued += () => CheckSelfAndChildren(BooleanEditor.Checked);

				var rowWidget = new Widget {
					Layout = new HBoxLayout(),
					Padding = new Thickness { Left = leftOffset }
				};
				rowWidget.AddNode(hint.SubHints.Count > 0 ? (button = CreateExpandButton()) : offsetWidget.Clone());
				rowWidget.AddNode(BooleanEditor);
				AddNode(rowWidget);
				container = new Widget {
					Layout = new VBoxLayout()
				};
				foreach (var subHint in hint.SubHints.Values) {
					if (subHint.Hidden) {
						continue;
					}
					container.AddNode(new VisualHintEditor(subHint, leftOffset + 23) {
						parent = this
					});
				}
				TryCheckAll();
			}

			private void CheckHandle(CheckBox.ChangedEventArgs e)
			{
				hint.Enabled = e.Value;
				if (e.ChangedByUser) {
					CheckSelfAndChildren(e.Value);
				}
			}

			private void CheckSelfAndChildren(bool value)
			{
				BooleanEditor.Checked = value;
				foreach (var editor in container.Nodes.Cast<VisualHintEditor>()) {
					editor.CheckSelfAndChildren(value);
				}
				parent?.TryCheckAll();
			}

			private void TryCheckAll()
			{
				bool checkedAll = container.Nodes.Count > 0 | BooleanEditor.Checked;
				foreach (var editor in container.Nodes.Cast<VisualHintEditor>()) {
					if (!(checkedAll &= editor.BooleanEditor.Checked)) {
						break;
					}
				}
				BooleanEditor.Checked = checkedAll;
				parent?.TryCheckAll();
			}

			private void ToggleExpanded()
			{
				if (container.Parent != null) {
					container.Unlink();
					button.Texture = IconPool.GetTexture("Timeline.plus");
					return;
				}
				AddNode(container);
				button.Texture = IconPool.GetTexture("Timeline.minus");
			}

			public ToolbarButton CreateExpandButton()
			{
				var button = new ToolbarButton {
					Highlightable = false,
					MinMaxSize = new Vector2(19, 29),
					Padding = new Thickness {
						Left = 5,
						Right = 5,
						Top = 10,
						Bottom = 10
					},
					Texture = IconPool.GetTexture("Timeline.plus")
				};
				button.Clicked += ToggleExpanded;
				return button;
			}
		}
	}
}
