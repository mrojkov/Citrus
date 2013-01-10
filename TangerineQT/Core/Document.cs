using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Qyoto;

namespace Tangerine
{
	public partial class Document
	{
		public static Document Null = new Document(readOnly: true);
		public static Document Active = Null;
		public static event Lime.BareEventHandler Changed;

		public event Lime.BareEventHandler Closed;
		public string Path { get; private set; }
		public DocumentHistory History = new DocumentHistory();
		public bool ReadOnly;
		public bool IsModified = true;

		public DocumentView View;

		public Document(string path)
		{
			Path = path;
			ReadOnly = IsFileReadonly(path);
			Container = RootNode;
			Active = this;
			RootNode = Lime.Serialization.ReadObjectFromFile<Lime.Node>(path);
			RootNode.AssignMissedGuids();
			Container = RootNode;
			View = new DocumentView(this);
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

		public void OnChanged()
		{
			if (Changed != null) {
				Changed();
			}
		}

		public bool Close()
		{
			string text = string.Format("Save changes to '{0}'?", Path);
			var buttons = QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No | QMessageBox.StandardButton.Cancel;
			var result = QMessageBox.Question(The.DefaultQtParent, "Tangerine", text, buttons);
			if (result == QMessageBox.StandardButton.Cancel) {
				return false;
			}
			if (result == QMessageBox.StandardButton.Yes) {
				Save();
			}
			View.Close();
			if (Closed != null) {
				Closed();
			}
			return true;
		}

		public void Save()
		{
			IsModified = false;
		}

		//public void AddSomeNodes()
		//{
		//	for (int i = 0; i < 20; i++) {
		//		var frame = new Lime.Frame();
		//		frame.Guid = Guid.NewGuid();
		//		frame.Id = string.Format("Frame {0:00}", i);
		//		for (int j = 0; j < 12; j++) {
		//			var image = new Lime.Image();
		//			image.Id = j.ToString("Image 00");
		//			image.Guid = Guid.NewGuid();
		//			frame.AddNode(image);
		//		}

		//		var ani = frame.Animators["Position"];
		//		ani.Add(0, new Lime.Vector2(0, 0));
		//		ani.Add(10, new Lime.Vector2(100, 0));
		//		ani.Add(12, new Lime.Vector2(100, 0));
		//		ani.Add(16, new Lime.Vector2(100, 0));
		//		ani.Add(19, new Lime.Vector2(100, 0));
		//		ani.Add(30, new Lime.Vector2(200, 0));

		//		ani = frame.Animators["Scale"];
		//		ani.Add(5, new Lime.Vector2(0, 0));
		//		ani.Add(15, new Lime.Vector2(100, 0));
		//		//ani.Add(20, new Lime.Vector2(100, 0));
		//		//ani.Add(30, new Lime.Vector2(200, 0));

		//		ani = frame.Animators["Size"];
		//		ani.Add(0, new Lime.Vector2(0, 0));
		//		ani.Add(50, new Lime.Vector2(50, 0));
		//		ani.Add(100, new Lime.Vector2(100, 0));
		//		//ani.Add(20, new Lime.Vector2(100, 0));
		//		//ani.Add(30, new Lime.Vector2(200, 0));

		//		ani = frame.Animators["Rotation"];
		//		ani.Add(15, 0f);
		//		ani.Add(60, 1f);

		//		//ani.Add(50, 0);

		//		RootNode.AddNode(frame);
		//	}
		//	Container = RootNode;
		//	RebuildRows();
		//}
	}
}
