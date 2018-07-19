using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public enum AlignObject
	{
		Selection,
		KeyObject,
		Root
	}

	public class AlignToHandler : CommandHandler
	{
		ICommand command;

		public AlignToHandler(ICommand command)
		{
			this.command = command;
		}

		public static string AlignToString(AlignObject align)
		{
			switch (align) {
				case AlignObject.Selection:
					return "Align to Selection";
				case AlignObject.KeyObject:
					return "Align to Key Object";
				case AlignObject.Root:
					return "Align to Root";
				default:
					throw new ArgumentException();
			}
		}

		public static ITexture AlignToTexture(AlignObject align)
		{
			return IconPool.GetTexture($"Tools.{Enum.GetName(typeof(AlignObject), align)}");
		}

		private void SetTextAndTexture()
		{
			var alignObject = AlignPreferences.Instance.AlignObject;
			command.Text = AlignToString(AlignPreferences.Instance.AlignObject);
			int index = TangerineApp.Instance.Toolbars["Tools"].IndexOf(Tools.AlignTo);
			var button = (ToolbarButton)TangerineApp.Instance.Toolbars["Tools"].Widget.Nodes[index];
			button.Texture = AlignToTexture(alignObject);
		}

		public override void Execute()
		{
			AlignObjectContextMenu.Create(this);
		}

		private static class AlignObjectContextMenu
		{
			public static void Create(AlignToHandler alignToHandler)
			{
				var menu = new Menu();
				foreach (AlignObject alignObject in Enum.GetValues(typeof(AlignObject))) {
					menu.Add(new Command(AlignToString(alignObject),
						new ChangeAlignObject(alignObject, alignToHandler).Execute));
				}
				menu.Popup();
			}

			private class ChangeAlignObject : CommandHandler
			{
				private readonly AlignObject alignObject;
				private readonly AlignToHandler alignToHandler;

				public ChangeAlignObject( AlignObject alignObject, AlignToHandler alignToHandler)
				{
					this.alignObject = alignObject;
					this.alignToHandler = alignToHandler;
				}

				public override void Execute()
				{
					AlignPreferences.Instance.AlignObject = alignObject;
					alignToHandler.SetTextAndTexture();
				}
			}

		}
	}

	public abstract class AlignToolHandler : DocumentCommandHandler
	{
		protected abstract void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb);
		protected abstract void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb);

		protected static Rectangle NormalizedAABB(Rectangle aabb, Widget container)
		{
			return new Rectangle(aabb.Left / container.Width, aabb.Top / container.Height,
				aabb.Right / container.Width, aabb.Bottom / container.Height);
		}
	}

	public abstract class CenterTool : AlignToolHandler
	{
		public override void ExecuteTransaction()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				HandleWidgets(container, nodes, aabb);
				HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
			}
		}
	}

	public abstract class AlignTool : AlignToolHandler
	{

		private void ToSelection()
		{
			Rectangle aabb;
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			if (Utils.CalcAABB(nodes, container, out aabb)) {
				HandleWidgets(container, nodes, aabb);
				HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
			}
		}
		private void ToKeyObject()
		{
			throw new NotImplementedException();
		}

		private void ToRoot()
		{
			var container = (Widget)Core.Document.Current.Container;
			var nodes = Core.Document.Current.SelectedNodes();
			Rectangle aabb = new Rectangle(0, 0, container.Width, container.Height);
			HandleWidgets(container, nodes, aabb);
			HandlePointObjects(container, nodes, NormalizedAABB(aabb, container));
		}

		public override void ExecuteTransaction()
		{
			switch (AlignPreferences.Instance.AlignObject) {
				case AlignObject.Selection:
					ToSelection();
					break;
				case AlignObject.KeyObject:
					ToKeyObject();
					break;
				case AlignObject.Root:
					ToRoot();
					break;
			}
		}
	}

	public class CenterHorizontally : CenterTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = (container.Width - aabb.Width) / 2 - aabb.A.X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				float d = (1 - aabb.Width) / 2 - aabb.A.X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X += d;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class CenterVertically : CenterTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = (container.Height - aabb.Height) / 2 - aabb.A.Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				float d = (1 - aabb.Height) / 2 - aabb.A.Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y += d;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignCentersHorizontally : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = aabb.Center.X - widget.CalcAABBInSpaceOf(container).Center.X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.X - aabb.Center.X) > Mathf.ZeroTolerance) {
					p.X = aabb.Center.X;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignCentersVertically : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = aabb.Center.Y - widget.CalcAABBInSpaceOf(container).Center.Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y += d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.Y - aabb.Center.Y) > Mathf.ZeroTolerance) {
					p.Y = aabb.Center.Y;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignTop : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Top - aabb.Top;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.Y - aabb.Top) > Mathf.ZeroTolerance) {
					p.Y = aabb.Top;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(PointObject.Position), p, false);
				}
			}
		}
	}

	public class AlignBottom : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Bottom - aabb.Bottom;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.Y - aabb.Bottom) > Mathf.ZeroTolerance) {
					p.Y = aabb.Bottom;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignLeft : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Left - aabb.Left;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.X - aabb.Left) > Mathf.ZeroTolerance) {
					p.X = aabb.Left;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public class AlignRight : AlignTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var widget in nodes.Editable().OfType<Widget>()) {
				var p = widget.Position;
				float d = widget.CalcAABBInSpaceOf(container).Right - aabb.Right;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			foreach (var po in nodes.Editable().OfType<PointObject>()) {
				var p = po.Position;
				if (Mathf.Abs(p.X - aabb.Right) > Mathf.ZeroTolerance) {
					p.X = aabb.Right;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(Widget.Position), p, false);
				}
			}
		}
	}

	public abstract class DistributeTool : AlignTool { }

	public class DistributeVertically : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.Position.Y);
			if (widgets.Count() == 0) {
				return;
			}
			float minY = aabb.Top + widgets.First().CalcAABBInSpaceOf(container).Height / 2;
			float maxY = aabb.Bottom - widgets.Last().CalcAABBInSpaceOf(container).Height / 2;
			float step = (maxY - minY) / (widgets.Count() - 1);
			float Y = minY;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.Y - Y;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.Y -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				Y += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.Y);
			float step = (aabb.Bottom - aabb.Top) / (objects.Count() - 1);
			float Y = aabb.Top;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.Y - Y) > Mathf.ZeroTolerance) {
					p.Y = Y;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				Y += step;
			}
		}
	}

	public class DistributeHorizontally : DistributeTool
	{
		protected override void HandleWidgets(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var widgets = nodes.Editable().OfType<Widget>().OrderBy(w => w.Position.X);
			if (widgets.Count() == 0) {
				return;
			}
			float minX = aabb.Left + widgets.First().CalcAABBInSpaceOf(container).Width / 2;
			float maxX = aabb.Right - widgets.Last().CalcAABBInSpaceOf(container).Width / 2;
			float step = (maxX - minX) / (widgets.Count() - 1);
			float X = minX;
			foreach (var widget in widgets) {
				var p = widget.Position;
				float d = p.X - X;
				if (Mathf.Abs(d) > Mathf.ZeroTolerance) {
					p.X -= d;
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), p, false);
				}
				X += step;
			}
		}

		protected override void HandlePointObjects(Widget container, IEnumerable<Node> nodes, Rectangle aabb)
		{
			var objects = nodes.Editable().OfType<PointObject>().OrderBy(po => po.Position.X);
			float step = (aabb.Right - aabb.Left) / (objects.Count() - 1);
			float X = aabb.Left;
			foreach (var obj in objects) {
				var p = obj.Position;
				if (Mathf.Abs(p.X - X) > Mathf.ZeroTolerance) {
					p.X = X;
					Core.Operations.SetAnimableProperty.Perform(obj, nameof(PointObject.Position), p, false);
				}
				X += step;
			}
		}
	}

}
