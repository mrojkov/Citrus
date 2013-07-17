namespace Orange
{
	public partial class MainWindow
	{
		public Gtk.Window NativeWindow;
		public Gtk.FileChooserButton CitrusProjectChooser;
		public Gtk.ComboBox PlatformPicker;
		public Gtk.TextView OutputPane;
		public Gtk.ComboBox ActionPicker;
		public Gtk.CheckButton UpdateBeforeBuildCheckbox;
		public Gtk.Button GoButton;

		private void Create()
		{
			NativeWindow = new Gtk.Window(Gtk.WindowType.Toplevel);

			NativeWindow.Title = "Citrus Aurantium";
			NativeWindow.WindowPosition = Gtk.WindowPosition.Center;
			NativeWindow.DefaultSize = new Gdk.Size(500, 400);

			var mainVBox = new Gtk.VBox() { Spacing = 6, BorderWidth = 6 };
			CreateHeaderSection(mainVBox);

			var output = CreateOutputPane();
			mainVBox.PackStart(output, expand: true, fill: true, padding: 0);
			
			var hbox = CreateFooterSection();
			mainVBox.PackStart(hbox, expand: false, fill: true, padding: 0);
			NativeWindow.Add(mainVBox);

			NativeWindow.Hidden += Window_Hidden;
			GoButton.Clicked += GoButton_Clicked;

			NativeWindow.ShowAll();
		}

		private Gtk.Widget CreateFooterSection()
		{
			var hbox = new Gtk.HBox();
			
			// ActionPicker section
			ActionPicker = Gtk.ComboBox.NewText();
			hbox.PackStart(ActionPicker);
			hbox.Spacing = 5;

			// GoButton section
			this.GoButton = new Gtk.Button() { WidthRequest = 80, Label = "Go" };
			hbox.PackEnd(GoButton, expand: false, fill: true, padding: 0);
			return hbox;
		}

		private Gtk.Widget CreateOutputPane()
		{
			var scrolledWindow = new Gtk.ScrolledWindow() { ShadowType = Gtk.ShadowType.In };
			OutputPane = new Gtk.TextView() {
				CanFocus = true,
				Editable = false,
				CursorVisible = false
			};
			scrolledWindow.Add(this.OutputPane);
			return scrolledWindow;
		}

		private void CreateHeaderSection(Gtk.VBox mainVBox)
		{
			var table = new Gtk.Table(2, 2, homogeneous: false) { 
				RowSpacing = 6, 
				ColumnSpacing = 6
			};
			
			// Platform section
			var label1 = new Gtk.Label() { Xalign = 1, LabelProp = "Target platform" };
			table.Attach(label1, 0, 1, 0, 1, xoptions: Gtk.AttachOptions.Fill, yoptions: 0,
				xpadding: 0, ypadding: 0);

			PlatformPicker = Gtk.ComboBox.NewText();
			PlatformPicker.AppendText("Desktop (PC, Mac, Linux)");
			PlatformPicker.AppendText("iPhone/iPad");
			PlatformPicker.AppendText("Unity");
			PlatformPicker.Active = 0;
			table.Attach(PlatformPicker, 1, 2, 0, 1);

			// Citrus project section
			var label2 = new Gtk.Label() { Xalign = 1, LabelProp = "Citrus Project" };
			table.Attach(label2, 0, 1, 1, 2, xoptions: Gtk.AttachOptions.Fill, yoptions: 0,
				xpadding: 0, ypadding: 0);

			CitrusProjectChooser = new Gtk.FileChooserButton("Select a File", Gtk.FileChooserAction.Open);
			table.Attach(CitrusProjectChooser, 1, 2, 1, 2);
			CitrusProjectChooser.FileSet += CitrusProjectChooser_SelectionChanged;
			
			// Svn update section
			UpdateBeforeBuildCheckbox = new Gtk.CheckButton() { Xalign = 1, Label = "Update project before build" };

			// Pack everything to vbox
			mainVBox.PackStart(table, expand: false, fill: false, padding: 0);
			mainVBox.PackStart(UpdateBeforeBuildCheckbox, expand: false, fill: false, padding: 0);
		}
	}
}
