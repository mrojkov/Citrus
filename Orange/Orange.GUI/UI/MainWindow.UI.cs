using System;
using System.IO;
using Gtk;

namespace Orange
{
	public partial class MainWindow : UserInterface
	{
		public Window NativeWindow;
		public FileChooserButton CitrusProjectChooser;
		private ComboBox platformPicker;
		public Gtk.TextView OutputPane;
		public ComboBox ActionPicker;
		public CheckButton UpdateBeforeBuildCheckbox;
		public Button GoButton;
		private HBox mainHBox;

		public override void Initialize()
		{
			base.Initialize();
			Application.Init();

			Create();
			TextWriter writer = new LogWriter(OutputPane);
			Console.SetOut(writer);
			Console.SetError(writer);
			GoButton.GrabFocus();
			NativeWindow.Show();

			CreateMenuItems();
			The.Workspace.Load();
			Application.Run();

			UpdatePlatformPicker();
		}

		public override void ProcessPendingEvents()
		{
			while (Application.EventsPending()) {
				Application.RunIteration();
			}
		}

		private static void CreateMenuItems()
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			The.MenuController.CreateAssemblyMenuItems();
		}

		private void Create()
		{
			NativeWindow = new Window(WindowType.Toplevel) {
				Title = "Citrus Aurantium",
				WindowPosition = WindowPosition.Center,
				DefaultSize = new Gdk.Size(500, 400)
			};
			mainHBox = new HBox {
				Name = "MainHBox"
			};
			var mainVBox = new VBox {
				Spacing = 6,
				BorderWidth = 6,
				Name = "MainVBox"
			};

			CreateHeaderSection(mainVBox);

			var output = CreateOutputPane();
			mainVBox.PackStart(output, expand: true, fill: true, padding: 0);

			var hbox = CreateFooterSection();
			mainVBox.PackStart(hbox, expand: false, fill: true, padding: 0);

			mainHBox.Add(mainVBox);

			NativeWindow.Add(mainHBox);

			NativeWindow.Hidden += Window_Hidden;
			GoButton.Clicked += GoButton_Clicked;

			NativeWindow.ShowAll();
		}

		private Widget CreateFooterSection()
		{
			var hbox = new HBox();

			// ActionPicker section
			ActionPicker = ComboBox.NewText();
			hbox.PackStart(ActionPicker);
			hbox.Spacing = 5;

			// GoButton section
			GoButton = new Button() { WidthRequest = 80, Label = "Go" };
			hbox.PackEnd(GoButton, expand: false, fill: true, padding: 0);
			return hbox;
		}

		private Widget CreateOutputPane()
		{
			var scrolledWindow = new ScrolledWindow() { ShadowType = ShadowType.In };
			OutputPane = new Gtk.TextView() {
				CanFocus = true,
				Editable = false,
				CursorVisible = false
			};
			scrolledWindow.Add(OutputPane);
			return scrolledWindow;
		}

		private void CreateHeaderSection(VBox mainVBox)
		{
			var table = new Table(2, 2, homogeneous: false) {
				RowSpacing = 6,
				ColumnSpacing = 6
			};

			// Platform section
			var label1 = new Label() { Xalign = 1, LabelProp = "Target" };
			table.Attach(label1, 0, 1, 0, 1, xoptions: AttachOptions.Fill, yoptions: 0,
				xpadding: 0, ypadding: 0);

			platformPicker = ComboBox.NewText();
			UpdatePlatformPicker();
			table.Attach(platformPicker, 1, 2, 0, 1);

			// Citrus project section
			var label2 = new Label() { Xalign = 1, LabelProp = "Citrus Project" };
			table.Attach(label2, 0, 1, 1, 2, xoptions: AttachOptions.Fill, yoptions: 0,
				xpadding: 0, ypadding: 0);

			CitrusProjectChooser = new FileChooserButton("Select a File", FileChooserAction.Open);
			table.Attach(CitrusProjectChooser, 1, 2, 1, 2);
			CitrusProjectChooser.FileSet += CitrusProjectChooser_SelectionChanged;

			// Svn update section
			UpdateBeforeBuildCheckbox = new CheckButton() { Xalign = 1, Label = "Update project before build" };

			// Pack everything to vbox
			mainVBox.PackStart(table, expand: false, fill: false, padding: 0);
			mainVBox.PackStart(UpdateBeforeBuildCheckbox, expand: false, fill: false, padding: 0);
		}

		private void UpdatePlatformPicker()
		{
			(platformPicker.Model as ListStore).Clear();

			foreach (var target in The.Workspace.Targets)
				platformPicker.AppendText(target.Name);

			platformPicker.Active = 0;
		}
	}
}
