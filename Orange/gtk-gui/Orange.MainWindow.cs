
// This file has been generated by the GUI designer. Do not modify.
namespace Orange
{
	public partial class MainWindow
	{
		private global::Gtk.VBox dialog1_VBox;
		private global::Gtk.VBox vbox2;
		private global::Gtk.Table table1;
		private global::Gtk.FileChooserButton CitrusProjectChooser;
		private global::Gtk.Label label1;
		private global::Gtk.Label label2;
		private global::Gtk.ComboBox TargetPlatform;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TextView CompileLog;
		private global::Gtk.Table table3;
		private global::Gtk.ComboBox Action;
		private global::Gtk.Button GoButton;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget Orange.MainWindow
			this.Name = "Orange.MainWindow";
			this.Title = "Orange";
			this.WindowPosition = ((global::Gtk.WindowPosition)(1));
			// Container child Orange.MainWindow.Gtk.Container+ContainerChild
			this.dialog1_VBox = new global::Gtk.VBox ();
			this.dialog1_VBox.Name = "dialog1_VBox";
			this.dialog1_VBox.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			this.vbox2.BorderWidth = ((uint)(4));
			// Container child vbox2.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(2)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.CitrusProjectChooser = new global::Gtk.FileChooserButton ("Select a File", ((global::Gtk.FileChooserAction)(0)));
			this.CitrusProjectChooser.Name = "CitrusProjectChooser";
			this.CitrusProjectChooser.ShowHidden = true;
			this.table1.Add (this.CitrusProjectChooser);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1 [this.CitrusProjectChooser]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = "Citrus project";
			this.table1.Add (this.label1);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.label1]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = "Target platform";
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.TargetPlatform = global::Gtk.ComboBox.NewText ();
			this.TargetPlatform.AppendText ("Desktop (PC, Mac, Linux)");
			this.TargetPlatform.AppendText ("iPhone/iPad");
			this.TargetPlatform.Name = "TargetPlatform";
			this.TargetPlatform.Active = 0;
			this.table1.Add (this.TargetPlatform);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.TargetPlatform]));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add (this.table1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.table1]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.CompileLog = new global::Gtk.TextView ();
			this.CompileLog.CanFocus = true;
			this.CompileLog.Name = "CompileLog";
			this.CompileLog.Editable = false;
			this.CompileLog.CursorVisible = false;
			this.GtkScrolledWindow.Add (this.CompileLog);
			this.vbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow]));
			w7.Position = 1;
			this.dialog1_VBox.Add (this.vbox2);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox [this.vbox2]));
			w8.Position = 0;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table3 = new global::Gtk.Table (((uint)(1)), ((uint)(2)), false);
			this.table3.Name = "table3";
			this.table3.RowSpacing = ((uint)(6));
			this.table3.ColumnSpacing = ((uint)(6));
			this.table3.BorderWidth = ((uint)(5));
			// Container child table3.Gtk.Table+TableChild
			this.Action = global::Gtk.ComboBox.NewText ();
			this.Action.AppendText ("Build Game & Run");
			this.Action.AppendText ("Build Content Only");
			this.Action.AppendText ("Rebuild Game");
			this.Action.AppendText ("Reveal Content");
			this.Action.AppendText("Extract Tangerine Scenes");
			this.Action.AppendText ("Extract Translatable Strings");
			this.Action.AppendText ("Generate Serialization Assembly");
			this.Action.Name = "Action";
			this.Action.Active = 0;
			this.table3.Add (this.Action);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table3 [this.Action]));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.GoButton = new global::Gtk.Button ();
			this.GoButton.WidthRequest = 80;
			this.GoButton.CanFocus = true;
			this.GoButton.Name = "GoButton";
			this.GoButton.UseUnderline = true;
			this.GoButton.Label = "_Go";
			this.table3.Add (this.GoButton);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table3 [this.GoButton]));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.dialog1_VBox.Add (this.table3);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.dialog1_VBox [this.table3]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			this.Add (this.dialog1_VBox);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 469;
			this.DefaultHeight = 417;
			this.Show ();
			this.Hidden += new global::System.EventHandler (this.OnHidden);
			this.GoButton.Clicked += new global::System.EventHandler (this.OnGoButtonClicked);
		}
	}
}
