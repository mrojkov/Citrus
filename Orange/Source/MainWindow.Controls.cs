namespace Orange
{
	public partial class MainWindow
	{
		private Gtk.FileChooserButton CitrusProjectChooser;
		private Gtk.ComboBox TargetPlatformPicker;
		private Gtk.TextView OutputPane;
		private Gtk.ComboBox ActionPicker;
		private Gtk.Button GoButton;

		private void CreateControls()
		{
			Title = "Citrus Aurantium";
			WindowPosition = Gtk.WindowPosition.Center;
			DefaultSize = new Gdk.Size(500, 400);

			var mainVBox = new Gtk.VBox() { Spacing = 6, BorderWidth = 6 };
			var header = CreateHeaderSection();
			mainVBox.PackStart(header, expand: false, fill: false, padding: 0);

			var output = CreateOutputPane();
			mainVBox.PackStart(output, expand: true, fill: true, padding: 0);
			
			var hbox = CreateFooterSection();
			mainVBox.PackStart(hbox, expand: false, fill: true, padding: 0);
			Add (mainVBox);

			Hidden += Window_Hidden;
			GoButton.Clicked += GoButton_Clicked;

			ShowAll();
		}

		private Gtk.Widget CreateFooterSection()
		{
			var hbox = new Gtk.HBox();
			
			// ActionPicker section
			ActionPicker = Gtk.ComboBox.NewText();
			ActionPicker.AppendText("Build Game & Run");
			ActionPicker.AppendText("Build Content Only");
			ActionPicker.AppendText("Rebuild Game");
			ActionPicker.AppendText("Reveal Content");
			ActionPicker.AppendText("Extract Tangerine Scenes");
			ActionPicker.AppendText("Extract Translatable Strings");
			ActionPicker.AppendText("Generate Serialization Assembly");
			ActionPicker.Active = 0;
			hbox.PackStart(ActionPicker);
			hbox.Spacing = 5;

			// GoButton section
			this.GoButton = new Gtk.Button() { WidthRequest = 80, Label = "_Go" };
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

		private Gtk.Widget CreateHeaderSection()
		{
			var table = new Gtk.Table(2, 2, homogeneous: false) { 
				RowSpacing = 6, 
				ColumnSpacing = 6
			};
			
			// Target platform section
			var label1 = new Gtk.Label() { Xalign = 1, LabelProp = "Target platform" };
			table.Attach(label1, 0, 1, 0, 1, xoptions: Gtk.AttachOptions.Fill, yoptions: 0,
				xpadding: 0, ypadding: 0);

			TargetPlatformPicker = Gtk.ComboBox.NewText();
			TargetPlatformPicker.AppendText("Desktop (PC, Mac, Linux)");
			TargetPlatformPicker.AppendText("iPhone/iPad");
			TargetPlatformPicker.AppendText("Unity");
			TargetPlatformPicker.Active = 0;
			table.Attach(TargetPlatformPicker, 1, 2, 0, 1);

			// Citrus project section
			var label2 = new Gtk.Label() { Xalign = 1, LabelProp = "Citrus Project" };
			table.Attach(label2, 0, 1, 1, 2, xoptions: Gtk.AttachOptions.Fill, yoptions: 0,
				xpadding: 0, ypadding: 0);

			CitrusProjectChooser = new Gtk.FileChooserButton("Select a File", Gtk.FileChooserAction.Open);
			table.Attach(CitrusProjectChooser, 1, 2, 1, 2);
			return table;
		}
	}
}
