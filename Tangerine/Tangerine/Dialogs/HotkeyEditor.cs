using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine.Dialogs
{
	public class HotkeyEditor : Widget
	{
		private readonly Dictionary<Key, KeyboardButton> buttons = new Dictionary<Key, KeyboardButton>();
		private readonly List<KeyboardButton> modifierButtons = new List<KeyboardButton>();

		private bool IsCommandCurrent(CommandInfo info) => info.Shortcut.Modifiers.HasFlag(Modifiers);

		private bool IsCommandSelected(CommandInfo info) =>
			info.Shortcut.Modifiers.HasFlag(Modifiers) && (Main != Key.Unknown) && info.Shortcut.Main == Main;

		public IEnumerable<CommandInfo> SelectedCommands { get; private set; }

		public Modifiers Modifiers { get; private set; }
		public Key Main { get; private set; }

		public CommandCategoryInfo Category { get; set; }

		public HotkeyProfile Profile { get; set; }

		public Action SelectedShortcutChanged { get; set; }

		public HotkeyEditor()
		{
			Profile = HotkeyRegistry.CurrentProfile;
			BuildKeyboard();
			Updating += ScreenKeyboard_Updating;
			Updated += ScreenKeyboard_Updated;
			this.AddChangeWatcher(() => Category, category => {
				UpdateButtonCommands();
				UpdateShortcuts();
			});
			this.AddChangeWatcher(() => Profile, profile => {
				UpdateButtonCommands();
				UpdateShortcuts();
			});
			this.AddChangeWatcher(() => Modifiers, modifiers => UpdateShortcuts());
			this.AddChangeWatcher(() => Main, main => UpdateShortcuts());
		}

		public void UpdateButtonCommands()
		{
			foreach (var button in this.buttons) {
				button.Value.Commands.Clear();
			}
			foreach (var command in Profile.Commands) {
				var key = command.Shortcut.Main;
				if (key == Key.Unknown)
					continue;
				if (!buttons.TryGetValue(key, out var b))
					continue;
				b.Commands.Add(command);
			}
		}

		public void UpdateShortcuts()
		{
			if (Category == null) {
				return;
			}
			bool isGenericCategory = (Category.Id == typeof(GenericCommands).Name);
			SelectedCommands = isGenericCategory ? Profile.Commands.Where(IsCommandSelected) :
				Profile.Commands.Where(i => IsCommandSelected(i) && i.CategoryInfo == Category);
			foreach (var (key, button)  in buttons) {
				button.CommandName = null;
				button.CommandState = KeyboardCommandState.None;
				var currentCommands = isGenericCategory ?
					button.Commands.Where(IsCommandCurrent) :
					button.Commands.Where(i => IsCommandCurrent(i) && i.CategoryInfo == Category);
				bool hasGenericCommand = false;
				bool hasPanelCommand = false;
				foreach (var command in currentCommands) {
					bool isPanel = command.CategoryInfo.Id != typeof(GenericCommands).Name;
					hasPanelCommand = hasPanelCommand || isPanel;
					if (command.CategoryInfo == Category) {
						bool isGeneric = !isPanel;
						hasGenericCommand = hasGenericCommand || isGeneric;
					}
				}
				if (hasPanelCommand) {
					button.CommandState = hasGenericCommand ? KeyboardCommandState.Both :
						isGenericCategory ? KeyboardCommandState.HalfPanel : KeyboardCommandState.Panel;
				} else if (hasGenericCommand) {
					button.CommandState = KeyboardCommandState.Generic;
				}

				button.CommandName = currentCommands.FirstOrDefault(i => i.CategoryInfo == Category)?.Title;
			}
			SelectedShortcutChanged?.Invoke();
		}

		private void PressModifier(Modifiers modifier, Key key)
		{
			var input = Input;
			var btns = modifierButtons.Where(i => i.Key == key);
			if (input.WasKeyPressed(key)) {
				Modifiers |= modifier;
				if (btns.Any()) {
					foreach (var btn in btns) {
						btn.State = KeyboardButtonState.Press;
					}
				}
			} else if (input.WasKeyReleased(key)) {
				Modifiers &= ~modifier;
				if (btns.Any()) {
					foreach (var btn in btns) {
						btn.State = KeyboardButtonState.None;
					}
				}
			}
		}

		private void PressButton(Key key)
		{
			if (key == Main) {
				return;
			}
			if (key != Key.Unknown) {
				buttons[key].State = KeyboardButtonState.Press;
			}
			if (Main != Key.Unknown) {
				buttons[Main].State = KeyboardButtonState.None;
			}
			Main = key;
		}

		private void SwitchModifier(Modifiers modifier, Key key)
		{
			SetFocus();
			bool isHolded = Modifiers.HasFlag(modifier);
			Modifiers = isHolded ? Modifiers & ~modifier : Modifiers | modifier;
			if (buttons.TryGetValue(key, out var btn)) {
				btn.State = isHolded ? KeyboardButtonState.None : KeyboardButtonState.Hold;
			}
		}

		private void SwitchButton(Key key)
		{
			SetFocus();
			if (Main != Key.Unknown) {
				buttons[Main].State = KeyboardButtonState.None;
				if (key == Main) {
					Main = Key.Unknown;
					return;
				}
			}
			if (buttons.TryGetValue(key, out var btn) && Shortcut.ValidateMainKey(key)) {
				Main = key;
				btn.State = KeyboardButtonState.Hold;
			}
		}

		private void ScreenKeyboard_Updating(float delta)
		{
			PressModifier(Modifiers.Alt, Key.Alt);
			PressModifier(Modifiers.Shift, Key.Shift);
			PressModifier(Modifiers.Control, Key.Control);
			PressModifier(Modifiers.Win, Key.Win);

			var input = CommonWindow.Current.Input;
			var keys = Key.Enumerate().Where(k =>
				input.IsKeyPressed(k) &&
				!k.IsModifier() && !k.IsMouseKey() && Shortcut.ValidateMainKey(k));
			if (!keys.Any()) {
				if (Main != Key.Unknown) {
					buttons[Main].State = KeyboardButtonState.None;
					Main = Key.Unknown;
				}
				return;
			}
			PressButton(keys.First());
		}

		private void ScreenKeyboard_Updated(float delta)
		{
			var input = Input;
			if (IsFocused()) {
				input.ConsumeKeys(Key.Enumerate().Where(
					k => input.WasKeyRepeated(k) || input.WasKeyPressed(k) || input.WasKeyReleased(k)));
				Command.ConsumeRange(Command.Editing);
			}
		}


		private void BuildKeyboard()
		{
			Layout = new TableLayout { ColumnCount = 3, RowCount = 1, Spacing = 20 };
			var fRow = new Widget { Layout = new TableLayout { ColumnCount = 12, RowCount = 1, Spacing = 4 } };
			var numbersRow = new Widget { Layout = new TableLayout { ColumnCount = 14, RowCount = 1, Spacing = 4 } };
			var firstRow = new Widget { Layout = new TableLayout { ColumnCount = 14, RowCount = 1, Spacing = 4 } };
			var secondRow = new Widget { Layout = new TableLayout { ColumnCount = 13, RowCount = 1, Spacing = 4 } };
			var thirdRow = new Widget { Layout = new TableLayout { ColumnCount = 12, RowCount = 1, Spacing = 4 } };
			var controlsRow = new Widget { Layout = new TableLayout { ColumnCount = 7, RowCount = 1, Spacing = 4 } };

			var leftPart = new Widget {
				Layout = new TableLayout { ColumnCount = 1, RowCount = 6, Spacing = 4 },
				Nodes = { fRow, numbersRow, firstRow, secondRow, thirdRow, controlsRow },
				LayoutCell = new LayoutCell { StretchX = 15 }
			};

			var middlePart = new Widget {
				Layout = new TableLayout { ColumnCount = 3, RowCount = 6, Spacing = 4 },
				LayoutCell = new LayoutCell { StretchX = 3 }
			};

			var rightPart = new Widget {
				Layout = new TableLayout { ColumnCount = 4, RowCount = 7, Spacing = 4 },
				LayoutCell = new LayoutCell { StretchX = 4 }
			};

			for (int i = Key.F1.Code; i <= Key.F12.Code; ++i) {
				CreateButton(fRow, new Key(i));
			}

			CreateButton(numbersRow, Key.Tilde, "~");
			CreateButton(numbersRow, Key.Number1, "1");
			CreateButton(numbersRow, Key.Number2, "2");
			CreateButton(numbersRow, Key.Number3, "3");
			CreateButton(numbersRow, Key.Number4, "4");
			CreateButton(numbersRow, Key.Number5, "5");
			CreateButton(numbersRow, Key.Number6, "6");
			CreateButton(numbersRow, Key.Number7, "7");
			CreateButton(numbersRow, Key.Number8, "8");
			CreateButton(numbersRow, Key.Number9, "9");
			CreateButton(numbersRow, Key.Number0, "0");
			CreateButton(numbersRow, Key.Minus, "-");
			CreateButton(numbersRow, Key.EqualsSign, "=");
			CreateButton(numbersRow, Key.BackSpace).LayoutCell = new LayoutCell(Alignment.RightCenter, 2);

			CreateButton(firstRow, Key.Tab).LayoutCell = new LayoutCell(Alignment.LeftCenter, 1.5f);
			CreateButton(firstRow, Key.Q);
			CreateButton(firstRow, Key.W);
			CreateButton(firstRow, Key.E);
			CreateButton(firstRow, Key.R);
			CreateButton(firstRow, Key.T);
			CreateButton(firstRow, Key.Y);
			CreateButton(firstRow, Key.U);
			CreateButton(firstRow, Key.I);
			CreateButton(firstRow, Key.O);
			CreateButton(firstRow, Key.P);
			CreateButton(firstRow, Key.LBracket, "[");
			CreateButton(firstRow, Key.RBracket, "]");
			CreateButton(firstRow, Key.BackSlash, "\\").LayoutCell = new LayoutCell(Alignment.RightCenter, 1.5f);

			CreateButton(secondRow, Key.CapsLock, "Caps Lock").LayoutCell = new LayoutCell(Alignment.LeftCenter, 2);
			CreateButton(secondRow, Key.A);
			CreateButton(secondRow, Key.S);
			CreateButton(secondRow, Key.D);
			CreateButton(secondRow, Key.F);
			CreateButton(secondRow, Key.G);
			CreateButton(secondRow, Key.H);
			CreateButton(secondRow, Key.J);
			CreateButton(secondRow, Key.K);
			CreateButton(secondRow, Key.L);
			CreateButton(secondRow, Key.Semicolon, ";");
			CreateButton(secondRow, Key.Quote, "\'");
			CreateButton(secondRow, Key.Enter).LayoutCell = new LayoutCell(Alignment.RightCenter, 2);

			var lShift = CreateButton(thirdRow, Key.Shift);
			lShift.LayoutCell = new LayoutCell(Alignment.LeftCenter, 2.5f);
			lShift.Gestures.Add(new ClickGesture());
			lShift.Clicked = () => SwitchModifier(Modifiers.Shift, Key.Shift);

			CreateButton(thirdRow, Key.Z);
			CreateButton(thirdRow, Key.X);
			CreateButton(thirdRow, Key.C);
			CreateButton(thirdRow, Key.V);
			CreateButton(thirdRow, Key.B);
			CreateButton(thirdRow, Key.N);
			CreateButton(thirdRow, Key.M);
			CreateButton(thirdRow, Key.Comma, ",");
			CreateButton(thirdRow, Key.Period, ".");
			CreateButton(thirdRow, Key.Slash, "/");

			var rShift = CreateButton(thirdRow, Key.Shift);
			rShift.LayoutCell = new LayoutCell(Alignment.RightCenter, 2.5f);
			rShift.Clicked = () => SwitchModifier(Modifiers.Shift, Key.Shift);

			var lCtrl = CreateButton(controlsRow, Key.Control, "Ctrl");
			lCtrl.LayoutCell = new LayoutCell(Alignment.LeftCenter, 1.5f);
			lCtrl.Clicked = () => SwitchModifier(Modifiers.Control, Key.Control);

			var lWin = CreateButton(controlsRow, Key.Win);
			lWin.LayoutCell = new LayoutCell(Alignment.Center, 1.5f);
			lWin.Clicked = () => SwitchModifier(Modifiers.Win, Key.Win);

			var lAlt = CreateButton(controlsRow, Key.Alt);
			lAlt.LayoutCell = new LayoutCell(Alignment.Center, 1.5f);
			lAlt.Clicked = () => SwitchModifier(Modifiers.Alt, Key.Alt);

			CreateButton(controlsRow, Key.Space).LayoutCell = new LayoutCell(Alignment.Center, 6);

			var rAlt = CreateButton(controlsRow, Key.Alt);
			rAlt.LayoutCell = new LayoutCell(Alignment.Center, 1.5f);
			rAlt.Clicked = () => SwitchModifier(Modifiers.Alt, Key.Alt);

			var rWin = CreateButton(controlsRow, Key.Win);
			rWin.LayoutCell = new LayoutCell(Alignment.Center, 1.5f);
			rWin.Clicked = () => SwitchModifier(Modifiers.Win, Key.Win);

			var rCtrl = CreateButton(controlsRow, Key.Control, "Ctrl");
			rCtrl.LayoutCell = new LayoutCell(Alignment.RightCenter, 1.5f);
			rCtrl.Clicked = () => SwitchModifier(Modifiers.Control, Key.Control);

			CreateButton(middlePart, Key.PrintScreen, "Print Screen");
			CreateButton(middlePart, Key.ScrollLock, "Scroll Lock");
			CreateButton(middlePart, Key.Pause);
			CreateButton(middlePart, Key.Insert);
			CreateButton(middlePart, Key.Home);
			CreateButton(middlePart, Key.PageUp, "Page Up");
			CreateButton(middlePart, Key.Delete);
			CreateButton(middlePart, Key.End);
			CreateButton(middlePart, Key.PageDown, "Page Down");
			CreateSpace(middlePart, 1, 3);
			CreateSpace(middlePart, 1, 1);
			CreateButton(middlePart, Key.Up);
			CreateSpace(middlePart, 1, 1);
			CreateButton(middlePart, Key.Left);
			CreateButton(middlePart, Key.Down);
			CreateButton(middlePart, Key.Right);

			var mouseSpace = CreateSpace(rightPart, 3, 4);
			mouseSpace.Layout = new TableLayout { ColumnCount = 2, RowCount = 1, Spacing = 4 };
			CreateButton(mouseSpace, Key.MouseBack, "Mouse Back");
			CreateButton(mouseSpace, Key.MouseForward, "Mouse Forward");
			CreateButton(rightPart, Key.Keypad7, "7");
			CreateButton(rightPart, Key.Keypad8, "8");
			CreateButton(rightPart, Key.Keypad9, "9");
			CreateButton(rightPart, Key.KeypadMinus, "-");
			CreateButton(rightPart, Key.Keypad4, "4");
			CreateButton(rightPart, Key.Keypad5, "5");
			CreateButton(rightPart, Key.Keypad6, "6");
			CreateButton(rightPart, Key.KeypadPlus, "+");
			CreateButton(rightPart, Key.Keypad1, "1");
			CreateButton(rightPart, Key.Keypad2, "2");
			CreateButton(rightPart, Key.Keypad3, "3");
			CreateButton(rightPart, Key.KeypadDivide, "/");
			var lastRow = CreateSpace(rightPart, 1, 4);
			lastRow.Layout = new TableLayout { ColumnCount = 3, RowCount = 1, Spacing = 4 };
			CreateButton(lastRow, Key.Keypad0, "0").LayoutCell = new LayoutCell { StretchX = 2 };
			CreateButton(lastRow, Key.KeypadDecimal, ".");
			CreateButton(lastRow, Key.KeypadMultiply, "*");

			AddNode(leftPart);
			AddNode(middlePart);
			AddNode(rightPart);

			foreach (var (key, button) in buttons) {
				if (!button.Key.IsModifier()) {
					button.Clicked = () => SwitchButton(button.Key);
				}
			}
			UpdateButtonCommands();
		}

		private Widget CreateButton(Widget parent, Key key, string text = null)
		{
			var button = new KeyboardButton(key) {
				Text = string.IsNullOrEmpty(text) ? key.ToString() : text,
				MinSize = Vector2.Zero,
				MaxSize = Vector2.PositiveInfinity
			};
			buttons[key] = button;
			if (key.IsModifier()) {
				modifierButtons.Add(button);
			}
			parent.AddNode(button);
			return button;
		}

		private Widget CreateSpace(Widget parent, int rowSpan, int columnSpan)
		{
			var space = new Widget {
				MinSize = Vector2.Zero,
				MaxSize = Vector2.PositiveInfinity,
				LayoutCell = new LayoutCell { RowSpan = rowSpan, ColumnSpan = columnSpan }
			};
			parent.AddNode(space);
			return space;
		}
	}

	enum KeyboardButtonState
	{
		None = 0,
		Press,
		Hold
	}

	enum KeyboardCommandState
	{
		None = 0,
		Generic = 1,
		Panel = 2,
		Both = 3,
		HalfPanel = 4
	}

	class KeyboardButton : ThemedButton
	{
		private readonly SimpleText commandName;

		public KeyboardButton(Key key) : base()
		{
			Presenter = new KeyboardButtonPresenter();
			TabTravesable = null;
			var caption = Nodes.First(i => i.Id == "TextPresenter") as SimpleText;
			caption.VAlignment = VAlignment.Bottom;
			caption.HAlignment = HAlignment.Left;
			caption.Padding = new Thickness(5, 2);
			caption.TextColor = ColorTheme.Current.Keyboard.GrayText;
			commandName = new SimpleText {
				TextColor = ColorTheme.Current.Keyboard.BlackText,
				FontHeight = Theme.Metrics.TextHeight,
				HAlignment = HAlignment.Left,
				VAlignment = VAlignment.Top,
				OverflowMode = TextOverflowMode.Ellipsis,
				Padding = new Thickness(5, 2)
			};
			Nodes.Add(commandName);
			commandName.ExpandToContainerWithAnchors();
			commandName.Height -= caption.FontHeight;

			Key = key;
			((KeyboardButtonPresenter) Presenter).IsModifier = Key.IsModifier();
			State = KeyboardButtonState.None;
		}

		public string CommandName
		{
			get => commandName.Text;
			set => commandName.Text = value;
		}

		public KeyboardCommandState CommandState
		{
			get => (Presenter as KeyboardButtonPresenter).CommandState;
			set => (Presenter as KeyboardButtonPresenter).CommandState = value;
		}

		public List<CommandInfo> Commands { get; private set; } = new List<CommandInfo>();

		private KeyboardButtonState state;
		public KeyboardButtonState State
		{
			get => state;
			set {
				state = value;
				(Presenter as IButtonPresenter).SetState(state.ToString());
			}
		}

		public readonly Key Key;
	}

	class KeyboardButtonPresenter : SyncCustomPresenter, ThemedButton.IButtonPresenter
	{
		private Color4 borderColor;
		public bool IsModifier { get; set; } = false;
		public KeyboardCommandState CommandState { get; set; }

		private static readonly Vertex[] triangleVertices = new Vertex[3];

		public KeyboardButtonPresenter()
		{
			for (int i = 0; i < triangleVertices.Length; ++i) {
				triangleVertices[i].Color = ColorTheme.Current.Keyboard.PanelKeyBackground;
			}
		}

		public void SetState(string state)
		{
			CommonWindow.Current.Invalidate();
			switch (state) {
				case "None":
					borderColor = ColorTheme.Current.Keyboard.Border;
					break;
				case "Press":
				case "Hold":
					borderColor = ColorTheme.Current.Keyboard.SelectedBorder;
					break;
			}
		}

		public override void Render(Node node)
		{
			var colors = ColorTheme.Current.Keyboard;
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			Color4 backColor = IsModifier ? colors.ModifierBackground : colors.ButtonBackground;

			triangleVertices[0].Pos = new Vector2(widget.Width, 0);
			triangleVertices[1].Pos = widget.Size;
			triangleVertices[2].Pos = new Vector2(0, widget.Height);

			switch (CommandState) {
				case KeyboardCommandState.None:
					Renderer.DrawRect(Vector2.Zero, widget.Size, backColor);
					break;
				case KeyboardCommandState.Generic:
					Renderer.DrawRect(Vector2.Zero, widget.Size, colors.GenericKeyBackground);
					break;
				case KeyboardCommandState.Panel:
					Renderer.DrawRect(Vector2.Zero, widget.Size, colors.PanelKeyBackground);
					break;
				case KeyboardCommandState.Both:
					Renderer.DrawRect(Vector2.Zero, widget.Size, colors.GenericKeyBackground);
					Renderer.DrawTriangleFan(triangleVertices, 3);
					break;
				case KeyboardCommandState.HalfPanel:
					Renderer.DrawRect(Vector2.Zero, widget.Size, backColor);
					Renderer.DrawTriangleFan(triangleVertices, 3);
					break;
				default:
					break;
			}

			float thickness = borderColor == ColorTheme.Current.Keyboard.SelectedBorder ? 2 : 1;
			Renderer.DrawRectOutline(Vector2.Zero, widget.Size, borderColor, thickness);
		}
	}
}
