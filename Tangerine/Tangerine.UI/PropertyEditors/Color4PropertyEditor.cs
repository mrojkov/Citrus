using System;
using System.Text.RegularExpressions;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class Color4PropertyEditor : ExpandablePropertyEditor<Color4>
	{
		private EditBox editor;
		private bool colorFromPanel;

		public Color4PropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ColorBoxButton colorBox;
			var panel = new ColorPickerPanel();
			var currentColor = CoalescedPropertyValue(Color4.White).DistinctUntilChanged();
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center) },
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					new HSpacer(4),
					(colorBox = new ColorBoxButton(currentColor)),
					CreatePipetteButton(),
				}
			});
			ExpandableContent.AddNode(panel.Widget);
			panel.Widget.Padding = panel.Widget.Padding + new Thickness(right: 12.0f);
			panel.Widget.Tasks.Add(currentColor.Consume(v => {
				if (!colorFromPanel) {
					panel.Color = v;
				}
				colorFromPanel = false;
			}));
			panel.Changed += () => {
				EditorParams.History?.RollbackTransaction();
				colorFromPanel = true;
				SetProperty(panel.Color);
			};
			panel.DragStarted += () => EditorParams.History?.BeginTransaction();
			panel.DragEnded += () => {
				EditorParams.History?.CommitTransaction();
				EditorParams.History?.EndTransaction();
			};
			colorBox.Clicked += () => Expanded = !Expanded;
			var currentColorString = currentColor.Select(i => i.ToString(Color4.StringPresentation.Dec));
			editor.Submitted += text => SetComponent(text, currentColorString);
			editor.Tasks.Add(currentColorString.Consume(v => editor.Text = v));
			editor.AddChangeWatcher(() => editor.Text, value => CheckEditorText(value, editor));
		}

		private static void CheckEditorText(string value, EditBox editor)
		{
			var match = Regex.Match(value, @"^\s*\[\s*(\d+)\s*\]\s*$");
			if (match.Success) {
				UInt32 number = UInt32.Parse(match.Groups[1].Value);
				editor.Text = $"{0x000000FF & number}. {(0x0000FF00 & number) >> 8}. " +
				              $"{(0x00FF0000 & number) >> 16}. {(0xFF000000 & number) >> 24}";
			}
		}

		public void SetComponent(string text, IDataflowProvider<string> currentColorString)
		{
			Color4 newColor;
			if (Color4.TryParse(text, out newColor)) {
				SetProperty(newColor);
			}
			else {
				editor.Text = currentColorString.GetValue();
			}
		}

		public override void Submit()
		{
			var currentColor = CoalescedPropertyValue(Color4.White).DistinctUntilChanged();
			var currentColorString = currentColor.Select(i => i.ToString(Color4.StringPresentation.Dec));
			SetComponent(editor.Text, currentColorString);
		}

		private Node CreatePipetteButton()
		{
			var button = new ToolbarButton {
				Texture = IconPool.GetTexture("Tools.Pipette"),
			};
			button.Tasks.Add(UIProcessors.PickColorProcessor(button, v => SetProperty(v)));
			return button;
		}

		class ColorBoxButton : Button
		{
			public ColorBoxButton(IDataflowProvider<Color4> colorProvider)
			{
				Nodes.Clear();
				Size = MinMaxSize = new Vector2(25, Theme.Metrics.DefaultButtonSize.Y);
				var color = colorProvider.GetDataflow();
				PostPresenter = new DelegatePresenter<Widget>(widget => {
					widget.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, widget.Size, Color4.White);
					color.Poll();
					var checkSize = new Vector2(widget.Width / 4, widget.Height / 3);
					for (int i = 0; i < 3; i++) {
						var checkPos = new Vector2(widget.Width / 2 + ((i == 1) ? widget.Width / 4 : 0), i * checkSize.Y);
						Renderer.DrawRect(checkPos, checkPos + checkSize, Color4.Black);
					}
					Renderer.DrawRect(Vector2.Zero, widget.Size, color.Value);
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
				});
			}
		}
	}
}
