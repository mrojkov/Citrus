using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	public class Separator : MenuItem
	{
		Image line = new Image() {
			Id = "Line",
			Shader = ShaderId.Silhuette,
			Height = 2,
			Color = Color4.Black
		};

		public Separator()
		{
			Frame.AddNode(new StackSiblingsHorizontally("Line"));
			Frame.AddNode(new CenterSiblingsVertically());
			Frame.AddNode(new Spacer(5));
			Frame.AddNode(line);
			Frame.AddNode(new Spacer(5));
		}
	}
}
