using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace Lime
{
	[DebuggerTypeProxy(typeof(WidgetDebugView))]
	public partial class Widget : Node
	{
		internal class WidgetDebugView
		{
			private Widget widget;

			public struct Geometry
			{
				public Vector2 Position;
				public Vector2 Scale;
				public Vector2 Pivot;
				public Vector2 Size;
				public float Rotation;
				public Color4 Color;
				public bool Visible;
				public Blending Blending;

				public override string ToString()
				{
					return string.Format("Position: {0}", Position);
				}
			}

			public WidgetDebugView(Widget widget)
			{
				this.widget = widget;
			}

			public Node Parent { get { return widget.Parent; } }

			public Geometry LocalGeometry
			{
				get
				{
					return new Geometry {
						Color = widget.Color,
						Position = widget.Position,
						Size = widget.Size,
						Scale = widget.Scale,
						Rotation = widget.Rotation,
						Blending = widget.Blending,
						Visible = widget.Visible,
						Pivot = widget.Pivot
					};
				}
			}

			public Geometry GlobalGeometry
			{
				get
				{
					widget.RecalcGlobalMatrixAndColor();
					var b = widget.CalcBasisFromMatrix(widget.GlobalMatrix);
					return new Geometry {
						Position = b.Position,
						Pivot = widget.Pivot,
						Rotation = b.Rotation,
						Scale = b.Scale,
						Size = widget.Size,
						Visible = widget.GloballyVisible,
						Blending = widget.GlobalBlending,
						Color = widget.GlobalColor
					};
				}
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public Node[] Nodes { get { return widget.Nodes.AsArray; } }

			public List<string> DiagnoseVisibility
			{
				get
				{
					var suggestions = new List<string>();
					widget.GetReasonsWhyItCanBeInvisible(suggestions);
					return suggestions;
				}
			}
		}

		public virtual void GetReasonsWhyItCanBeInvisible(List<string> suggestions)
		{
			RecalcGlobalMatrixAndColor();
			if (!ChildOf(RootFrame.Instance)) {
				suggestions.Add("Widget is not added to the main hierarchy");
			}
			if (!Visible) {
				suggestions.Add("Flag 'Visible' is not set");
			} else if (Opacity == 0) {
				suggestions.Add("It is fully transparent! Check up 'Opacity' property!");
			} else if (Opacity < 0.1f) {
				suggestions.Add("It is almost transparent! Check up 'Opacity' property!");
			} else if (!globallyVisible) {
				suggestions.Add("One of its parent has 'Visible' flag not set");
			} else if (globalColor.A < 10) {
				suggestions.Add("One of its parent has 'Opacity' close to zero");
			}
			var basis = CalcBasisInSpaceOf(RootFrame.Instance);
			if (Mathf.Abs(basis.Scale.X) < 0.01f || Mathf.Abs(basis.Scale.Y) < 0.01f) {
				suggestions.Add(string.Format("Widget is probably too small (Scale: {0})", basis.Scale));
			}
			bool withinScreenBounds =
				basis.Position.X > 10 && basis.Position.X < RootFrame.Instance.Width - 10 &&
				basis.Position.Y > 10 && basis.Position.Y < RootFrame.Instance.Height - 10;
			if (!withinScreenBounds) {
				suggestions.Add(string.Format("Widget is possible out of the screen (Position: {0})", basis.Position));
			}
			if (!(this is Image) && (this.Nodes.Count == 0)) {
				suggestions.Add("Widget has no any drawable node");
			}
		}
	}
}
