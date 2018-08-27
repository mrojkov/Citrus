using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public abstract class FilePropertyEditor<T> : CommonPropertyEditor<T>
	{
		protected readonly EditBox editor;
		protected readonly Button button;
		protected static string LastOpenedDirectory = Project.Current.GetSystemDirectory(Document.Current.Path);

		protected FilePropertyEditor(IPropertyEditorParams editorParams, string[] allowedFileTypes) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					new HSpacer(4),
					(button = new ThemedButton {
						Text = "...",
						MinMaxWidth = 20,
						LayoutCell = new LayoutCell(Alignment.Center)
					})
				}
			});
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Submitted += text => SetComponent(text);
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
		}

		public void SetComponent(string text)
		{
			AssignAsset(AssetPath.CorrectSlashes(text));
		}

		public override void Submit()
		{
			SetComponent(editor.Text);
		}

		private void SetFilePath(string path)
		{
			string asset, type;
			if (Utils.ExtractAssetPathOrShowAlert(path, out asset, out type) &&
			    Utils.AssertCurrentDocument(asset, type)) {
				AssignAsset(AssetPath.CorrectSlashes(asset));
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
			var invalidChars = Path.GetInvalidPathChars();
			if (path.Any(i => invalidChars.Contains(i))) {
				return false;
			}
			return true;
		}
	}
}
