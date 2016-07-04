using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Tangerine.Core
{
	public interface ISelectedObjectsProvider
	{
		IEnumerable<object> Get();
	}

	public interface IAnimationContext
	{
		int AnimationFrame { get; set; }
		string AnimationId { get; set; }
	}

	public class Document : IAnimationContext
	{
		public enum CloseAction
		{
			Cancel,
			SaveChanges,
			DiscardChanges
		}
		
		public static readonly Document Null = new Document { ReadOnly = true };
		public static Document Current { get; set; }

		public string Path { get; private set; }
		public readonly DocumentHistory History;
		public bool ReadOnly { get; private set; }
		public bool IsModified { get; private set; }

		public event Func<CloseAction> Closing;
		public event Action Closed;
		
		public Node RootNode { get; private set; }
		public Node Container { get; set; }
		public IEnumerable<object> SelectedObjects => SelectedObjectsProvider.Get();

		public int AnimationFrame
		{
			get { return Container.AnimationFrame; }
			set { Container.AnimationFrame = value; }
		}

		public string AnimationId { get; set; }
		public ISelectedObjectsProvider SelectedObjectsProvider { get; set; }

		static Document()
		{
			Current = Null;
		}

		public Document()
		{
			History = new DocumentHistory();
		}

		public Document(string path) : this()
		{
			Path = path;
			ReadOnly = IsFileReadonly(path);
			RootNode = Lime.Serialization.ReadObjectFromFile<Lime.Node>(path);
		}

		private static bool IsFileReadonly(string path)
		{
			return (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0;
		}

		public bool Close()
		{
			if (Closing != null) {
				var a = Closing();
				if (a == CloseAction.Cancel) {
					return false;
				} else if (a == CloseAction.SaveChanges) {
					Save();
				}
			}
			if (Closed != null) {
				Closed();
			}
			return true;
		}

		public void Save()
		{
			IsModified = false;
		}
		
		public void AddSomeNodes()
		{
			RootNode = new Widget();
			RootNode.Markers.Add(new Marker { Id = "Start", Action = MarkerAction.Play, Frame = 10 });
			RootNode.Markers.Add(new Marker { Id = "Loop", Action = MarkerAction.Jump, Frame = 20, JumpTo = "Start" });
			for (int i = 0; i < 10; i++) {
				var frame = new Lime.Frame();
				frame.Id = string.Format("Frame {0:00}", i);
				for (int j = 0; j < 12; j++) {
					var image = new Lime.Image();
					image.Id = j.ToString("Image 00");
					frame.AddNode(image);
				}
				if (i % 2 == 0) {
					var ani = frame.Animators["Position"];
					ani.Keys.Add(0, new Lime.Vector2(0, 0));
					ani.Keys.Add(5, new Lime.Vector2(100, 0));
					ani.Keys.Add(12, new Lime.Vector2(200, 50));
					ani.Keys.Add(16, new Lime.Vector2(100, 0));
					ani.Keys.Add(19, new Lime.Vector2(100, 0));
					ani.Keys.Add(30, new Lime.Vector2(200, 0));

					ani = frame.Animators["Scale"];
					ani.Keys.Add(5, new Lime.Vector2(0, 0));
					ani.Keys.Add(15, new Lime.Vector2(100, 0));
					//ani.Add(20, new Lime.Vector2(100, 0));
					//ani.Add(30, new Lime.Vector2(200, 0));

					ani = frame.Animators["Size"];
					ani.Keys.Add(0, new Lime.Vector2(0, 0));
					ani.Keys.Add(50, new Lime.Vector2(50, 0));
					ani.Keys.Add(100, new Lime.Vector2(100, 0));
					////ani.Add(20, new Lime.Vector2(100, 0));
					////ani.Add(30, new Lime.Vector2(200, 0));

					ani = frame.Animators["Rotation"];
					ani.Keys.Add(15, 0f);
					ani.Keys.Add(60, 1f);
				}
				RootNode.AddNode(frame);
			}
			RootNode.AnimationFrame = 0;
			// Container = RootNode;
		}		
	}
}
