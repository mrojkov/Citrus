using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(SimpleText), typeof(RichText))]
	class SignedDistanceFieldComponent : NodeBehavior
	{
		private const string GroupFont = "01. Font";

		internal SignedDistanceFieldMaterial SDFMaterial { get; private set; } = new SignedDistanceFieldMaterial();

		private SDFPresenter presenter = new SDFPresenter();
		private SDFRenderChainBuilder renderChainBuilder = new SDFRenderChainBuilder();
	}
}
