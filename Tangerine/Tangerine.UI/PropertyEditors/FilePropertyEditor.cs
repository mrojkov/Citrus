using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public abstract class FilePropertyEditor<T> : ExpandablePropertyEditor<T>
	{
		private class PrefixData
		{
			public string Prefix { get; set; }
		}

		protected readonly EditBox editor;
		private readonly StringPropertyEditor prefixEditor;
		protected static string LastOpenedDirectory = Path.GetDirectoryName(Document.Current.FullPath);
		protected readonly string[] allowedFileTypes;

		private readonly Button button;
		private readonly PrefixData prefix = new PrefixData();

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editor.Enabled = Enabled;
			button.Enabled = Enabled;
			prefixEditor.Enabled = Enabled;
		}

		public bool ShowPrefix { get; set; } = true;

		protected FilePropertyEditor(IPropertyEditorParams editorParams, string[] allowedFileTypes) : base(editorParams)
		{
			this.allowedFileTypes = allowedFileTypes;
			EditorContainer.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					Spacer.HSpacer(4),
					(button = new ThemedButton {
						Text = "...",
						MinMaxWidth = 20,
						LayoutCell = new LayoutCell(Alignment.Center)
					})
				}
			});
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Submitted += text => SetComponent(text);
			button.Clicked += OnSelectClicked;
			ExpandableContent.Padding = new Thickness(24, 10, 2, 2);
			prefixEditor = new StringPropertyEditor(new PropertyEditorParams(ExpandableContent, prefix, nameof(PrefixData.Prefix)) { LabelWidth = 180 });
			prefix.Prefix = GetLongestCommonPrefix(GetPaths());
			ContainerWidget.AddChangeWatcher(() => prefix.Prefix, v => {
				string oldPrefix = GetLongestCommonPrefix(GetPaths());
				if (oldPrefix == v) {
					return;
				}
				SetPathPrefix(oldPrefix, v);
				prefix.Prefix = v.Trim('/');
			});
			ContainerWidget.AddChangeWatcher(() => ShowPrefix, show => {
				Expanded = show && Expanded;
				ExpandButton.Visible = show;
			});
			var current = CoalescedPropertyValue();
			editor.AddChangeLateWatcher(current, v => editor.Text = ValueToStringConverter(v.Value) ?? "");
			ManageManyValuesOnFocusChange(editor, current);
		}

		protected override void FillContextMenuItems(Menu menu)
		{
			base.FillContextMenuItems(menu);
			if (EditorParams.Objects.Skip(1).Any()) {
				return;
			}
			var path = GetPaths().First();
			if (!string.IsNullOrEmpty(path)) {
				path = Path.Combine(Project.Current.AssetsDirectory, path);
				FilesystemCommands.NavigateTo.UserData = path;
				menu.Insert(0, FilesystemCommands.NavigateTo);
				FilesystemCommands.OpenInSystemFileManager.UserData = path;
				menu.Insert(0, FilesystemCommands.OpenInSystemFileManager);
			}
		}

		private List<string> GetPaths()
		{
			var result = new List<string>();
			foreach (var o in EditorParams.Objects) {
				result.Add(ValueToStringConverter(PropertyValue(o).GetValue()));
			}
			return result;
		}

		private void SetPathPrefix(string oldPrefix, string prefix) =>
			this.SetProperty<T>(current => StringToValueConverter(AssetPath.CorrectSlashes(
				Path.Combine(prefix, ValueToStringConverter(current).Substring(oldPrefix.Length).TrimStart('/')))));

		protected abstract string ValueToStringConverter(T obj);

		protected abstract T StringToValueConverter(string path);

		public void SetComponent(string text) => SetFilePath(text);

		public override void Submit() => SetFilePath(editor.Text);

		private void SetFilePath(string path)
		{
			string asset, type;
			if (Utils.ExtractAssetPathOrShowAlert(path, out asset, out type) &&
			    Utils.AssertCurrentDocument(asset, type))
			{
				if (!IsValid(asset)) {
					AlertDialog.Show($"Asset '{asset}' missing or path contains characters other than latin letters and digits.");
				} else {
					AssignAsset(AssetPath.CorrectSlashes(asset));
				}
			}
		}

		public override void DropFiles(IEnumerable<string> files)
		{
			var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
			if (nodeUnderMouse != null && nodeUnderMouse.SameOrDescendantOf(editor) && files.Any()) {
				SetFilePath(files.First());
			}
		}

		protected override void Copy() => Clipboard.Text = editor.Text;

		protected override void Paste()
		{
			try {
				AssignAsset(AssetPath.CorrectSlashes(Clipboard.Text));
			} catch (System.Exception) { }
		}

		protected abstract void AssignAsset(string path);

		protected virtual bool IsValid(string path)
		{
			return PropertyValidator.ValidateValue(path, EditorParams.PropertyInfo, out var none) == ValidationResult.Ok;
		}

		public string GetLongestCommonPrefix(List<string> paths)
		{
			if (paths == null || paths.Count == 0) {
				return String.Empty;
			}
			const char Separator = '/';
			var directoryParts = new List<string>(paths[0].Split(Separator));
			for (int index = 1; index < paths.Count; index++) {
				var first = directoryParts;
				var second = paths[index].Split(Separator);
				int maxPrefixLength = Math.Min(first.Count, second.Length);
				var tempDirectoryParts = new List<string>(maxPrefixLength);
				for (int part = 0; part < maxPrefixLength; part++) {
					if (first[part] == second[part])
						tempDirectoryParts.Add(first[part]);
				}
				directoryParts = tempDirectoryParts;
			}
			return String.Join(Separator.ToString(), directoryParts);
		}

		public bool TryGetClosestAvailableDirectory(string path, out string directory)
		{
			directory = path;
			while (!Directory.Exists(directory)) {
				directory = Path.GetDirectoryName(directory);
				if (string.IsNullOrEmpty(directory)) {
					return false;
				}
			}
			return true;
		}

		protected virtual void OnSelectClicked()
		{
			var current = CoalescedPropertyValue().GetDataflow();
			current.Poll();
			var value = current.Value;
			var path = ValueToStringConverter(value.Value);
			var dlg = new FileDialog {
				AllowedFileTypes = allowedFileTypes,
				Mode = FileDialogMode.Open,
				InitialDirectory =
					current.GotValue && value.IsDefined && !string.IsNullOrEmpty(path) && TryGetClosestAvailableDirectory(
						AssetPath.Combine(Project.Current.AssetsDirectory, path), out var dir) ?
							dir : Directory.Exists(LastOpenedDirectory) ?
								LastOpenedDirectory : Project.Current.AssetsDirectory
			};
			if (dlg.RunModal()) {
				SetFilePath(dlg.FileName);
				LastOpenedDirectory = Path.GetDirectoryName(dlg.FileName);
			}
		}
	}
}
