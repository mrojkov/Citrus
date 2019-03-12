using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Panels
{
	public class AnimationsPanel : IDocumentView
	{
		const string legacyAnimationId = "<Legacy>";
		private readonly Widget panelWidget;
		private readonly Frame rootWidget;
		private readonly ThemedScrollView scrollView;
		private readonly float rowHeight = Theme.Metrics.DefaultEditBoxSize.Y;

		public static AnimationsPanel Instance { get; private set; }

		static class Commands
		{
			public static readonly ICommand Up = new Command(Key.Up);
			public static readonly ICommand Down = new Command(Key.Down);
			public static readonly List<ICommand> All = new List<ICommand> { Up, Down };
		}

		public AnimationsPanel(Widget panelWidget)
		{
			Instance = this;
			this.panelWidget = panelWidget;
			scrollView = new ThemedScrollView { TabTravesable = new TabTraversable() };
			this.rootWidget = new Frame {
				Id = "AnimationsPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout { Spacing = 4 },
				Nodes = { scrollView }
			};
			scrollView.Content.CompoundPresenter.Insert(0, new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				var selectedIndex = GetSelectedAnimationIndex();
				Renderer.DrawRect(
					0, rowHeight * selectedIndex,
					w.Width, rowHeight * (selectedIndex + 1),
					scrollView.IsFocused() ? Theme.Colors.SelectedBackground : Theme.Colors.SelectedInactiveBackground);
			}));
			scrollView.Gestures.Add(new ClickGesture(1, ShowContextMenu));
			scrollView.Gestures.Add(new ClickGesture(0, SelectAnimationOnMouseClick));
			scrollView.Gestures.Add(new DoubleClickGesture(0, RenameAnimation));
			scrollView.LateTasks.Add(new KeyRepeatHandler(ScrollView_KeyRepeated));
			scrollView.AddChangeWatcher(CalcAnimationsHashCode, _ => Refresh());
		}

		private void RenameAnimation()
		{
			var index = GetSelectedAnimationIndex();
			var animation = GetAnimations()[index];
			var item = (Widget)scrollView.Content.Nodes[index];
			var label = item["Label"];
			label.Visible = false;
			var editor = new ThemedEditBox();
			label.Parent.Nodes.Insert(label.CollectionIndex(), editor);
			editor.Text = animation.Id;
			editor.SetFocus();
			editor.AddChangeWatcher(() => editor.IsFocused(), focused => {
				if (!focused) {
					editor.Unlink();
					label.Visible = true;
				}
			});
			editor.Submitted += s => {
				RenameAnimationHelper(s.Trim());
				editor.Unlink();
				label.Visible = true;
			};

			void RenameAnimationHelper(string newId)
			{
				string error = null;
				if (animation.IsLegacy) {
					error = "Can't rename legacy animation";
				} else if (newId.IsNullOrWhiteSpace()) {
					error = "Invalid animation name";
				} else if (TangerineDefaultCharsetAttribute.IsValid(newId, out var message) != ValidationResult.Ok) {
					error = message;
				} else if (GetAnimations().Any(a => a.Id == newId)) {
					error = $"An animation '{newId}' already exists";
				}
				if (error != null) {
					UI.AlertDialog.Show(error, "Ok");
					return;
				}
				Document.Current.History.DoTransaction(() => {
					var oldId = animation.Id;
					Core.Operations.SetProperty.Perform(animation, nameof(Animation.Id), newId);
					foreach (var node in animation.Owner.Descendants) {
						if (node.ContentsPath != null) {
							continue;
						}
						foreach (var a in node.Animations) {
							foreach (var track in a.Tracks) {
								foreach (var animator in track.Animators) {
									if (animator.AnimationId == oldId) {
										Core.Operations.SetProperty.Perform(animator, nameof(IAnimator.AnimationId), newId);
									}
								}
							}
						}
						foreach (var animator in node.Animators) {
							if (animator.AnimationId == oldId) {
								Core.Operations.SetProperty.Perform(animator, nameof(IAnimator.AnimationId), newId);
							}
						}
					}
				});
			}
		}

		private void SelectAnimationOnMouseClick()
		{
			scrollView.SetFocus();
			var index = (scrollView.Content.LocalMousePosition().Y / rowHeight).Floor();
			SelectAnimation(index);
		}

		private void ShowContextMenu()
		{
			SelectAnimationOnMouseClick();
			var menu = new Menu();
			menu.Add(new Command("Add", () => AddAnimation(Document.Current.RootNode, false)));
			menu.Add(new Command("Add Compound", () => AddAnimation(Document.Current.RootNode, true)));
			var path = GetNodePath(Document.Current.Container);
			if (!string.IsNullOrEmpty(path)) {
				menu.Add(new Command($"Add To '{path}'", () => AddAnimation(Document.Current.Container, false)));
				menu.Add(new Command($"Add Compound To '{path}'", () => AddAnimation(Document.Current.Container, true)));
			}
			menu.Add(Command.MenuSeparator);
			menu.Add(new Command("Rename", RenameAnimation));
			menu.Add(new Command("Delete", () => {
				Document.Current.History.DoTransaction(() => {
					var index = GetSelectedAnimationIndex();
					if (index > 0) {
						SelectAnimation(index - 1);
					} else {
						SelectAnimation(index + 1);
					}
					var animation = GetAnimations()[index];
					Core.Operations.RemoveFromList.Perform(animation.Owner.Animations, animation.Owner.Animations.IndexOf(animation));
				});
			}) { Enabled = !Document.Current.Animation.IsLegacy });
			menu.Popup();

			void AddAnimation(Node node, bool compound)
			{
				Document.Current.History.DoTransaction(() => {
					var animation = new Animation { Id = GenerateAnimationId(), IsCompound = compound };
					Core.Operations.InsertIntoList.Perform(node.Animations, node.Animations.Count, animation);
					SelectAnimation(GetAnimations().IndexOf(animation));
				});
				panelWidget.Tasks.Add(DelayedRenameAnimation());
			}

			IEnumerator<object> DelayedRenameAnimation()
			{
				yield return null;
				RenameAnimation();
			}

			string GenerateAnimationId()
			{
				for (int i = 1; ; i++) {
					var id = "NewAnimation" + (i > 1 ? i.ToString() : "");
					if (!GetAnimations().Any(a => a.Id == id)) {
						return id;
					}
				}
				throw new System.Exception();
			}
		}

		private static string GetNodePath(Node node)
		{
			var t = "";
			for (var n = node; n != Document.Current.RootNode; n = n.Parent) {
				var id = string.IsNullOrEmpty(n.Id) ? "?" : n.Id;
				t = id + ((t != "") ? ": " + t : t);
			}
			return t;
		}

		private int GetSelectedAnimationIndex() => GetAnimations().IndexOf(Document.Current.Animation);

		private List<Animation> animationsStorage = new List<Animation>();
		private List<Animation> GetAnimations()
		{
			animationsStorage.Clear();
			Document.Current.GetAnimations(animationsStorage);
			return animationsStorage;
		}

		private long CalcAnimationsHashCode()
		{
			var h = new Hasher();
			h.Begin();
			foreach (var a in GetAnimations()) {
				h.Write(a.Id ?? string.Empty);
				h.Write(a.Owner.GetHashCode());
				h.Write(a.IsCompound);
				h.Write(a.IsLegacy);
			}
			return h.End();
		}

		private void ScrollView_KeyRepeated(WidgetInput input, Key key)
		{
			var index = GetSelectedAnimationIndex();
			if (Commands.Down.WasIssued()) {
				scrollView.SetFocus();
				index++;
				SelectAnimation(index);
			}
			if (Commands.Up.WasIssued()) {
				scrollView.SetFocus();
				index--;
				SelectAnimation(index);
			}
			Command.ConsumeRange(Commands.All);
		}

		private void SelectAnimation(int index)
		{
			var a = GetAnimations();
			index = index.Clamp(0, a.Count - 1);
			EnsureRowVisible(index);
			Window.Current.Invalidate();
			Document.Current.History.DoTransaction(() => {
				Core.Operations.SetProperty.Perform(Document.Current, nameof(Document.SelectedAnimation), a[index], isChangingDocument: false);
			});
		}

		private void EnsureRowVisible(int row)
		{
			while ((row + 1) * rowHeight > scrollView.ScrollPosition + scrollView.Height) {
				scrollView.ScrollPosition++;
			}
			while (row * rowHeight < scrollView.ScrollPosition) {
				scrollView.ScrollPosition--;
			}
		}

		public void Refresh()
		{
			int index = GetSelectedAnimationIndex();
			var content = scrollView.Content;
			content.Nodes.Clear();
			var animations = GetAnimations();
			content.Layout = new TableLayout {
				ColumnCount = 1,
				ColumnSpacing = 8,
				RowCount = animations.Count,
				ColumnDefaults = new List<DefaultLayoutCell> { new DefaultLayoutCell { StretchY = 0 } }
			};
			foreach (var a in animations) {
				var label = a.IsLegacy ? legacyAnimationId : (a.IsCompound ? '@' + a.Id : a.Id);
				var path = GetNodePath(a.Owner);
				if (!a.IsLegacy && !string.IsNullOrEmpty(path)) {
					label += " (" + path + ')';
				}
				var item = new Widget {
					MinHeight = rowHeight,
					Padding = new Thickness(2, 10, 0, 0),
					Layout = new HBoxLayout(),
					Nodes = {
						new ThemedSimpleText(label) {
							Id = "Label", LayoutCell = new LayoutCell { VerticalAlignment = VAlignment.Center }
						},
					}
				};
				content.Nodes.Add(item);
			}
			SelectAnimation(index);
		}

		public void Attach()
		{
			panelWidget.PushNode(rootWidget);
			Refresh();
		}

		public void Detach()
		{
			rootWidget.Unlink();
		}
	}
}
