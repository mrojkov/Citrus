using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class CaretDisplay
	{
		private Widget container;
		private ICaretPosition caretPos;
		private Widget caretWidget;

		public CaretDisplay(Widget container, ICaretPosition caretPos, Widget caretWidget)
		{
			this.container = container;
			this.caretPos = caretPos;
			this.caretWidget = caretWidget;
			container.AddNode(caretWidget);
			container.Tasks.Add(CaretDisplayTask());
		}

		private IEnumerator<object> CaretDisplayTask()
		{
			while (true) {
				caretWidget.Position = caretPos.GetWorldPosition();
				caretWidget.Visible = caretPos.IsVisible;
				yield return 0;
			}
		}
	}

	public interface IEditorParams
	{
		float KeyPressInterval { get; set; }
	}

	public class EditorParams : IEditorParams
	{
		public float KeyPressInterval { get; set; }

		public EditorParams()
		{
			KeyPressInterval = 0.05f;
		}
	}

	public class Editor
	{
		private Widget container;
		private IText text;
		private ICaretPosition caretPos;
		private IEditorParams editorParams;

		public Editor(Widget container, ICaretPosition caretPos, IEditorParams editorParams)
		{
			this.container = container;
			text = (IText)container;
			this.caretPos = caretPos;
			this.editorParams = editorParams;
			container.Tasks.Add(HandleKeyboardTask());
		}

		private IEnumerator<object> HandleKeyboardTask()
		{
			while (true) {
				if (Input.IsKeyPressed(Key.Left))
					caretPos.Pos--;
				if (Input.IsKeyPressed(Key.Right))
					caretPos.Pos++;
				if (Input.IsKeyPressed(Key.Up))
					caretPos.Line--;
				if (Input.IsKeyPressed(Key.Down))
					caretPos.Line++;
				yield return editorParams.KeyPressInterval;
			}
		}
	}
}
