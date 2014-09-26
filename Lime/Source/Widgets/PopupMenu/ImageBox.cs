using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	/// <summary>
	/// This class provides proportional scaling of inner image
	/// </summary>
	public class ImageBox : Widget
	{
		public Image Image;

		public ImageBox(Image image)
		{
			this.Image = image;
			Nodes.Add(image);
		}

		private void AdjustImage()
		{
			float minSideLength = Math.Min(Width, Height);
			Vector2 imageSize = (Vector2)Image.Texture.ImageSize;
			if (imageSize.X > imageSize.Y) {
				Image.Width = minSideLength;
				Image.Height = imageSize.Y / imageSize.X * minSideLength;
			} else {
				Image.Height = minSideLength;
				Image.Width = imageSize.X / imageSize.Y * minSideLength;
			}
			Image.Position = Size * Vector2.Half;
			Image.Pivot = Vector2.Half;
		}

		protected override void SelfUpdate(float delta)
		{
			AdjustImage();
		}
	}
}
