using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class CommonPropertyEditor<T> : IPropertyEditor
	{
		public IPropertyEditorParams EditorParams { get; private set; }
		public Widget ContainerWidget { get; private set; }
		public SimpleText PropertyLabel { get; private set; }
		public Widget LabelContainer { get; private set; }
		public Widget EditorContainer { get; private set; }

		public CommonPropertyEditor(IPropertyEditorParams editorParams)
		{
			EditorParams = editorParams;
			ContainerWidget = new Widget {
				Layout = new HBoxLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			//ContainerWidget.CompoundPostPresenter.Add(new LayoutDebugPresenter(Color4.Red, 2.0f));
			editorParams.InspectorPane.AddNode(ContainerWidget);
			if (editorParams.ShowLabel) {
				LabelContainer = new Widget {
					Layout = new HBoxLayout(),
					LayoutCell = new LayoutCell { StretchX = 1.0f },
					Nodes = {
						(PropertyLabel = new ThemedSimpleText {
							Text = editorParams.DisplayName ?? editorParams.PropertyName,
							VAlignment = VAlignment.Center,
							LayoutCell = new LayoutCell(Alignment.LeftCenter),
							ForceUncutText = false,
							Padding = new Thickness(left: 5.0f),
							HitTestTarget = true,
							TabTravesable = new TabTraversable()
						})
					}
				};
				PropertyLabel.Tasks.Add(ManageLabelTask());
				ContainerWidget.Tasks.Add(Tip.ShowTipOnMouseOverTask(PropertyLabel, () => PropertyLabel.Text));
				ContainerWidget.AddNode(LabelContainer);
				EditorContainer = new Widget {
					Layout = new HBoxLayout(),
					LayoutCell = new LayoutCell { StretchX = 2.0f },
				};
				ContainerWidget.AddNode(EditorContainer);
			} else {
				LabelContainer = EditorContainer = ContainerWidget;
			}
		}

		IEnumerator<object> ManageLabelTask()
		{
			while (true) {
				var popupMenu = PropertyLabel.Input.WasMouseReleased(1);
				if (popupMenu || PropertyLabel.Input.WasMouseReleased(0)) {
					PropertyLabel.SetFocus();
				}
				PropertyLabel.Color = PropertyLabel.IsFocused() ? Theme.Colors.KeyboardFocusBorder : Theme.Colors.BlackText;
				if (popupMenu) {
					// Wait until the label actually change its color.
					yield return null;
					yield return null;
					ShowPropertyContextMenu();
				}
				if (PropertyLabel.IsFocused()) {
					if (Command.Copy.WasIssued()) {
						Command.Copy.Consume();
						Copy();
					}
					if (Command.Paste.WasIssued()) {
						Command.Paste.Consume();
						Paste();
					}
					if (resetToDefault.WasIssued()) {
						resetToDefault.Consume();
						var defaultValue = EditorParams.DefaultValueGetter();
						if (defaultValue != null)
							SetProperty(defaultValue);
					}
				}
				yield return null;
			}
		}

		static Yuzu.Json.JsonSerializer serializer = new Yuzu.Json.JsonSerializer {
			JsonOptions = new Yuzu.Json.JsonSerializeOptions { FieldSeparator = " ", Indent = "", EnumAsString = true }
		};

		static Yuzu.Json.JsonDeserializer deserializer = new Yuzu.Json.JsonDeserializer {
			JsonOptions = new Yuzu.Json.JsonSerializeOptions { EnumAsString = true }
		};

		protected virtual void Copy()
		{
			var v = CoalescedPropertyValue().GetValue();
			try {
				Clipboard.Text = Serialize(v);
			} catch (System.Exception) { }
		}

		protected virtual void Paste()
		{
			try {
				var v = Deserialize(Clipboard.Text);
				SetProperty(v);
			} catch (System.Exception) { }
		}

		protected virtual string Serialize(T value) => serializer.ToString(value);
		protected virtual T Deserialize(string source) => deserializer.FromString<T>(source + ' ');

		protected void DoTransaction(Action block)
		{
			if (EditorParams.History != null) {
				using (EditorParams.History.BeginTransaction()) {
					block();
					EditorParams.History.CommitTransaction();
				}
			} else {
				block();
			}
		}

		private ICommand resetToDefault = new Command("Reset To Default");

		protected virtual void FillContextMenuItems(Menu menu)
		{
			menu.AddRange(new [] {
				Command.Copy,
				Command.Paste,
			});
			if (EditorParams.DefaultValueGetter != null) {
				menu.Insert(0, resetToDefault);
			}
		}

		void ShowPropertyContextMenu()
		{
			var menu = new Menu {};
			FillContextMenuItems(menu);
			menu.Popup();
		}

		public virtual void DropFiles(IEnumerable<string> files) { }

		protected IDataflowProvider<T> CoalescedPropertyValue(T defaultValue = default(T))
		{
			var indexParameters = EditorParams.PropertyInfo.GetIndexParameters();
			if (indexParameters.Length == 0) {
				IDataflowProvider<T> provider = null;
				foreach (var o in EditorParams.Objects) {
					var p = new Property<T>(o, EditorParams.PropertyName);
					provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
				}
				return provider;
			} else if (indexParameters.Length == 1 && indexParameters.First().ParameterType == typeof(int)) {
				IDataflowProvider<T> provider = null;
				foreach (var o in EditorParams.Objects) {
					var p = new IndexedProperty<T>(o, EditorParams.PropertyName, EditorParams.IndexInList);
					provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
				}
				return provider;
			} else {
				throw new NotSupportedException();
			}
		}

		protected IDataflowProvider<ComponentType> CoalescedPropertyComponentValue<ComponentType>(Func<T, ComponentType> selector, ComponentType defaultValue = default(ComponentType))
		{
			var indexParameters = EditorParams.PropertyInfo.GetIndexParameters();
			if (indexParameters.Length == 0) {
				IDataflowProvider<ComponentType> provider = null;
				foreach (var o in EditorParams.Objects) {
					var p = new Property<T>(o, EditorParams.PropertyName).Select(selector);
					provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
				}
				return provider;
			} else if (indexParameters.Length == 1 && indexParameters.First().ParameterType == typeof(int)) {
				IDataflowProvider<ComponentType> provider = null;
				foreach (var o in EditorParams.Objects) {
					var p = new IndexedProperty<T>(o, EditorParams.PropertyName, EditorParams.IndexInList).Select(selector);
					provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
				}
				return provider;
			} else {
				throw new NotSupportedException();
			}
		}

		protected void SetProperty(object value)
		{
			DoTransaction(() => {
				if (EditorParams.IsAnimable) {
					foreach (var o in EditorParams.RootObjects) {
						((IPropertyEditorParamsInternal)EditorParams).PropertySetter(o, EditorParams.PropertyPath, value);
					}
				} else {
					foreach (var o in EditorParams.Objects) {
						((IPropertyEditorParamsInternal)EditorParams).PropertySetter(o, EditorParams.PropertyName, value);
					}
				}
			});
		}

		protected void SetProperty<ValueType>(Func<ValueType, object> valueProducer)
		{
			DoTransaction(() => {
				if (EditorParams.IsAnimable) {
					foreach (var o in EditorParams.RootObjects) {
						var (p, a, i) = AnimationUtils.GetPropertyByPath((IAnimationHost)o, EditorParams.PropertyPath);
						var current = i == -1 ? p.Info.GetValue(a) : p.Info.GetValue(a, new object[] { i });
						((IPropertyEditorParamsInternal)EditorParams).PropertySetter(o, EditorParams.PropertyPath, valueProducer((ValueType)current));
					}
				} else {
					foreach (var o in EditorParams.Objects) {
						var current = EditorParams.IndexInList != -1
							? (new IndexedProperty(o, EditorParams.PropertyName, EditorParams.IndexInList)).Value
							: (new Property(o, EditorParams.PropertyName).Value);
						((IPropertyEditorParamsInternal)EditorParams).PropertySetter(o, EditorParams.PropertyName, valueProducer((ValueType)current));
					}
				}
			});
		}

		public virtual void Submit()
		{ }
	}
}
