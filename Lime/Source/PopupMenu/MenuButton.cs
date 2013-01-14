using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	public class MenuButton : Button
	{
		Widget hbox = new Widget();

		Image background = new Image() {
			Blending = Blending.Silhuette,
		};

		ImageBox imageBox = new ImageBox(new Image()) {
			Size = (MenuItem.Height * 0.8f) * Vector2.One,
			Visible = false
		};

		public string ImagePath
		{
			get { return imageBox.Image.Texture.SerializationPath; }
			set {
				imageBox.Image.Texture = new SerializableTexture(value);
				imageBox.Visible = !imageBox.Image.Texture.ChessTexture;
			}
		}

		SimpleText captionLabel = new SimpleText() {
			Id = "TextPresenter",
			FontHeight = MenuItem.Height * 0.8f,
			Color = Color4.Black,
		};

		SimpleText arrowLabel = new SimpleText() {
			FontHeight = MenuItem.Height * 0.75f,
			Color = Color4.Gray,
			Text = ">",
		};

		public bool ArrowVisible
		{
			get { return arrowLabel.Visible; }
			set { arrowLabel.Visible = value; }
		}

		public MenuButton()
		{
			PropagateAnimation = true;
			CreateNodes();
			SetupStates();
			ArrowVisible = false;
		}

		private void CreateNodes()
		{
			captionLabel.Size = captionLabel.CalcContentSize();
			arrowLabel.Size = arrowLabel.CalcContentSize();
			AddNode(new ExpandSiblingsToParent());
			AddNode(hbox);
			AddNode(background);
			hbox.AddNode(new StackSiblingsHorizontally("TextPresenter"));
			hbox.AddNode(new CenterSiblingsVertically());
			hbox.AddNode(new Spacer(10));
			hbox.AddNode(imageBox);
			hbox.AddNode(new Spacer(10));
			hbox.AddNode(captionLabel);
			hbox.AddNode(new Spacer(10));
			hbox.AddNode(arrowLabel);
			hbox.AddNode(new Spacer(10));
		}

		private void SetupStates()
		{
			var bgColor = background.Animators["Color"];
			var lbColor = captionLabel.Animators["Color"];
			var arColor = arrowLabel.Animators["Color"];

			Markers.AddStopMarker("Normal", 0);
			bgColor.Add(0, Color4.White);
			lbColor.Add(0, Color4.Black);
			arColor.Add(0, Color4.Gray);

			Markers.AddStopMarker("Focus", 10);
			bgColor.Add(10, Color4.Gray);
			lbColor.Add(10, Color4.Black);
			arColor.Add(10, Color4.Black);

			Markers.AddStopMarker("Press", 20);
			bgColor.Add(20, Color4.Black);
			lbColor.Add(20, Color4.White);
			arColor.Add(20, Color4.White);

			Markers.AddStopMarker("Disable", 30);
			bgColor.Add(30, Color4.White);
			lbColor.Add(30, Color4.Gray);
			arColor.Add(30, Color4.Gray);
		}
	}
}
