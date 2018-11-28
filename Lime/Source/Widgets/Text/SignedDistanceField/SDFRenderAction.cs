
namespace Lime
{
	internal abstract class SDFRenderAction
	{
		public abstract bool EnabledCheck(SDFRenderObject ro);
		public abstract void Do(SDFRenderObject ro);
	}
}
