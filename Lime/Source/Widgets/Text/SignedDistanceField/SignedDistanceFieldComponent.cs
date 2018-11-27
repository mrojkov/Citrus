using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	//[AllowedComponentOwnerTypes(typeof(SimpleText), typeof(RichText))]
	[AllowedComponentOwnerTypes(typeof(Widget))]
	class SignedDistanceFieldComponent : NodeBehavior
	{
		private const string GroupFont = "01. Font";
		private const float MinimumContrast = 0f;
		private const float MaximumContrast = 100f;

		internal SignedDistanceFieldMaterial SDFMaterial { get; private set; } = new SignedDistanceFieldMaterial();

		private SDFPresenter presenter = new SDFPresenter();
		private SDFRenderChainBuilder renderChainBuilder = new SDFRenderChainBuilder();
		private float contrast = 5f;

		[TangerineInspect]
		[TangerineGroup(GroupFont)]
		public float Contrast
		{
			get => contrast;
			set => contrast = Mathf.Clamp(value, MinimumContrast, MaximumContrast);
		}

		public void GetOwnerRenderObjects(RenderChain renderChain, RenderObjectList roObjects)
		{
			DettachFromNode(Owner);
			Owner.AddToRenderChain(renderChain);
			renderChain.GetRenderObjects(roObjects);
			AttachToNode(Owner);
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				DettachFromNode(oldOwner);
			}
			if (Owner != null) {
				AttachToNode(Owner);
			}
		}

		private void AttachToNode(Node node)
		{
			node.Presenter = presenter;
			node.RenderChainBuilder = renderChainBuilder;
			renderChainBuilder.Owner = node.AsWidget;
		}

		private void DettachFromNode(Node node)
		{
			node.RenderChainBuilder = node;
			node.Presenter = DefaultPresenter.Instance;
			renderChainBuilder.Owner = null;
		}

		public override NodeComponent Clone()
		{
			var clone = (SignedDistanceFieldComponent)base.Clone();
			clone.presenter = (SDFPresenter)presenter.Clone();
			clone.renderChainBuilder = (SDFRenderChainBuilder)renderChainBuilder.Clone(null);
			clone.SDFMaterial = (SignedDistanceFieldMaterial)SDFMaterial.Clone();
			return clone;
		}
	}
}
