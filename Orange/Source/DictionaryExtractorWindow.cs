using System;
using System.IO;
using Gtk;

namespace Orange
{
	public partial class DictionaryExtractorWindow
	{
		private Window NativeWindow;
		private Button GoButton;
		private DictionaryOldFormatExtractor.StringInfo[] stringInfos;
		private ListStore store;
		private TreeView treeView;

		public System.Action OnGo;

		public DictionaryExtractorWindow(DictionaryOldFormatExtractor.StringInfo[] stringInfos)
		{
			this.stringInfos = stringInfos;

			NativeWindow = new Window(WindowType.Toplevel) {
				Title = "Extract Dictionary",
				WindowPosition = WindowPosition.Center,
				DefaultSize = new Gdk.Size(1000, 600)
			};

			var mainVBox = new VBox {
				Spacing = 6,
				BorderWidth = 6,
				Name = "MainVBox"
			};

			var header = CreateHeaderSection();
			mainVBox.PackStart(header, expand: false, fill: true, padding: 0);

			var list = CreateListPane();
			mainVBox.PackStart(list, expand: true, fill: true, padding: 0);

			var footer = CreateFooterSection();
			mainVBox.PackStart(footer, expand: false, fill: true, padding: 0);

			NativeWindow.Add(mainVBox);

			GoButton.Clicked += GoButton_Clicked;

			NativeWindow.ShowAll();
			NativeWindow.Show();
			GoButton.GrabFocus();
		}

		void GoButton_Clicked(object sender, EventArgs e)
		{
			NativeWindow.Destroy();
			if (OnGo != null) {
				OnGo();
			}
		}

		void SetEnableForSelection(bool enable)
		{
			foreach (var path in treeView.Selection.GetSelectedRows()) {
				TreeIter iter;
				if (store.GetIter(out iter, path)) {
					int idx = (int)store.GetValue(iter, 3);
					stringInfos[idx].Allow = enable;
					store.SetValue(iter, 0, enable);
				}
			}
		}

		Widget CreateListPane()
		{
			store = new ListStore(typeof(bool), typeof(string), typeof(string), typeof(int));
			for (int i = 0; i < stringInfos.Length; i++) {
				var info = stringInfos[i];
				string sources = "";
				foreach (string s in info.Sources) {
					if (string.IsNullOrEmpty(sources)) {
						sources = s;
					} else {
						sources = sources + "\n" + s;
					}
				}
				store.AppendValues(info.Allow, info.Text, sources, i);
			}

			treeView = new TreeView(store);
			treeView.EnableGridLines = TreeViewGridLines.Both;
			treeView.Selection.Mode = SelectionMode.Multiple;

			var checkboxRenderer = new CellRendererToggle();
			checkboxRenderer.Toggled += (object o, ToggledArgs args) => {
				TreeIter iter;
				if (store.GetIterFromString(out iter, args.Path)) {
					int idx = (int)store.GetValue(iter, 3);
					stringInfos[idx].Allow = !stringInfos[idx].Allow;
					store.SetValue(iter, 0, stringInfos[idx].Allow);
				}
			};
			checkboxRenderer.Activatable = true;
			treeView.AppendColumn(new TreeViewColumn("Extract", checkboxRenderer, "active", 0) { MinWidth = 50 });
			treeView.AppendColumn(new TreeViewColumn("Text", new CellRendererText() { Ellipsize = Pango.EllipsizeMode.End }, "text", 1) { Resizable = true, MinWidth = 200 });
			treeView.AppendColumn(new TreeViewColumn("Source", new CellRendererText() { Ellipsize = Pango.EllipsizeMode.End }, "text", 2) { Resizable = true });

			var scrolledWindow = new ScrolledWindow() { ShadowType = ShadowType.In, HscrollbarPolicy = PolicyType.Never };
			scrolledWindow.Add(treeView);
			return scrolledWindow;
		}

		Widget CreateHeaderSection()
		{
			var hbox = new HBox();

			var enable = new Button() { WidthRequest = 120, Label = "Enable selected" };
			hbox.PackStart(enable, expand: false, fill: true, padding: 0);
			enable.Clicked += (sender, e) => {
				SetEnableForSelection(true);
			};

			var disable = new Button() { WidthRequest = 120, Label = "Disable selected" };
			hbox.PackStart(disable, expand: false, fill: true, padding: 0);
			disable.Clicked += (sender, e) => {
				SetEnableForSelection(false);
			};

			return hbox;
		}

		Widget CreateFooterSection()
		{
			var hbox = new HBox();
			
			// GoButton section
			GoButton = new Button() { WidthRequest = 80, Label = "Go" };
			hbox.PackEnd(GoButton, expand: false, fill: true, padding: 0);
			return hbox;
		}
	}
}
