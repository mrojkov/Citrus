using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.PopupMenu
{
	/// <summary>
	/// Виджет, отображающий элементы отладочного меню
	/// </summary>
	public class MenuButton : Button
	{
		Widget hbox = new Widget();

		Image background = new Image() {
			Shader = ShaderId.Silhuette,
		};

		ImageBox imageBox = new ImageBox(new Image()) {
			Size = (MenuItem.Height * 0.8f) * Vector2.One,
			Visible = false
		};

		/// <summary>
		/// Путь к текстуре
		/// </summary>
		public string ImagePath
		{
			get { return imageBox.Image.Texture.SerializationPath; }
			set {
				imageBox.Image.Texture = new SerializableTexture(value);
				imageBox.Visible = !imageBox.Image.Texture.IsStubTexture;
			}
		}

		SimpleText captionLabel = new SimpleText() {
			Id = "TextPresenter",
			FontHeight = MenuItem.Height * 0.8f,
			Color = Color4.Black,
			OverflowMode = TextOverflowMode.Ellipsis,
		};

		SimpleText arrowLabel = new SimpleText() {
			FontHeight = MenuItem.Height * 0.75f,
			Color = Color4.Gray,
			Text = ">",
		};

		/// <summary>
		/// Видна ли стрелка справа (показывает, что есть вложенное меню)
		/// </summary>
		public bool ArrowVisible
		{
			get { return arrowLabel.Visible; }
			set { arrowLabel.Visible = value; }
		}

		public MenuButton()
		{
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
			hbox.AddNode(new StackSiblingsHorizontally(stretch: "TextPresenter"));
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

			Markers.Add(new Marker("Normal", 0, MarkerAction.Stop));
			bgColor.Keys.Add(0, Color4.White);
			lbColor.Keys.Add(0, Color4.Black);
			arColor.Keys.Add(0, Color4.Gray);

			Markers.Add(new Marker("Focus", 10, MarkerAction.Stop));
			bgColor.Keys.Add(10, Color4.Gray);
			lbColor.Keys.Add(10, Color4.Black);
			arColor.Keys.Add(10, Color4.Black);

			Markers.Add(new Marker("Press", 20, MarkerAction.Stop));
			bgColor.Keys.Add(20, Color4.Black);
			lbColor.Keys.Add(20, Color4.White);
			arColor.Keys.Add(20, Color4.White);

			Markers.Add(new Marker("Disable", 30, MarkerAction.Stop));
			bgColor.Keys.Add(30, Color4.White);
			lbColor.Keys.Add(30, Color4.Gray);
			arColor.Keys.Add(30, Color4.Gray);
		}
	}
}
