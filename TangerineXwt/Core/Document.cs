using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xwt;

namespace Tangerine
{
	public partial class Document
	{
		public static Document Null = new Document(readOnly: true);
		public static Document Active = Null;
		public static event System.Action Changed;

		public event System.Action Closed;
		public string Path { get; private set; }
		public DocumentHistory History = new DocumentHistory();
		public bool ReadOnly;
		public bool IsModified = true;

		public DocumentView View;

		public Document(string path)
		{
			RootNode = new Lime.Frame();
			Path = path;
			//ReadOnly = IsFileReadonly(path);
			Container = RootNode;
			Active = this;
			GenerateDocumentSampleContent();
			//RootNode = Lime.Serialization.ReadObjectFromFile<Lime.Node>(path);
			RootNode.AssignMissedGuids();
			Container = RootNode;
			View = new DocumentView(this);
		}

		void GenerateDocumentSampleContent()
		{
			for (int i = 1; i < 10; i++) {
				var img = new Lime.Frame() { Id = "Image " + i };
				var pos = img.Animators ["Position"];
				var scale = img.Animators ["Scale"];
				for (int j = 0; j < 10; j++) {
					pos.Add (new Lime.KeyFrame() { Frame = j * 10, Value = new Lime.Vector2(0, j) });
					//scale[j * 10 + 5] = new Lime.KeyFrame() { Value = new Lime.Vector2(1, 1) };
				}
				RootNode.AddNode (img);
			}
		}

		private static bool IsFileReadonly(string path)
		{
			return (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0;
		}

		public Document(bool readOnly)
		{
			this.ReadOnly = readOnly;
			RootNode = new Lime.Widget();
			RootNode.Guid = Guid.NewGuid();
			Container = RootNode;
			Active = this;
		}

		public void UpdateViews()
		{
			if (Changed != null) {
				Changed();
			}
		}

		public bool Close()
		{
			//string text = string.Format("Save changes to '{0}'?", Path);
			//var message = new Xwt.ConfirmationMessage(text, Xwt.Command.Yes);
			//var buttons = QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No | QMessageBox.StandardButton.Cancel;
			//var result = QMessageBox.Question(The.DefaultQtParent, "Tangerine", text, buttons);
			//if (result == QMessageBox.StandardButton.Cancel) {
			//	return false;
			//}
			//if (result == QMessageBox.StandardButton.Yes) {
			//	Save();
			//}
			//View.Close();
			//if (Closed != null) {
			//	Closed();
			//}
			return true;
		}

		public void Save()
		{
			IsModified = false;
		}

		public void AddSomeNodes()
		{
			for (int i = 0; i < 20; i++) {
				var frame = new Lime.Frame();
				frame.Guid = Guid.NewGuid();
				frame.Id = string.Format("Frame {0:00}", i);
				for (int j = 0; j < 12; j++) {
					var image = new Lime.Image();
					image.Id = j.ToString("Image 00");
					image.Guid = Guid.NewGuid();
					frame.AddNode(image);
				}

				var ani = frame.Animators["Position"];
				ani.Add(0, new Lime.Vector2(0, 0));
				ani.Add(10, new Lime.Vector2(100, 0));
				ani.Add(12, new Lime.Vector2(100, 0));
				ani.Add(16, new Lime.Vector2(100, 0));
				ani.Add(19, new Lime.Vector2(100, 0));
				ani.Add(30, new Lime.Vector2(200, 0));

				ani = frame.Animators["Scale"];
				ani.Add(5, new Lime.Vector2(0, 0));
				ani.Add(15, new Lime.Vector2(100, 0));
				//ani.Add(20, new Lime.Vector2(100, 0));
				//ani.Add(30, new Lime.Vector2(200, 0));

				ani = frame.Animators["Size"];
				ani.Add(0, new Lime.Vector2(0, 0));
				ani.Add(50, new Lime.Vector2(50, 0));
				ani.Add(100, new Lime.Vector2(100, 0));
				//ani.Add(20, new Lime.Vector2(100, 0));
				//ani.Add(30, new Lime.Vector2(200, 0));

				ani = frame.Animators["Rotation"];
				ani.Add(15, 0f);
				ani.Add(60, 1f);

				//ani.Add(50, 0);

				RootNode.AddNode(frame);
			}
			Container = RootNode;
			RebuildRows();
		}
	}
}
