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

		protected override Node CloneInternal()
		{
			var model = base.CloneInternal() as Model3D;
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
						if (string.IsNullOrEmpty(attachment.EntryTrigger)) {
							return;
						}
						var blender = Components.Get<AnimationBlender>();
						var enabledBlending = false;
						if (blender != null) {
							enabledBlending = blender.Enabled;
							blender.Enabled = false;
						}

						// TODO: Move this to Orange.FbxModelImporter
						var oldTrigger = Trigger;
						Trigger = attachment.EntryTrigger;
						TriggerMultipleAnimations(Trigger);
						var animationBehaviour = Components.Get<AnimationBehaviour>();
						if (animationBehaviour != null) {
							foreach (var a in animationBehaviour.Animations) {
								if (a.IsRunning) {
									a.Advance(0);
								}
							}
						}
						Trigger = oldTrigger;

						if (blender != null) {
							blender.Enabled = enabledBlending;
						}
					}
				}
			}
		}
	}
}
