using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.UI;
using static Lime.ThemedButton;

namespace Tangerine.Dialogs
{
	public class HotkeyEditor : Widget
	{
		private List<KeyboardButton> buttons = new List<KeyboardButton>();
		private IEnumerable<KeyboardButton> FindButtons(Key key) => buttons.Where(i => i.Key == key);

		private bool IsCommandCurrent(CommandInfo info) =>
			info.Shortcut.Modifiers.HasFlag(modifiers);
		
		private bool IsCommandSelected(CommandInfo info) =>
			info.Shortcut.Modifiers.HasFlag(modifiers) &&
			((main == Key.Unknown) ? false : info.Shortcut.Main == main);

		public IEnumerable<CommandInfo> SelectedCommands { get; private set; }

		private IEnumerable<ICommand> consumableCommands = HotkeyRegistry.Commands.Select(i => i.Command).Union(Command.Editing);

		private Modifiers modifiers;
		private Key main;

		private CommandCategory category;
		public CommandCategory Category {
			get { return category; }
			set {
				category = value;
				UpdateShortcuts();
			}
		}

		public Action SelectedShortcutChanged { get; set; }

		public HotkeyEditor()
		{
			BuildKeyboard();
			Updating += ScreenKeyboard_Updating;
			Updated += ScreenKeyboard_Updated;
			Core.WidgetExtensions.AddChangeWatcher(this, () => modifiers, modifiers => { UpdateShortcuts(); });
			Core.WidgetExtensions.AddChangeWatcher(this, () => main, main => { UpdateShortcuts(); });
		}

		public void UpdateButtonCommands()
		{
			foreach (var button in this.buttons) {
				button.Commands.Clear();
			}
			foreach (var command in HotkeyRegistry.Commands) {
				var key = command.Shortcut.Main;
				if (key == Key.Unknown)
					continue;
				var buttons = FindButtons(key);
				if (!buttons.Any())
					continue;
				foreach (var button in buttons) {
					button.Commands.Add(command);
				}
			}
		}

		public void UpdateShortcuts()
		{
			if (Category == null)
				return;
			bool isGenericCategory = (Category.SystemName == typeof(GenericCommands).Name);
			SelectedCommands = isGenericCategory ? HotkeyRegistry.Commands.Where(i => IsCommandSelected(i)) :
				HotkeyRegistry.Commands.Where(i => IsCommandSelected(i) && i.Category == Category);
			foreach (var button in buttons) {
				button.CommandName = null;
				button.CommandState = KeyboardCommandState.None;
				IEnumerable<CommandInfo> currentCommands = isGenericCategory ?
					button.Commands.Where(i => IsCommandCurrent(i)) :
					button.Commands.Where(i => IsCommandCurrent(i) && i.Category == Category);
				bool hasGenericCommand = false;
				bool hasPanelCommand = false;
				foreach (var command in currentCommands) {
					bool isPanel = command.Category.SystemName != typeof(GenericCommands).Name;
					hasPanelCommand = hasPanelCommand || isPanel;
					if (command.Category == Category) {
						bool isGeneric = !isPanel;
						hasGenericCommand = hasGenericCommand || isGeneric;
					}
				}
				if (hasPanelCommand) {
					button.CommandState = hasGenericCommand ? KeyboardCommandState.Both :
						isGenericCategory ? KeyboardCommandState.HalfPanel : KeyboardCommandState.Panel;
				}
				else if (hasGenericCommand) {
					button.CommandState = KeyboardCommandState.Generic;
				}

				button.CommandName = currentCommands.Where(i => i.Category == Category).FirstOrDefault()?.Name;
			}
			SelectedShortcutChanged?.Invoke();
		}

		private void PressModifier(Modifiers modifier, Key key)
		{
			var input = Input;
			if (input.WasKeyPressed(key)) {
				modifiers |= modifier;
				var buttons = FindButtons(key);
				foreach (var button in buttons) {
					button.State = KeyboardButtonState.Press;
				}
			}
			else if (input.WasKeyReleased(key)) {
				modifiers &= ~modifier;
				var buttons = FindButtons(key);
				foreach (var button in buttons) {
					button.State = KeyboardButtonState.None;
				}
			}
		}

		private void PressButton(Key key)
		{
			var input = Input;
			if (input.WasKeyPressed(key)) {
				if (main != Key.Unknown) {
					FindButtons(main).First().State = KeyboardButtonState.None;
				}
				var buttons = FindButtons(key);
				if (buttons.Any() && Shortcut.ValidateMainKey(key)) {
					var newButton = buttons.First();
					main = key;
					newButton.State = KeyboardButtonState.Press;
				}

			}
			else if (input.WasKeyReleased(key) && key == main) {
				var buttons = FindButtons(key);
				if (buttons.Any()) {
					var oldButton = buttons.First();
					main = Key.Unknown;
					oldButton.State = KeyboardButtonState.None;
				}
			}
		}

		private void SwitchModifier(Modifiers modifier, Key key)
		{
			SetFocus();
			var buttons = FindButtons(key);
			bool isHolded = modifiers.HasFlag(modifier);
			modifiers = isHolded ? modifiers & ~modifier : modifiers | modifier;
			foreach (var button in buttons) {
				button.State = isHolded ? KeyboardButtonState.None : KeyboardButtonState.Hold;
			}
		}

		private void SwitchButton(Key key)
		{
			SetFocus();
			if (main != Key.Unknown) {
				FindButtons(main).First().State = KeyboardButtonState.None;
				if (key == main) {
					main = Key.Unknown;
					return;
				}
			}
			var buttons = FindButtons(key);
			if (buttons.Any() && Shortcut.ValidateMainKey(key)) {
				var newButton = buttons.First();
					main = key;
				newButton.State = KeyboardButtonState.Hold;
			}
		}

		private void ScreenKeyboard_Updating(float delta)
		{
			if (!IsFocused())
				return;
			PressModifier(Modifiers.Alt, Key.Alt);
			PressModifier(Modifiers.Shift, Key.Shift);
			PressModifier(Modifiers.Control, Key.Control);
			PressModifier(Modifiers.Win, Key.Win);

			var input = Input;
			var keys = Key.Enumerate().Where(k => input.WasKeyPressed(k) || input.WasKeyReleased(k));
			if (!keys.Any())
				return;
			foreach (var key in keys) {
				if (!key.IsModifier() && !key.IsMouseKey()) {
					PressButton(key);
					break;
				}
			}
		}

		private void ScreenKeyboard_Updated(float delta)
		{
			if (!IsFocused())
				return;
			var input = Input;
			input.ConsumeKeys(Key.Enumerate().Where(
				k => input.WasKeyRepeated(k) || input.WasKeyPressed(k) || input.WasKeyReleased(k)));
			Command.ConsumeRange(consumableCommands);
		}


		private void BuildKeyboard()
		{
			Layout = new TableLayout { ColCount = 3, RowCount = 1, Spacing = 20 };
			var fRow = new Widget { Layout = new TableLayout { ColCount = 12, RowCount = 1, Spacing = 4 } };
			var numbersRow = new Widget { Layout = new TableLayout { ColCount = 14, RowCount = 1, Spacing = 4 } };
			var firstRow = new Widget { Layout = new TableLayout { ColCount = 14, RowCount = 1, Spacing = 4 } };
			var secondRow = new Widget { Layout = new TableLayout { ColCount = 13, RowCount = 1, Spacing = 4 } };
			var thirdRow = new Widget { Layout = new TableLayout { ColCount = 12, RowCount = 1, Spacing = 4 } };
			var controlsRow = new Widget { Layout = new TableLayout { ColCount = 7, RowCount = 1, Spacing = 4 } };

			var leftPart = new Widget {
				Layout = new TableLayout { ColCount = 1, RowCount = 6, Spacing = 4 },
				Nodes = { fRow, numbersRow, firstRow, secondRow, thirdRow, controlsRow },
				LayoutCell = new LayoutCell { StretchX = 15 }
			};

			var middlePart = new Widget {
				Layout = new TableLayout { ColCount = 3, RowCount = 6, Spacing = 4 },
				LayoutCell = new LayoutCell { StretchX = 3 }
			};

			var rightPart = new Widget {
				Layout = new TableLayout { ColCount = 4, RowCount = 6, Spacing = 4 },
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

			CreateSpace(rightPart, 1, 4);
			CreateSpace(rightPart, 1, 4);
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
			lastRow.Layout = new TableLayout { ColCount = 3, RowCount = 1, Spacing = 4 };
			CreateButton(lastRow, Key.Keypad0, "0").LayoutCell = new LayoutCell { StretchX = 2 };
			CreateButton(lastRow, Key.KeypadDecimal, ".");
			CreateButton(lastRow, Key.KeypadMultiply, "*");

			AddNode(leftPart);
			AddNode(middlePart);
			AddNode(rightPart);

			foreach (var button in buttons) {
				if (!button.Key.IsModifier())
					button.Clicked = () => SwitchButton(button.Key);
			}
			UpdateButtonCommands();
		}

		private Widget CreateButton(Widget parent, Key key, string text = null)
		{
			var button = new KeyboardButton(key) {
				Text = String.IsNullOrEmpty(text) ? key.ToString() : text,
				MinSize = Vector2.Zero,
				MaxSize = Vector2.PositiveInfinity
			};
			buttons.Add(button);
			parent.AddNode(button);
			return button;
		}

		private Widget CreateSpace(Widget parent, int rowSpan, int colSpan)
		{
			var space = new Widget {
				MinSize = Vector2.Zero,
				MaxSize = Vector2.PositiveInfinity,
				LayoutCell = new LayoutCell { RowSpan = rowSpan, ColSpan = colSpan }
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
		private SimpleText commandName;

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
			(Presenter as KeyboardButtonPresenter).IsModifier = Key.IsModifier();
			State = KeyboardButtonState.None;
		}

		public string CommandName
		{
			get { return commandName.Text; }
			set { commandName.Text = value; }
		}

		public KeyboardCommandState CommandState
		{
			get { return (Presenter as KeyboardButtonPresenter).CommandState; }
			set { (Presenter as KeyboardButtonPresenter).CommandState = value; }
		}

		public List<CommandInfo> Commands { get; private set; } = new List<CommandInfo>();
		
		private KeyboardButtonState state;
		public KeyboardButtonState State
		{
			get { return state; }
			set {
				state = value;
				(Presenter as IButtonPresenter).SetState(state.ToString());
			}
		}

		public readonly Key Key;
	}

	class KeyboardButtonPresenter : ButtonPresenter
	{
		private Color4 borderColor;
		public bool IsModifier { get; set; } = false;
		public KeyboardCommandState CommandState { get; set; }

		static private Vertex[] triangleVertices = new Vertex[3];

		public KeyboardButtonPresenter()
		{
			for (int i = 0; i < triangleVertices.Length; ++i) {
				triangleVertices[i].Color = ColorTheme.Current.Keyboard.PanelKeyBackground;
			}
		}

		public override void SetState(string state)
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
