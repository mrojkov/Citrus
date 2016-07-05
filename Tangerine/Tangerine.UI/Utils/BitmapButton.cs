using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

namespace Tangerine.UI
{
	public class BitmapButton : Button
	{
		public readonly Image Image;
		ITexture defaultTexture;
		ITexture hoverTexture;

		public ITexture DefaultTexture
		{
			get { return defaultTexture; }
			set
			{
				if (defaultTexture != value) {
					defaultTexture = value;
					Refresh();
				}
			}
		}

		public ITexture HoverTexture
		{
			get { return hoverTexture; }
			set
			{
				if (hoverTexture != value) {
					hoverTexture = value;
					Refresh();
				}
			}
		}

		public BitmapButton() : this(Metrics.IconSize) { }

		public BitmapButton(Vector2 size)
		{
			Nodes.Clear();
			Size = MinMaxSize = size;
			DefaultAnimation.AnimationEngine = new ButtonAnimationEngine(this);
			Image = new Image { Size = size };
			Nodes.Add(Image);
		}

		public BitmapButton(ITexture defaultTexture, ITexture hoverTexture, Vector2 size)
			: this(size)
		{
			DefaultTexture = defaultTexture;
			HoverTexture = hoverTexture;
		}

		public BitmapButton(ITexture defaultTexture, ITexture hoverTexture)
			: this(defaultTexture, hoverTexture, Metrics.IconSize)
		{
		}

		void Refresh() => (DefaultAnimation.AnimationEngine as ButtonAnimationEngine).Refresh();

		class ButtonAnimationEngine : AnimationEngine
		{
			string activeAnimation;
			private readonly BitmapButton button;

			public ButtonAnimationEngine(BitmapButton button)
			{
				this.button = button;
			}

			public override bool TryRunAnimation(Animation animation, string markerId)
			{
				activeAnimation = markerId;
				Refresh();
				return true;
			}

			public void Refresh()
			{
				button.Image.Texture = activeAnimation == "Focus" ? button.HoverTexture : button.DefaultTexture;
			}
		}
	}	
}