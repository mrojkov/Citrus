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
			void InvokeTrigger(string trigger)
			{
				SplitTrigger(trigger, out var markerId, out var animationId);
				TryRunAnimation(markerId, animationId);
			}

			base.LoadExternalScenes(yuzu);
			yuzu = yuzu ?? Yuzu.Instance.Value;
			if (ContentsPath != null) {
				var attachmentPath = System.IO.Path.ChangeExtension(ContentsPath, ".Attachment.txt");
				if (AssetBundle.Current.FileExists(attachmentPath)) {
					using (var stream = AssetBundle.Current.OpenFileLocalized(attachmentPath)) {
						var attachment = yuzu.ReadObject<Model3DAttachmentParser.ModelAttachmentFormat>(attachmentPath, stream);
						if (string.IsNullOrEmpty(attachment.EntryTrigger)) return;
						if (attachment.EntryTrigger.IndexOf(',') >= 0) {
							foreach (var s in attachment.EntryTrigger.Split(',')) {
								InvokeTrigger(s.Trim());
							}
						} else {
							InvokeTrigger(attachment.EntryTrigger.Trim());
						}
						Update(0);
					}
				}
			}
		}

		protected static void SplitTrigger(string trigger, out string markerId, out string animationId)
		{
			if (!trigger.Contains('@')) {
				markerId = trigger;
				animationId = null;
			} else {
				var t = trigger.Split('@');
				markerId = t[0];
				animationId = t[1];
			}
		}
	}
}
