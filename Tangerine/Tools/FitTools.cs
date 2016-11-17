using System;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class ResetScale : DocumentCommandHandler
	{
		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Scale), Vector2.One);
			}
		}
	}

	public class ResetRotation : DocumentCommandHandler
	{
		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Rotation), 0);
			}
		}
	}

	public class FlipX : DocumentCommandHandler
	{
		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				var s = widget.Scale;
				s.X = -s.X;
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Scale), s);
			}
		}
	}

	public class FlipY : DocumentCommandHandler
	{
		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				var s = widget.Scale;
				s.Y = -s.Y;
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Scale), s);
			}
		}
	}

	public class FitToContainer : DocumentCommandHandler
	{
		public override void Execute()
		{
			var container = (Widget)Core.Document.Current.Container;
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Size), container.Size);
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Rotation), 0);
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), widget.Pivot * container.Size);
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Scale), Vector2.One);
				Core.Operations.SetProperty.Perform(widget, nameof(Widget.Anchors), Anchors.LeftRightTopBottom);
			}
		}
	}

	public class FitToContent : DocumentCommandHandler
	{
		public override void Execute()
		{
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes().Editable();
			foreach (var widget in nodes.OfType<Widget>()) {
				Rectangle aabb;
				if (Utils.CalcAABB(widget.Nodes, widget, out aabb)) {
					foreach (var w in widget.Nodes.OfType<Widget>()) {
						Core.Operations.SetProperty.Perform(w, nameof(Widget.Position), w.Position - aabb.A);
					}
					foreach (var po in widget.Nodes.OfType<PointObject>()) {
						Core.Operations.SetProperty.Perform(po, nameof(Widget.Position), po.Position - aabb.A);
					}
					var p0 = widget.CalcTransitionToSpaceOf(container) * aabb.A;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Size), aabb.Size);
					var p1 = widget.CalcTransitionToSpaceOf(container) * Vector2.Zero;
					Core.Operations.SetProperty.Perform(widget, nameof(Widget.Position), widget.Position + p0 - p1);
				}
			}
		}
	}
}
