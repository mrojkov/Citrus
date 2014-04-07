using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum MovieAction
	{
		[ProtoEnum]
		Play,
		[ProtoEnum]
		Pause,
		[ProtoEnum]
		Stop
	}

	[ProtoContract]
	public sealed class Movie : Widget, IImageCombinerArg
	{
		bool skipRender;
		bool requestSkipRender;
		bool textureInitialized;
		MovieTexture movieTexture;

		[ProtoMember(1)]
		public string Path
		{
			get { return Serialization.ShrinkPath(movieTexture.Path); }
			set { movieTexture.Path = Serialization.ExpandPath(value); }
		}

		[ProtoMember(2)]
		public bool Looped { get; set; }

		[Trigger]
		public MovieAction Action { get; set; }

		public Movie()
		{
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
			requestSkipRender = true;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && !skipRender && textureInitialized) {
				chain.Add(this, Layer);
			}
		}

		public override void Update(int delta)
		{
			skipRender = requestSkipRender;
			requestSkipRender = false;
			movieTexture.Looped = Looped;
			movieTexture.Update(delta * 0.001f);
			base.Update(delta);
		}

		public override void Render()
		{
			var pam = Renderer.PremultipliedAlphaMode;
			try {
				Renderer.PremultipliedAlphaMode = false;
				Renderer.Blending = Blending.None;
				Renderer.Blending = GlobalBlending;
				Renderer.Transform1 = LocalToWorldTransform;
				Renderer.DrawSprite(movieTexture, GlobalColor, Vector2.Zero, Size, Vector2.Zero, Vector2.One);
				Renderer.FlushSpriteBatch();
			} finally {
				Renderer.Blending = Blending.None;
				Renderer.PremultipliedAlphaMode = pam;
			}
		}

		protected override bool SelfHitTest(Vector2 point)
		{
			if (!GloballyVisible || skipRender || !InsideClipRect(point)) {
				return false;
			}
			return base.HitTest(point);
		}

		protected internal override void OnTrigger(string property)
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
					movieTexture.Play();
					textureInitialized = true;
					break;
				case MovieAction.Pause:
					movieTexture.Pause(true);
					break;
				case MovieAction.Stop:
					movieTexture.Stop();
					break;
			}
		}
	}
}