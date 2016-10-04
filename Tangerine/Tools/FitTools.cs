using System;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class ResetScale : Command
	{
		public override string Text => "Reset scale";
		public override ITexture Icon => UI.IconPool.GetTexture("Tools.SetUnitScale");

		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetProperty.Perform(widget, "Scale", Vector2.One);
			}
		}
	}

	public class ResetRotation : Command
	{
		public override string Text => "Reset rotation";
		public override ITexture Icon => UI.IconPool.GetTexture("Tools.SetZeroRotation");

		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetProperty.Perform(widget, "Rotation", 0);
			}
		}
	}

	public class FlipX : Command
	{
		public override string Text => "Flip horizontally";
		public override ITexture Icon => UI.IconPool.GetTexture("Tools.FlipH");

		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				var s = widget.Scale;
				s.X = -s.X;
				Core.Operations.SetProperty.Perform(widget, "Scale", s);
			}
		}
	}

	public class FlipY : Command
	{
		public override string Text => "Flip vertically";
		public override ITexture Icon => UI.IconPool.GetTexture("Tools.FlipV");

		public override void Execute()
		{
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				var s = widget.Scale;
				s.Y = -s.Y;
				Core.Operations.SetProperty.Perform(widget, "Scale", s);
			}
		}
	}

	public class FitToContainer : Command
	{
		public override string Text => "Fit to container";
		public override ITexture Icon => UI.IconPool.GetTexture("Tools.FitToContainer");

		public override void Execute()
		{
			var container = (Widget)Core.Document.Current.Container;
			foreach (var widget in Core.Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
				Core.Operations.SetProperty.Perform(widget, "Size", container.Size);
				Core.Operations.SetProperty.Perform(widget, "Rotation", 0);
				Core.Operations.SetProperty.Perform(widget, "Position", widget.Pivot * container.Size);
				Core.Operations.SetProperty.Perform(widget, "Scale", Vector2.One);
				Core.Operations.SetProperty.Perform(widget, "Anchors", Anchors.LeftRightTopBottom);
			}
		}
	}

	public class FitToContent : Command
	{
		public override string Text => "Fit to content";
		public override ITexture Icon => UI.IconPool.GetTexture("Tools.FitToContent");

		public override void Execute()
		{
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes().Editable();
			foreach (var widget in nodes.OfType<Widget>()) {
				Rectangle aabb;
				if (Utils.CalcAABB(widget.Nodes, widget, out aabb)) {
					foreach (var w in widget.Nodes.OfType<Widget>()) {
						Core.Operations.SetProperty.Perform(w, "Position", w.Position - aabb.A);
					}
					foreach (var po in widget.Nodes.OfType<PointObject>()) {
						Core.Operations.SetProperty.Perform(po, "Position", po.Position - aabb.A);
					}
					var p0 = widget.CalcTransitionToSpaceOf(container) * aabb.A;
					Core.Operations.SetProperty.Perform(widget, "Size", aabb.Size);
					var p1 = widget.CalcTransitionToSpaceOf(container) * Vector2.Zero;
					Core.Operations.SetProperty.Perform(widget, "Position", widget.Position + p0 - p1);
				}
			}
		}
	}
}
