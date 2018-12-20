using System;
using System.Linq;
using System.Text.RegularExpressions;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class Color4PropertyEditor : ExpandablePropertyEditor<Color4>
	{
		private EditBox editor;
		private Color4 lastColor;

		public event Action Changed;

		public Color4PropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ColorBoxButton colorBox;
			var panel = new ColorPickerPanel();
			var currentColor = CoalescedPropertyValue(Color4.White).DistinctUntilChanged();
			EditorContainer.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center) },
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					Spacer.HSpacer(4),
					(colorBox = new ColorBoxButton(currentColor)),
					CreatePipetteButton(),
					Spacer.HStretch(),
				},
			});
			ExpandableContent.AddNode(panel.Widget);
			panel.Widget.Padding = panel.Widget.Padding + new Thickness(right: 12.0f);
			panel.Widget.Tasks.Add(currentColor.Consume(v => {
				if (panel.Color != v.Value) {
					panel.Color = v.Value;
				}
				Changed?.Invoke();
			}));
			panel.Changed += () => {
				EditorParams.History?.RollbackTransaction();
				SetProperty(panel.Color);
			};
			panel.DragStarted += () => {
				EditorParams.History?.BeginTransaction();
				lastColor = panel.Color;
			};
			panel.DragEnded += () => {
				if (panel.Color != lastColor || (editorParams.Objects.Skip(1).Any() && SameValues())) {
					EditorParams.History?.CommitTransaction();
				}
				EditorParams.History?.EndTransaction();
			};
			colorBox.Clicked += () => Expanded = !Expanded;
			var currentColorString = currentColor.Select(i => i.Value.ToString(Color4.StringPresentation.Dec));
			editor.Submitted += text => {
				SetComponent(text, currentColorString);
			};
			editor.Tasks.Add(currentColorString.Consume(v => {
				editor.Text = SameValues() ? v : ManyValuesText;
			}));
			editor.AddChangeWatcher(() => editor.Text, value => {
				CheckEditorText(value, editor);
			});
			ManageManyValuesOnFocusChange(editor, currentColor);
		}

		// TODO: Use Validator
		private static void CheckEditorText(string value, EditBox editor)
		{
			var match = Regex.Match(value, @"^\s*\[\s*(\d+)\s*\]\s*$");
			if (match.Success && uint.TryParse(match.Groups[1].Value, out var number)) {
				editor.Text = $"{0x000000FF & number}. {(0x0000FF00 & number) >> 8}. " +
				              $"{(0x00FF0000 & number) >> 16}. {(0xFF000000 & number) >> 24}";
			}
		}

		public void SetComponent(string text, IDataflowProvider<string> currentColorString)
		{
			if (Color4.TryParse(text, out var newColor)) {
				SetProperty(newColor);
			} else {
				editor.Text = SameValues() ? currentColorString.GetValue() : ManyValuesText;
			}
		}

		public override void Submit()
		{
			var currentColor = CoalescedPropertyValue(Color4.White).DistinctUntilChanged();
			var currentColorString = currentColor.Select(i => i.Value.ToString(Color4.StringPresentation.Dec));
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
			public ColorBoxButton(IDataflowProvider<CoalescedValue<Color4>> colorProvider)
			{
				Nodes.Clear();
				Size = MinMaxSize = new Vector2(25, Theme.Metrics.DefaultButtonSize.Y);
				var color = colorProvider.GetDataflow();
				PostPresenter = new SyncDelegatePresenter<Widget>(widget => {
					widget.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, widget.Size, Color4.White);
					color.Poll();
					var checkSize = new Vector2(widget.Width / 4, widget.Height / 3);
					for (int i = 0; i < 3; i++) {
						var checkPos = new Vector2(widget.Width / 2 + ((i == 1) ? widget.Width / 4 : 0), i * checkSize.Y);
						Renderer.DrawRect(checkPos, checkPos + checkSize, Color4.Black);
					}
					Renderer.DrawRect(Vector2.Zero, widget.Size, color.Value.Value);
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
				});
			}
		}
	}
}
