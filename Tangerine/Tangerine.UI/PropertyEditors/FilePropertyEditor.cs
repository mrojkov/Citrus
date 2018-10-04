using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public abstract class FilePropertyEditor<T> : ExpandablePropertyEditor<T>
	{
		private class PrefixData {
			public string Prefix { get; set; }
		}

		protected readonly EditBox editor;
		protected readonly Button button;
		protected static string LastOpenedDirectory = Project.Current.GetSystemDirectory(Document.Current.Path);

		private readonly PrefixData prefix = new PrefixData();

		protected FilePropertyEditor(IPropertyEditorParams editorParams, string[] allowedFileTypes) : base(editorParams)
		{
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
			bool textValid = true;
			editor.AddChangeWatcher(() => editor.Text, text => textValid = IsValid(text));
			editor.CompoundPostPresenter.Add(new SyncDelegatePresenter<EditBox>(editBox => {
				if (!textValid) {
					editBox.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, editBox.Size, Color4.Red.Transparentify(0.8f));
				}
			}));
			button.Clicked += () => {
				var dlg = new FileDialog {
					AllowedFileTypes = allowedFileTypes,
					Mode = FileDialogMode.Open,
					InitialDirectory = Directory.Exists(LastOpenedDirectory) ? LastOpenedDirectory : Project.Current.GetSystemDirectory(Document.Current.Path),
				};
				if (dlg.RunModal()) {
					SetFilePath(dlg.FileName);
					LastOpenedDirectory = Project.Current.GetSystemDirectory(dlg.FileName);
				}
			};
			ExpandableContent.Padding = new Thickness(24, 10, 2, 2);
			var prefixEditor = new StringPropertyEditor(new PropertyEditorParams(ExpandableContent, prefix, nameof(PrefixData.Prefix)) { LabelWidth = 180 });
			prefix.Prefix = GetLongestCommonPrefix(GetPaths());
			ContainerWidget.AddChangeWatcher(() => prefix.Prefix, v => {
				string oldPrefix = GetLongestCommonPrefix(GetPaths());
				if (oldPrefix == v) {
					return;
				}
				SetPathPrefix(oldPrefix, v);
				prefix.Prefix = v.Trim('/');
			});

			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = ValueToStringConverter(v) ?? "");
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

		public void SetComponent(string text)
		{
			SetFilePath(text);
		}

		public override void Submit()
		{
			SetFilePath(editor.Text);
		}

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

		protected override void Copy()
		{
			Clipboard.Text = editor.Text;
		}

		protected override void Paste()
		{
			try {
				AssignAsset(AssetPath.CorrectSlashes(Clipboard.Text));
			} catch (System.Exception) { }
		}

		protected abstract void AssignAsset(string path);

		protected virtual bool IsValid(string path)
		{
			foreach (var c in path) {
				if (!ValidChars.Contains(c)) {
					return false;
				}
			}
			return true;
		}

		private static readonly IReadOnlyList<char> validchars =
			Enumerable.Range(1, 128).Select(i => (char)i).
			Where(c =>
				char.IsLetterOrDigit(c) ||
				c == '\\' || c == '/' ||
				c == '_' || c == '.' || c == '!').ToList();

		protected virtual IEnumerable<char> ValidChars => validchars;

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
	}
}
