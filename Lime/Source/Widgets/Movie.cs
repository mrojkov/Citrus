using Yuzu;

namespace Lime
{
	public enum MovieAction
	{
		Play,
		Pause,
		Stop
	}

	[TangerineRegisterNode(Order = 4)]
	[TangerineVisualHintGroup("/All/Nodes/Media")]
	public sealed class Movie : Widget, IImageCombinerArg
	{
		bool skipRender;
		bool textureInitialized;
		MovieTexture movieTexture;

		[YuzuMember]
		[TangerineKeyframeColor(8)]
		[TangerineFileProperty(new [] { "ogv" })]
		public string Path
		{
			get { return Yuzu.Current?.ShrinkPath(movieTexture.Path) ?? movieTexture.Path; }
			set { movieTexture.Path = Yuzu.Current?.ExpandPath(value) ?? value; }
		}

		[YuzuMember]
		[TangerineKeyframeColor(7)]
		public bool Looped { get; set; }

		[Trigger]
		[TangerineKeyframeColor(17)]
		public MovieAction Action { get; set; }

		public Movie()
		{
			Presenter = DefaultPresenter.Instance;
			movieTexture = new MovieTexture();
		}

		public override Vector2 CalcContentSize()
		{
			return (Vector2)movieTexture.ImageSize;
		}

		public override ITexture Texture
		{
			get { return movieTexture; }
			set { movieTexture = value as MovieTexture; }
		}

		ITexture IImageCombinerArg.GetTexture()
		{
			return movieTexture;
		}

		void IImageCombinerArg.SkipRender()
		{
			skipRender = true;
		}

		Matrix32 IImageCombinerArg.UVTransform { get { return Matrix32.Identity; } }

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && !skipRender && textureInitialized) {
				AddSelfToRenderChain(chain, Layer);
			}
			skipRender = false;
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			movieTexture.Looped = Looped;
			movieTexture.Update(delta);
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(this);
			ro.Texture = movieTexture;
			ro.Color = GlobalColor;
			ro.Size = Size;
			return ro;
		}

		public override void OnTrigger(string property, double animationTimeCorrection = 0)
		{
			if (property == "Action") {
				HandleAction();
			} else {
				base.OnTrigger(property);
			}
		}

		private void HandleAction()
		{
			switch (Action) {
				case MovieAction.Play:
					Play();
					break;
				case MovieAction.Pause:
					Pause();
					break;
				case MovieAction.Stop:
					Stop();
					break;
			}
		}

		public void Stop()
		{
			movieTexture.Stop();
		}

		public void Pause()
		{
			movieTexture.Pause(true);
		}

		public void Play()
		{
			movieTexture.Play();
			textureInitialized = true;
		}

		public bool IsPlaying()
		{
			return (movieTexture != null) && /*!movieTexture.Stopped &&*/ !movieTexture.Paused;
		}

		private class RenderObject : WidgetRenderObject
		{
			public ITexture Texture;
			public Color4 Color;
			public Vector2 Size;

			public override void Render()
			{
				PrepareRenderState();
				Renderer.DrawSprite(Texture, Color, Vector2.Zero, Size, Vector2.Zero, Vector2.One);
			}

			protected override void OnRelease()
			{
				Texture = null;
			}
		}
	}
}
