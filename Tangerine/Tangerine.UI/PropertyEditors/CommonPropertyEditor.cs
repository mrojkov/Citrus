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
		protected const string ManyValuesText = "<many values>";

		public IPropertyEditorParams EditorParams { get; private set; }
		public Widget ContainerWidget { get; private set; }
		public SimpleText PropertyLabel { get; private set; }
		public Widget LabelContainer { get; private set; }
		public Widget EditorContainer { get; private set; }
		public Widget WarningsContainer { get; private set; }
		public Widget PropertyContainerWidget { get; private set; }

		public CommonPropertyEditor(IPropertyEditorParams editorParams)
		{
			EditorParams = editorParams;
			ContainerWidget = new Widget {
				Layout = new VBoxLayout(),
			};
			PropertyContainerWidget = new Widget {
				Layout = new HBoxLayout { IgnoreHidden = false },
			};
			ContainerWidget.AddNode(PropertyContainerWidget);
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
				PropertyContainerWidget.Tasks.Add(Tip.ShowTipOnMouseOverTask(PropertyLabel, () => PropertyLabel.Text));
				PropertyContainerWidget.AddNode(LabelContainer);
				EditorContainer = new Widget {
					Layout = new HBoxLayout(),
					LayoutCell = new LayoutCell { StretchX = 2.0f },
				};
				PropertyContainerWidget.AddNode(EditorContainer);
				WarningsContainer = new Widget {
					Layout = new VBoxLayout(),
					LayoutCell = new LayoutCell(),
				};
				ContainerWidget.AddNode(WarningsContainer);
			} else {
				LabelContainer = EditorContainer = PropertyContainerWidget;
				WarningsContainer = new Widget {
					Layout = new VBoxLayout(),
					LayoutCell = new LayoutCell(),
				};
				ContainerWidget.AddNode(WarningsContainer);
			}
			Validate();
		}

		private void AddWarning(string message, ValidationResult validationResult)
		{
			if (message.IsNullOrWhiteSpace()) {
				return;
			}
			WarningsContainer.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					new Image(IconPool.GetTexture($"Inspector.{validationResult.ToString()}")) {
						MinMaxSize = new Vector2(16, 16),
						LayoutCell = new LayoutCell(Alignment.LeftCenter)
					},
					new ThemedSimpleText {
						Text = message,
						VAlignment = VAlignment.Center,
						LayoutCell = new LayoutCell(Alignment.LeftCenter),
						ForceUncutText = false,
						Padding = new Thickness(left: 5.0f),
						HitTestTarget = true,
						TabTravesable = new TabTraversable()
					}
				}
			});
		}

		private void ClearWarnings()
		{
			WarningsContainer.Nodes.Clear();
		}

		IEnumerator<object> ManageLabelTask()
		{
			var clickGesture0 = new ClickGesture(0, () => {
				PropertyLabel.SetFocus();
			});
			var clickGesture1 = new ClickGesture(1, () => {
				PropertyLabel.SetFocus();
				ShowPropertyContextMenu();
			});
			PropertyLabel.Gestures.Add(clickGesture0);
			PropertyLabel.Gestures.Add(clickGesture1);
			while (true) {
				PropertyLabel.Color = PropertyLabel.IsFocused() ? Theme.Colors.KeyboardFocusBorder : Theme.Colors.BlackText;
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
			JsonOptions = new Yuzu.Json.JsonSerializeOptions { FieldSeparator = " ", Indent = "", EnumAsString = true, SaveRootClass = true, }
		};

		static Yuzu.Json.JsonDeserializer deserializer = new Yuzu.Json.JsonDeserializer {
			JsonOptions = new Yuzu.Json.JsonSerializeOptions { EnumAsString = true }
		};

		protected virtual void Copy()
		{
			var v = CoalescedPropertyValue().GetValue();
			try {
				Clipboard.Text = Serialize(v.Value);
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

		protected IDataflowProvider<T> PropertyValue(object o)
		{
			var indexParameters = EditorParams.PropertyInfo.GetIndexParameters();
			switch (indexParameters.Length)
			{
				case 0:
					return new Property<T>(o, EditorParams.PropertyName);
				case 1 when indexParameters[0].ParameterType == typeof(int):
					return new IndexedProperty<T>(o, EditorParams.PropertyName, EditorParams.IndexInList);
				default:
					throw new NotSupportedException();
			}
		}

		protected IDataflowProvider<CoalescedValue<T>> CoalescedPropertyValue(T defaultValue = default, Func<T, T, bool> comparator = null)
		{
			var indexParameters = EditorParams.PropertyInfo.GetIndexParameters();
			var provider = new CoalescedValuesProvider<T>(defaultValue, comparator);
			switch (indexParameters.Length) {
				case 0:
					foreach (var o in EditorParams.Objects) {
						provider.AddDataflow(new Property<T>(o, EditorParams.PropertyName));
					}
					return new DataflowProvider<CoalescedValue<T>>(() => provider);
				case 1 when indexParameters[0].ParameterType == typeof(int):
					foreach (var o in EditorParams.Objects) {
						provider.AddDataflow(new IndexedProperty<T>(o, EditorParams.PropertyName, EditorParams.IndexInList));
					}
					return new DataflowProvider<CoalescedValue<T>>(() => provider);
				default:
					throw new NotSupportedException();
			}
		}

		protected IDataflowProvider<CoalescedValue<ComponentValue>> CoalescedPropertyComponentValue<ComponentValue>(Func<T, ComponentValue> selector,
			ComponentValue defaultValue = default)
		{
			var indexParameters = EditorParams.PropertyInfo.GetIndexParameters();
			var provider = new CoalescedValuesProvider<ComponentValue>(defaultValue);
			switch (indexParameters.Length) {
				case 0:
					foreach (var o in EditorParams.Objects) {
						provider.AddDataflow(new Property<T>(o, EditorParams.PropertyName).Select(selector));
					}
					return new DataflowProvider<CoalescedValue<ComponentValue>>(() => provider);
				case 1 when indexParameters[0].ParameterType == typeof(int):
					foreach (var o in EditorParams.Objects) {
						provider.AddDataflow(new IndexedProperty<T>(o, EditorParams.PropertyName, EditorParams.IndexInList).Select(selector));
					}
					return new DataflowProvider<CoalescedValue<ComponentValue>>(() => provider);
				default:
					throw new NotSupportedException();
			}
		}

		protected IDataflowProvider<ComponentType> PropertyComponentValue<ComponentType>(object o, Func<T, ComponentType> selector)
		{
			var indexParameters = EditorParams.PropertyInfo.GetIndexParameters();
			switch (indexParameters.Length) {
				case 0:
					return new Property<T>(o, EditorParams.PropertyName).Select(selector);
				case 1 when indexParameters[0].ParameterType == typeof(int):
					return new IndexedProperty<T>(o, EditorParams.PropertyName, EditorParams.IndexInList).Select(selector);
				default:
					throw new NotSupportedException();
			}
		}

		protected void SetProperty(object value)
		{
			ClearWarnings();
			var result = PropertyValidator.ValidateValue(value, EditorParams.PropertyInfo, out string message);
			if (result != ValidationResult.Ok) {
				AddWarning(message, result);
				if (result == ValidationResult.Error) {
					return;
				}
			}
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
			ClearWarnings();
			void ValidateAndApply(object o, ValueType current)
			{
				var next = valueProducer(current);
				var result = PropertyValidator.ValidateValue(next, EditorParams.PropertyInfo, out var message);
				if (result !=ValidationResult.Ok) {
					if (!message.IsNullOrWhiteSpace() && o is Node node) {
						message = $"{node.Id}: {message}";
					}
					AddWarning(message, result);
					if (result == ValidationResult.Error) {
						return;
					}
				}
				((IPropertyEditorParamsInternal)EditorParams).PropertySetter(o,
					EditorParams.IsAnimable ? EditorParams.PropertyPath : EditorParams.PropertyName, next);
			}
			DoTransaction(() => {
				if (EditorParams.IsAnimable) {
					foreach (var o in EditorParams.RootObjects) {
						var (p, a, i) = AnimationUtils.GetPropertyByPath((IAnimationHost)o, EditorParams.PropertyPath);
						var current = i == -1 ? p.Info.GetValue(a) : p.Info.GetValue(a, new object[] { i });
						ValidateAndApply(o, (ValueType)current);
					}
				} else {
					foreach (var o in EditorParams.Objects) {
						var current = EditorParams.IndexInList != -1
							? (new IndexedProperty(o, EditorParams.PropertyName, EditorParams.IndexInList)).Value
							: (new Property(o, EditorParams.PropertyName).Value);
						ValidateAndApply(o, (ValueType)current);
					}
				}
			});
		}

		protected bool SameValues()
		{
			if (!EditorParams.Objects.Any()) {
				return false;
			}
			if (!EditorParams.Objects.Skip(1).Any()) {
				return true;
			}
			var first = PropertyValue(EditorParams.Objects.First()).GetValue();
			return EditorParams.Objects.Aggregate(true,
				(current, o) => current && EqualityComparer<T>.Default.Equals(first, PropertyValue(o).GetValue()));
		}

		protected bool SameComponentValues<ComponentType>(Func<T, ComponentType> selector)
		{
			if (!EditorParams.Objects.Any()) {
				return false;
			}
			var first = PropertyComponentValue(EditorParams.Objects.First(), selector).GetValue();
			return EditorParams.Objects.Aggregate(true,
				(current, o) => current && EqualityComparer<ComponentType>.Default.Equals(first, PropertyComponentValue(o, selector).GetValue()));
		}

		protected void ManageManyValuesOnFocusChange<U>(CommonEditBox editBox, IDataflowProvider<CoalescedValue<U>> current)
		{
			editBox.TextWidget.TextProcessor += (ref string text, Widget widget) => {
				if (!editBox.IsFocused() && !current.GetValue().IsDefined) {
					text = ManyValuesText;
				}
			};

			editBox.AddChangeLateWatcher(editBox.IsFocused, focused => {
				if (!focused && !current.GetValue().IsDefined) {
					editBox.Editor.Text.Invalidate();
				} else if (focused && !current.GetValue().IsDefined) {
					editBox.Text = "";
					editBox.Editor.Text.Invalidate();
				}
			});
		}

		public virtual void Submit()
		{ }

		protected void Validate()
		{
			var objects = EditorParams.IsAnimable ? EditorParams.RootObjects : EditorParams.Objects;
			foreach (var o in EditorParams.Objects) {
				var result = PropertyValidator.ValidateValue(PropertyValue(o).GetValue(), EditorParams.PropertyInfo,
					out var message);
				if (result != ValidationResult.Ok) {
					if (!message.IsNullOrWhiteSpace() && o is Node node) {
						message = $"{node.Id}: {message}";
					}
					AddWarning(message, result);
				}
			}
		}
	}
}
