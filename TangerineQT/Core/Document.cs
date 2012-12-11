using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class Document
	{
		public static Document Instance;

		public Lime.Node RootNode { get; private set; }

		public Document()
		{
			Instance = this;
			RootNode = new Lime.Widget();
			for (int i = 0; i < 100; i++) {
				var frame = new Lime.Frame();
				frame.Id = string.Format("Image {0:00}", i);
				
				var ani = frame.Animators["Position"];
				ani.Add(0, new Lime.Vector2(0, 0));
				ani.Add(10, new Lime.Vector2(100, 0));
				ani.Add(20, new Lime.Vector2(100, 0));
				ani.Add(30, new Lime.Vector2(200, 0));

				ani = frame.Animators["Scale"];
				ani.Add(0, new Lime.Vector2(0, 0));
				ani.Add(10, new Lime.Vector2(100, 0));
				ani.Add(20, new Lime.Vector2(100, 0));
				ani.Add(30, new Lime.Vector2(200, 0));

				ani = frame.Animators["Size"];
				ani.Add(0, new Lime.Vector2(0, 0));
				ani.Add(10, new Lime.Vector2(100, 0));
				ani.Add(20, new Lime.Vector2(100, 0));
				ani.Add(30, new Lime.Vector2(200, 0));

				ani = frame.Animators["Rotation"];
				ani.Add(0, 0f);
				ani.Add(10, 0f);
				ani.Add(20, 0f);
				ani.Add(30, 0f);

				RootNode.AddNode(frame);
			}
		}
	}
}
