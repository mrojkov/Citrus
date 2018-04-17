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
					textView.ScrollToEnd();
				});
			}

			public override Encoding Encoding { get; }
		}

		public static Console Instance { get; private set; }

		private DockPanel dockPanel;
		public readonly Widget RootWidget;
		private ThemedTextView textView;
		private TextWriter textWriter;

		public Console(DockPanel dockPanel)
		{
			if (Instance != null) {
				throw new InvalidOperationException();
			}
			Instance = this;
			this.dockPanel = dockPanel;
			RootWidget = new Widget {
				Layout = new VBoxLayout {
					Spacing = 6
				}
			};
			dockPanel.ContentWidget.AddNode(RootWidget);
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
				}
				if (textView.Input.WasKeyPressed(Key.Mouse1)) {
					menu.Popup();
				}
				if (textView.IsFocused() && Command.Copy.WasIssued()) {
					Command.Copy.Consume();
					Clipboard.Text = textView.Text;
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
			DockManager.Instance.ShowPanel(dockPanel);
		}
	}
}
