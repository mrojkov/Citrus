using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class CustomCheckbox : Button
	{
		readonly Image image;
		readonly ITexture checkedTexture;
		readonly ITexture uncheckedTexture;
		bool @checked;

		public bool Checked
		{
			get { return @checked; }
			set
			{
				@checked = value;
				Refresh(); 
			}
		}

		public CustomCheckbox(ITexture @checked, ITexture @unchecked)
		{
			checkedTexture = @checked;
			uncheckedTexture = @unchecked;
			image = new Image();
			Nodes.Add(image);
			Refresh();
		}

		void Refresh()
		{
			image.Texture = Checked ? checkedTexture : uncheckedTexture;
			if (image.Texture != null) {
				MinMaxSize = image.Size = (Vector2)image.Texture.ImageSize;
			}
		}
	}	
}
