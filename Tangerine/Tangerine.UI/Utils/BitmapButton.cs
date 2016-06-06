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
		public ITexture DefaultTexture { get; set; }
		public ITexture HoverTexture { get; set; }

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

		class ButtonAnimationEngine : AnimationEngine
		{
			private readonly BitmapButton button;

			public ButtonAnimationEngine(BitmapButton button)
			{
				this.button = button;
			}

			public override bool TryRunAnimation(Animation animation, string markerId)
			{
				button.Image.Texture = markerId == "Focus" ? button.HoverTexture : button.DefaultTexture;
				return true;
			}
		}
	}	
}