using System.Linq;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 22)]
	[TangerineVisualHintGroup("/All/Nodes/3D")]
	public class Model3D : Node3D
	{
		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{
			RebuildSkeleton();
		}

		public override Node Clone()
		{
			var model = base.Clone() as Model3D;
			model.RebuildSkeleton();
			return model;
		}

		public void RebuildSkeleton()
		{
			var submeshes = Descendants
				.OfType<Mesh3D>()
				.SelectMany(m => m.Submeshes);
			foreach (var sm in submeshes) {
				sm.RebuildSkeleton(this);
			}
		}

		public override void LoadExternalScenes(Yuzu yuzu = null)
		{
			base.LoadExternalScenes(yuzu);
			yuzu = yuzu ?? Yuzu.Instance.Value;
			if (ContentsPath != null) {
				var attachmentPath = System.IO.Path.ChangeExtension(ContentsPath, ".Attachment.txt");
				if (AssetBundle.Current.FileExists(attachmentPath)) {
					using (var stream = AssetBundle.Current.OpenFileLocalized(attachmentPath)) {
						var attachment = yuzu.ReadObject<Model3DAttachmentParser.ModelAttachmentFormat>(attachmentPath, stream);
						if (TryRunAnimation(attachment.EntryMarker, attachment.EntryAnimation)) {
							ApplyAnimationImmediately(this, true);
						}
					}
				}
			}
		}

		private static void ApplyAnimationImmediately(Node node, bool requiredTriggerSelf, Animation animation = null)
		{
			if (animation == null) {
				foreach (var nodeAnimation in node.Animations) {
					if (nodeAnimation.IsRunning) {
						InvokeAnimationTriggers(node, nodeAnimation, requiredTriggerSelf);
					}
				}
			} else {
				if (animation.IsRunning) {
					InvokeAnimationTriggers(node, animation, requiredTriggerSelf);
				}
			}
			foreach (var child in node.Nodes) {
				ApplyAnimationImmediately(child, requiredTriggerSelf: false);
			}
		}

		private static void InvokeAnimationTriggers(Node node, Animation animation, bool requiredTriggerSelf)
		{
			if (requiredTriggerSelf) {
				for (var animator = node.Animators.First; animator != null; animator = animator.Next) {
					if (animator.IsTriggerable && animator.AnimationId == animation.Id) {
						animator.InvokeTrigger(animation.Frame);
					}
				}
			}
			foreach (var n in node.Nodes) {
				for (var animator = n.Animators.First; animator != null; animator = animator.Next) {
					if (animator.IsTriggerable && animator.AnimationId == animation.Id) {
						animator.InvokeTrigger(animation.Frame);
						animator.Apply(animation.Time);
					}
				}
			}
		}
	}
}
