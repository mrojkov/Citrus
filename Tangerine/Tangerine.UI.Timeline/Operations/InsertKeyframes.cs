using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.Timeline.Operations
{
	public static class InsertPositionKeyframe
	{
		public static void Perform()
		{
			Core.Document.Current.History.DoTransaction(() => {
				foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
					Core.Operations.SetAnimableProperty.Perform(
						widget,
						nameof(Widget.Position),
						widget.Position,
						createAnimatorIfNeeded: true,
						createInitialKeyframeForNewAnimator: false
					);
				}
				foreach (var bone in Core.Document.Current.SelectedNodes().Editable().OfType<Bone>()) {
					Core.Operations.SetAnimableProperty.Perform(
						bone,
						nameof(Bone.Position),
						bone.Position,
						createAnimatorIfNeeded: true,
						createInitialKeyframeForNewAnimator: false
					);
				}
				foreach (var pointObject in Core.Document.Current.SelectedNodes().Editable().OfType<PointObject>()) {
					Core.Operations.SetAnimableProperty.Perform(
						pointObject,
						nameof(PointObject.Position),
						pointObject.Position,
						createAnimatorIfNeeded: true,
						createInitialKeyframeForNewAnimator: false
					);
				}
			});
		}
	}

	public static class InsertRotationKeyframe
	{
		public static void Perform()
		{
			Core.Document.Current.History.DoTransaction(() => {
				foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
					Core.Operations.SetAnimableProperty.Perform(
						widget,
						nameof(Widget.Rotation),
						widget.Rotation,
						createAnimatorIfNeeded: true,
						createInitialKeyframeForNewAnimator: false
					);
				}
				foreach (var bone in Core.Document.Current.SelectedNodes().Editable().OfType<Bone>()) {
					Core.Operations.SetAnimableProperty.Perform(
						bone,
						nameof(Bone.Rotation),
						bone.Rotation,
						createAnimatorIfNeeded: true,
						createInitialKeyframeForNewAnimator: false
					);
				}
			});
		}
	}

	public static class InsertScaleKeyframe
	{
		public static void Perform()
		{
			Core.Document.Current.History.DoTransaction(() => {
				foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
					Core.Operations.SetAnimableProperty.Perform(
						widget,
						nameof(Widget.Scale),
						widget.Scale,
						createAnimatorIfNeeded: true,
						createInitialKeyframeForNewAnimator: false
					);
				}
			});
		}
	}
}
