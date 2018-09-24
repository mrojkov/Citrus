using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

//                                     __         __----__
//                                    /  \__..--'' `-__-__''-_
//                                   ( /  \    ``--,,  `-.''''`
//                                   | |   `-..__  .,\    `.
//                     ___           ( '.  \ ____`\ )`-_    `.
//              ___   (   `.         '\   __/   __\' / `:-.._ \
//             (   `-. `.   `.       .|\_  (   / .-| |'.|    ``'
//              `-.   `-.`.   `.     |' ( ,'\ ( (WW| \W)j
//      ..---'''':-`.    `.\   _\   .||  ',  \_\_`/   ``-.
//    ,'      .'` .'_`-,   `  (  |  |''.   `.        \__/
//   /   _  .'  :' ( ```    __ \  \ |   \ ._:7,______.-'
//  | .-'/  : .'  .-`-._   (  `.\  '':   `-\    /
//  '`  /  :' : .: .-''>`-. `-. `   | '.    |  (
//     -  .' :' : /   / _( `_: `_:. `.  `;.  \  \
//     |  | .' : /|  | (___(   (      \   )\  ;  |
//    .' .' | | | `. |   \\\`---:.__-'') /  )/   |
//    |  |  | | |  | |   ///           |/   '    |
//   .' .'  '.'.`; |/ \  /     /             \__/
//   |  |    | | |.|   |      /-,_______\       \
//  /  / )   | | '|' _/      /     |    |\       \
//.:.-' .'  .' |   )/       /     |     | `--,    \
//     /    |  |  / |      |      |     |   /      )
//.__.'    /`  :|/_/|      |      |      | (       |
//`-.___.-`;  / '   |      |      |      |  \      |
//       .:_-'      |       \     |       \  `.___/
//                   \_______)     \_______)

namespace RainbowDash
{
	public class Helper
	{
		public static string TrimTextForId(string text)
		{
			return text.Replace(' ', '_');
		}

		public static void UpdateButtonSizeConstraints(Button b)
		{
			var textPresenter = b.FindNode("TextPresenter") as RichText;
			textPresenter.Size = new Vector2(6666.0f, 6666.0f);
			textPresenter.Text = b.Text;
			var extent0 = textPresenter.MeasureText();
			var w = extent0.Width;
			var h = extent0.Height;
			var i = 2;
			while (true) {
				if (w / i < h * i) {
					i--;
					break;
				}
				i++;
			}
			var size = new Vector2(w / i, h * i);
			textPresenter.Size = size;
			textPresenter.Text = b.Text;
			var extent = textPresenter.MeasureText();
#if WIN
			const int Margin = 4;
			const int MinSize = 32;
#else
			const int Margin = 16;
			const int MinSize = 64;
#endif
			size.X = Mathf.Clamp(extent.Width + Margin, MinSize, float.PositiveInfinity);
			size.Y = Mathf.Clamp(extent.Height + Margin, MinSize, float.PositiveInfinity);
			b.MinSize = size;
			b.MaxSize = Vector2.PositiveInfinity;
			b.Layout.InvalidateConstraintsAndArrangement();
		}

		public static void DecorateButton(Button b, Color4 bgColor, Color4? textColor = null)
		{
			textColor = textColor ?? Color4.Black;
			var textPresenter = b["TextPresenter"] as RichText;
			//(textPresenter.Nodes[0] as TextStyle).TextColor = textColor.Value;
			var bg = b["bg"];
			var aBg = bg.Animators["Color"];
			b.Markers.Add(new Marker("Normal", 0, MarkerAction.Stop));
			aBg.Keys.Add(0, bgColor);
			b.Markers.Add(new Marker("Focus", 10, MarkerAction.Stop));
			aBg.Keys.Add(10, bgColor);
			b.Markers.Add(new Marker("Press", 20, MarkerAction.Stop));
			aBg.Keys.Add(20, bgColor);
			b.Markers.Add(new Marker("Disable", 30, MarkerAction.Stop));
			aBg.Keys.Add(30, Color4.Gray);
		}
	}

	public class NiceColors
	{
		private static readonly Color4[] baseColors = MakeColors(204, 153);
		private static readonly Color4[] accentColors = MakeColors(178, 102);
		private static readonly Color4[] darkTextColors = MakeColors(30, 20, 0.3f);
		public static readonly Color4 DarkerGray = new Color4(32, 32, 32);

		public static Color4 BasicColor(int i)
		{
			return baseColors[i % baseColors.Length];
		}

		public static Color4 AccentColor(int i)
		{
			return accentColors[i % accentColors.Length];
		}

		public static Color4 DarkTextColor(int i)
		{
			return darkTextColors[i % darkTextColors.Length];
		}

		private static Color4[] MakeColors(byte i0, byte i1, float darken = 1.0f)
		{
			i0 = (byte)(darken * i0);
			i1 = (byte)(darken * i1);
			var i2 = (byte)(darken * 255);
			return new[] {
				new Color4(i2, i1, i1),
				new Color4(i2, i0, i1),
				new Color4(i2, i2, i1),
				new Color4(i0, i2, i1),
				new Color4(i1, i2, i1),
				new Color4(i1, i2, i0),
				new Color4(i1, i2, i2),
				new Color4(i1, i0, i2),
				new Color4(i1, i1, i2),
				new Color4(i0, i1, i2),
				new Color4(i2, i1, i2),
				new Color4(i2, i1, i0)
			};
		}
	}

	// TODO: expose setting font, size etc
	public class Item
	{
		public Section Parent;
		private readonly Button button;
		public Action Action;
		public Func<bool> Enabled;

		public string Text
		{
			get { return button.Text; }
			set
			{
				button.Text = value;
				Helper.UpdateButtonSizeConstraints(button);
			}
		}

		public string CheatIdText()
		{
			if (Parent == null) {
				return "./" + button.Id;
			}
			return string.Join("/", Parent.CheatIdText(), button.Id);
		}

		public Item(Button button)
		{
			this.button = button;
		}
	}

	public class Section
	{
		private readonly int colorIndex;

		public readonly Menu Menu;
		public Widget ItemPanel;
		public Section Parent;
		public int Id = -1;
		public int Depth;
		public string IdText = ".";

		public string CheatIdText()
		{
			return Parent == null ? IdText : string.Join("/", Parent.CheatIdText(), IdText);
		}

		public Section(Menu menu, Widget itemPanel, int colorIndex)
		{
			Menu = menu;
			ItemPanel = itemPanel;
			this.colorIndex = colorIndex;
		}

		public Section SubSection(string name)
		{
			return Menu.Section(Id, name, Depth);
		}

		public Item Item(string text, Action action, Func<bool> enabled = null)
		{
			var b = Menu.CreateItemButton();
			Helper.DecorateButton(b, NiceColors.BasicColor(colorIndex));
			b.Id = Helper.TrimTextForId(text);
			var item = new Item(b) {
				Text = text,
				Action = action,
				Enabled = enabled ?? (() => true),
				Parent = this
			};
			Menu.RegisterCheatButton(item.CheatIdText(), b);
			{
				b.Text = item.Text;
				Helper.UpdateButtonSizeConstraints(b);
			}
			b.Clicked = () => {
				item.Action();
				Menu.Hide();
			};
			b.Enabled = item.Enabled();
			ItemPanel.AddNode(b);
			return item;
		}
	}

	public class Menu
	{
		private static Menu instance;

		private readonly List<Button> foldButtons = new List<Button>();
		private readonly List<Widget> itemPanels = new List<Widget>();
		private readonly List<int> parents = new List<int>();
		private readonly Dictionary<string, Button> cheatButtons = new Dictionary<string, Button>();
		private readonly Frame topContainer;
		private readonly Widget worldContainer;
		private ScrollView listView;
		private int nextColorIndex;
		public Widget Root { get { return topContainer; } }

		public event Action Hidden;
		public bool IsShown { get { return instance != null; } }

		public void RegisterCheatButton(string id, Button button)
		{
			cheatButtons[id] = button;
		}

		public static List<string> GetCheatList()
		{
			return instance == null ? new List<string>() : instance.cheatButtons.Keys.ToList();
		}

		public static void Cheat(string id)
		{
			if (instance == null) {
				return;
			}

			foreach (var kv in instance.cheatButtons.Where(kv => kv.Key.StartsWith(id))) {
				kv.Value.Clicked();
			}
		}

		public Menu(Widget container, int layer)
		{
			worldContainer = container;
			topContainer = new Frame {
				Anchors = Anchors.LeftRightTopBottom,
				Size = container.Size - Vector2.One * 80
			};
			var listContainer = (Frame)topContainer.Clone();
			var back = new Widget {
				Anchors = Anchors.LeftRightTopBottom,
				Size = topContainer.Size + Vector2.One * 20,
				Shader = ShaderId.Silhuette,
				Color = Color4.DarkGray,
				Presenter = new WidgetFlatFillPresenter(Color4.Gray.Transparentify(0.3f))
			};
			topContainer.Layer = layer;
			var w = new Widget() { Layout = new VBoxLayout(), Size = topContainer.Size, Anchors = Anchors.LeftRightTopBottom };
			w.AddNode(listContainer);
			w.AddNode(CreateToolPanel());
			topContainer.AddNode(w);
			topContainer.AddNode(back);
			back.CenterOnParent();
			listView = new ScrollView(listContainer);
			listView.Content.Layout = new VBoxLayout();
			topContainer.LateTasks.Add(ExitTask);
			topContainer.SetFocus();
		}

		private IEnumerator<object> ExitTask()
		{
			var input = topContainer.Input;
			while (true) {
				if (input.WasKeyPressed(Key.Escape)) {
					Hide();
				}
				yield return null;
			}
		}

		public void Show()
		{
			instance = this;
			for (var i = 0; i < parents.Count; i++) {
				Fold(i, false);
			}
			worldContainer.AddNode(topContainer);
			topContainer.CenterOnParent();
			topContainer.Input.RestrictScope();
			topContainer.Tasks.Add(RefreshTask);
		}

		private void Fold(int id, bool visible)
		{
			if (foldButtons[id] == null) {
				itemPanels[id].Visible = true;
				return;
			}
			itemPanels[id].Visible = visible;
			var pQueue = new Queue<int>();
			pQueue.Enqueue(id);
			var depth = 1;
			Action<int, bool> updateArrow = (j, v) => {
				if (foldButtons[j] == null) {
					return;
				}
				var arrow = foldButtons[j]["arrow"] as RichText;
				arrow.Text = !v ? string.Concat(Enumerable.Repeat("&gt;  ", 1)) : string.Concat(Enumerable.Repeat("\\/  ", 1));
			};
			updateArrow(id, visible);
			while (pQueue.Count != 0) {
				id = pQueue.Dequeue();
				for (var i = 0; i < parents.Count; i++) {
					if (parents[i] != id || foldButtons[i] == null) {
						continue;
					}

					var v = depth == 1 && visible;
					foldButtons[i].Visible = v;
					itemPanels[i].Visible = false;
					updateArrow(i, false);
					pQueue.Enqueue(i);
				}
				depth++;
			}
		}

		private int AddNode(Button foldButton, Widget itemPanel, int parent = -1)
		{
			if (parents.Count != foldButtons.Count || foldButtons.Count != itemPanels.Count) {
				throw new InvalidOperationException();
			}

			foldButtons.Add(foldButton);
			itemPanels.Add(itemPanel);
			parents.Add(parent);
			return parents.Count - 1;
		}

		public Section Section(string text)
		{
			var itemPanel = CreateItemPanel();
			var section = new Section(this, itemPanel, nextColorIndex) {
				IdText = Helper.TrimTextForId(text)
			};
			var foldButton = CreateFoldButton(text);
			foldButton.Id = Helper.TrimTextForId(text);
			itemPanel.Id = Helper.TrimTextForId(text);
			var arrow = foldButton["arrow"] as RichText;
			Helper.DecorateButton(foldButton, NiceColors.AccentColor(nextColorIndex), NiceColors.DarkTextColor(nextColorIndex));
			//(arrow.Nodes[0] as TextStyle).TextColor = NiceColors.DarkTextColor(nextColorIndex);
			section.Id = AddNode(foldButton, itemPanel);
			foldButton.Clicked = () => { Fold(section.Id, !itemPanel.Visible); };
			listView.Content.AddNode(foldButton);
			listView.Content.AddNode(itemPanel);
			nextColorIndex++;
			return section;
		}

		// Unfoldable root level set of items with no title
		public Section Section()
		{
			var itemPanel = CreateItemPanel();
			itemPanel.Id = ".";
			var section = new Section(this, itemPanel, nextColorIndex) {
				Id = foldButtons.Count
			};
			AddNode(null, itemPanel);
			listView.Content.AddNode(itemPanel);
			nextColorIndex++;
			return section;
		}

		public Section Section(int parentId, string name, int depth)
		{
			var s = Section(name);
			s.ItemPanel.Id = s.IdText = Helper.TrimTextForId(name);
			s.Depth = depth + 1;
			var t = new Thickness {
				Left = 16 * (depth) + 8,
				Right = 8,
				Top = 8,
				Bottom = 8
			};
			var id = parents.Count - 1;
			itemPanels[id].Padding = t;
			if (foldButtons[id] != null) {
				foldButtons[id].Padding = t;
				var p = (foldButtons[id]["padding"] as Image);
				p.MinWidth = p.MaxWidth = t.Left;
			}
			parents[id] = parentId;
			return s;
		}

		public void Hide()
		{
			instance = null;
			topContainer.UnlinkAndDispose();
			if (Hidden != null) {
				Hidden();
			}
		}

		private static IEnumerator<object> RefreshTask()
		{
			while (true) {
				yield return null;
			}
		}

		public static Button CreateFoldButton(string text)
		{
#if WIN
			const int FontSize = 36;
			const int SpaceAfter = -16;
			const float Height = 40.0f;
#else
			const int FontSize = 64;
			const int SpaceAfter = -32;
			const float Height = 75.0f;
#endif
			var b = new Button {
				Text = text,
				Height = Height,
				MinHeight = Height
			};
			var textPresenter = new RichText {
				Id = "TextPresenter",
				Nodes = {
					new TextStyle {
						Font = new SerializableFont("Text"),
						Size = FontSize,
						Tag = "1",
						SpaceAfter = SpaceAfter
					}
				},
				Height = Height,
				HAlignment = HAlignment.Left,
				VAlignment = VAlignment.Center
			};
			var bg = new Image {
				Id = "bg",
				Shader = ShaderId.Silhuette,
				Height = Height,
				Anchors = Anchors.LeftRightTopBottom,
				Color = Color4.White
			};
			var arrow = new RichText {
				Id = "arrow",
				Nodes = {
					new TextStyle {
						Font = new SerializableFont("Text"),
						Tag = "1",
						Size = FontSize,
					}
				},
				Height = Height,
				VAlignment = VAlignment.Center,
				HAlignment = HAlignment.Right,
				MinWidth = Height,
				MaxWidth = Height
			};
			var padding = new Image {
				Id = "padding",
				MinWidth = 0,
				MaxWidth = 0,
				Shader = ShaderId.Silhuette,
				Color = Color4.DarkGray
			};
			var w = new Widget {
				Height = Height,
				Anchors = Anchors.LeftRightTopBottom,
				Layout = new HBoxLayout()
			};
			w.AddNode(padding);
			w.AddNode(arrow);
			w.AddNode(textPresenter);
			b.AddNode(w);
			b.AddNode(bg);
			return b;
		}

		public static Button CreateItemButton()
		{
#if WIN
			const int FontSize = 36;
			const int SpaceAfter = -16;
#else
			const int FontSize = 72;
			const int SpaceAfter = -32;
#endif
			var b = new Button {
				LayoutCell = new LayoutCell(),
				Layout = new StackLayout(),
			};
			var textPresenter = new RichText {
				Id = "TextPresenter",
				WordSplitAllowed = false,
				Nodes = {
					new TextStyle {
						Font = new SerializableFont("Text"),
						Tag = "1",
						Size = FontSize,
						SpaceAfter = SpaceAfter
					}
				},
				HAlignment = HAlignment.Center,
				VAlignment = VAlignment.Center
			};
			var bg = new Image {
				Id = "bg",
				Shader = ShaderId.Silhuette,
				Color = Color4.White
			};
			b.AddNode(textPresenter);
			b.AddNode(bg);
			return b;
		}

		public static Widget CreateItemPanel()
		{
			var w = new Widget {
				Padding = new Thickness(8.0f),
				Layout = new FlowLayout { Spacing = 4.0f }
			};
			return w;
		}

		private Widget CreateToolPanel()
		{
			const float Height = 50.0f;
			var w = new Widget {
				Height = Height,
				MinHeight = Height,
				MaxHeight = Height,
				Layout = new HBoxLayout() { Spacing = 8.0f }
			};
			var btnClose = CreateItemButton();
			var btnFoldAll = CreateItemButton();
			var btnUnfoldAll = CreateItemButton();
			btnClose.Id = "cheat_menu_close";
			btnFoldAll.Id = "cheat_menu_fold_all";
			btnUnfoldAll.Id = "cheat_menu_unfold_all";
			Helper.DecorateButton(btnClose, NiceColors.DarkerGray, Color4.White);
			Helper.DecorateButton(btnFoldAll, NiceColors.DarkerGray, Color4.White);
			Helper.DecorateButton(btnUnfoldAll, NiceColors.DarkerGray, Color4.White);
			btnClose.MaxHeight = Height;
			btnFoldAll.MaxHeight = Height;
			btnUnfoldAll.MaxHeight = Height;
			btnClose.Text = "X";
			btnFoldAll.Text = "FoldAll";
			btnUnfoldAll.Text = "UnfoldAll";
			w.AddNode(btnFoldAll);
			w.AddNode(btnUnfoldAll);
			w.AddNode(btnClose);
			btnClose.Clicked = Hide;
			Action<bool> fold = (foldOrUnfold) => {
				for (var i = 0; i < foldButtons.Count; i++) {
					var fb = foldButtons[i];
					if (fb == null) {
						continue;
					}
					var panel = itemPanels[i];
					if (panel.Visible ^ foldOrUnfold) {
						fb.Clicked();
					}
				}
			};
			btnFoldAll.Clicked = () => { fold(false); };
			btnUnfoldAll.Clicked = () => { fold(true); };
			return w;
		}
	}
}

