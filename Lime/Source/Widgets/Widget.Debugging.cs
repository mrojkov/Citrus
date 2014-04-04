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

			public struct WidgetBasis
			{
				public Vector2 Position;
				public Vector2 Scale;
				public float Rotation;

				public override string ToString()
				{
					return string.Format("Position: {0}; Scale: {1}; Rotation: {2}",
						Position, Scale, Rotation);
				}
			}

			public struct WidgetPainting
			{
				public Color4 Color;
				public Blending Blending;
				public bool Visible;
				public override string ToString()
				{
					return string.Format("Visible: {0}, Color: {1}; Blending: {2}", Visible, Color, Blending);
				}
			}

			public WidgetDebugView(Widget widget)
			{
				this.widget = widget;
			}

			public string Id { get { return widget.Id; } }

			public Node Parent { get { return widget.Parent; } }

			public Marker[] Markers { get { return widget.Markers.AsArray(); } }

			public Vector2 Size { get { return widget.Size; } }

			public Vector2 Pivot { get { return widget.Pivot; } }

			public WidgetBasis LocalBasis
			{
				get
				{
					return new WidgetBasis {
						Position = widget.Position,
						Scale = widget.Scale,
						Rotation = widget.Rotation,
					};
				}
			}

			public WidgetPainting LocalPainting
			{
				get
				{
					return new WidgetPainting {
						Visible = widget.Visible,
						Color = widget.Color,
						Blending = widget.Blending
					};
				}
			}

			public WidgetBasis GlobalBasis
			{
				get
				{
					widget.RecalcGlobalMatrixAndColor();
					var b = widget.CalcTransformFromMatrix(widget.LocalToWorldTransform);
					return new WidgetBasis {
						Position = b.Position,
						Rotation = b.Rotation,
						Scale = b.Scale,
					};
				}
			}

			public WidgetPainting GlobalPainting
			{
				get
				{
					widget.RecalcGlobalMatrixAndColor();
					return new WidgetPainting {
						Visible = widget.GloballyVisible,
						Color = widget.GlobalColor,
						Blending = widget.GlobalBlending
					};
				}
			}

			//[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
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
			if (!ChildOf(World.Instance) && (this != World.Instance)) {
				suggestions.Add("Widget is not added to the main hierarchy");
			}
			if (!Visible) {
				suggestions.Add("Flag 'Visible' is not set");
			} else if (Opacity == 0) {
				suggestions.Add("It is fully transparent! Check up 'Opacity' property!");
			} else if (Opacity < 0.1f) {
				suggestions.Add("It is almost transparent! Check up 'Opacity' property!");
			} else if (!GloballyVisible) {
				suggestions.Add("One of its parent has 'Visible' flag not set");
			} else if (GlobalColor.A < 10) {
				suggestions.Add("One of its parent has 'Opacity' close to zero");
			}
			var basis = CalcTransformInSpaceOf(World.Instance);
			if (Mathf.Abs(basis.Scale.X) < 0.01f || Mathf.Abs(basis.Scale.Y) < 0.01f) {
				suggestions.Add(string.Format("Widget is probably too small (Scale: {0})", basis.Scale));
			}
			bool withinScreenBounds =
				basis.Position.X > 10 && basis.Position.X < World.Instance.Width - 10 &&
				basis.Position.Y > 10 && basis.Position.Y < World.Instance.Height - 10;
			if (!withinScreenBounds) {
				suggestions.Add(string.Format("Widget is possible out of the screen (Position: {0})", basis.Position));
			}
			if (!(this is Image) && (this.Nodes.Count == 0)) {
				suggestions.Add("Widget hasn't any drawable node");
			}
		}
	}
}
