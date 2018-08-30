using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Docking;

namespace Tangerine.UI
{
	public class Console
	{
		private class TextViewWriter : TextWriter
		{
			private readonly ThemedTextView textView;

			public TextViewWriter(ThemedTextView textView)
			{
				this.textView = textView;
			}

			public override void WriteLine(string value)
			{
				Write(value + '\n');
			}

			public override void Write(string value)
			{
				Application.InvokeOnMainThread(() => {
					textView.Append(value);
				});
				Application.InvokeOnNextUpdate(textView.ScrollToEnd);
			}

			public override Encoding Encoding { get; }
		}

		public static Console Instance { get; private set; }

		private Panel panel;
		public readonly Widget RootWidget;
		private ThemedTextView textView;
		private TextWriter textWriter;

		public Console(Panel panel)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = this;
			this.panel = panel;
			RootWidget = new Widget {
				Layout = new VBoxLayout {
					Spacing = 6
				}
			};
			panel.ContentWidget.AddNode(RootWidget);
			RootWidget.AddNode(CreateTextView());
		}

		private Widget CreateTextView()
		{
			textView = new ThemedTextView { SquashDuplicateLines = true };
			textWriter = new TextViewWriter(textView);
			System.Console.SetOut(textWriter);
			System.Console.SetError(textWriter);
			var menu = new Menu();
			menu.Add(Command.Copy);
			textView.Updated += (dt) => {
				if (textView.Input.WasKeyPressed(Key.Mouse0)) {
					textView.SetFocus();
					Window.Current.Activate();
				}
				if (textView.IsFocused()) {
					Command.Copy.Enabled = true;
					if (Command.Copy.WasIssued()) {
						Command.Copy.Consume();
						Clipboard.Text = textView.Text;
					}
				}
				if (textView.Input.WasKeyPressed(Key.Mouse1)) {
					textView.SetFocus();
					Window.Current.Activate();
					menu.Popup();
				}
				var i = textView.Content.Nodes.Count;
				// numbers choosen by guess
				if (i >= 500) {
					textView.Content.Nodes.RemoveRange(0, 250);
				}
			};

			return textView;
		}

		public void Show()
		{
			DockManager.Instance.ShowPanel(panel.Id);
		}
	}
}
