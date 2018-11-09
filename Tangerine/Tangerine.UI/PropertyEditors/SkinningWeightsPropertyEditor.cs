using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class SkinningWeightsPropertyEditor : ExpandablePropertyEditor<SkinningWeights>
	{
		private readonly NumericEditBox[] indexEditors;
		private readonly NumericEditBox[] weigthsEditors;
		public SkinningWeightsPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editorParams.DefaultValueGetter = () => new SkinningWeights();
			indexEditors = new NumericEditBox[4];
			weigthsEditors = new NumericEditBox[4];
			foreach (var o in editorParams.Objects) {
				var prop = new Property<SkinningWeights>(o, editorParams.PropertyName).Value;
			}
			for (var i = 0; i <= 3; i++) {
				indexEditors[i] = editorParams.NumericEditBoxFactory();
				indexEditors[i].Step = 1;
				weigthsEditors[i] = editorParams.NumericEditBoxFactory();
				var wrapper = new Widget {
					Padding = new Thickness { Left = 20 },
					Layout = new HBoxLayout(),
					LayoutCell = new LayoutCell { StretchY = 0 }
				};
				var propertyLabel = new ThemedSimpleText {
					Text = $"Bone { char.ConvertFromUtf32(65 + i) }",
					VAlignment = VAlignment.Center,
					LayoutCell = new LayoutCell(Alignment.LeftCenter, 0),
					ForceUncutText = false,
					MinWidth = 140,
					OverflowMode = TextOverflowMode.Minify,
					HitTestTarget = true,
					TabTravesable = new TabTraversable(),
				};
				wrapper.AddNode(propertyLabel);
				wrapper.AddNode(new Widget {
					Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center), Spacing = 4 },
					Nodes = {
						indexEditors[i] ,
						weigthsEditors[i]
					}
				});
				ExpandableContent.AddNode(wrapper);
				var j = i;
				SetLink(i, CoalescedPropertyComponentValue(sw => sw[j].Index), CoalescedPropertyComponentValue(sw => sw[j].Weight));
			}
		}

		private void SetLink(int idx, IDataflowProvider<CoalescedValue<int>> indexProvider, IDataflowProvider<CoalescedValue<float>> weightProvider)
		{
			var currentIndexValue = indexProvider.GetValue();
			var currentWeightValue = weightProvider.GetValue();
			indexEditors[idx].Submitted += text => SetIndexValue(EditorParams, idx, indexEditors[idx], currentIndexValue);
			weigthsEditors[idx].Submitted += text => SetWeightValue(EditorParams, idx, weigthsEditors[idx], currentWeightValue);
			indexEditors[idx].AddChangeWatcher(indexProvider,
				v => indexEditors[idx].Text = v.IsUndefined ? v.Value.ToString() : ManyValuesText);
			weigthsEditors[idx].AddChangeWatcher(weightProvider,
				v => weigthsEditors[idx].Text = v.IsUndefined ? v.Value.ToString("0.###") : ManyValuesText);
		}

		private void SetIndexValue(IPropertyEditorParams editorParams, int idx, CommonEditBox editor, CoalescedValue<int> prevValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					SetProperty<SkinningWeights>((current) => {
						current[idx] = new BoneWeight {
							Index = (int)newValue,
							Weight = current[idx].Weight
						};
						return current;
					});
				});
			} else {
				editor.Text = prevValue.IsUndefined ? prevValue.Value.ToString() : ManyValuesText;
			}
		}

		private void SetWeightValue(IPropertyEditorParams editorParams, int idx, CommonEditBox editor, CoalescedValue<float> prevWeight)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					SetProperty<SkinningWeights>((current) => {
						current[idx] = new BoneWeight {
							Index = current[idx].Index,
							Weight = newValue
						};
						return current;
					});
				});
			} else {
				editor.Text = prevWeight.IsUndefined ? prevWeight.Value.ToString("0.###") : ManyValuesText;
			}
		}
	}
}
