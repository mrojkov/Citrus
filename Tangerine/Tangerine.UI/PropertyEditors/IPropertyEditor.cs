using System;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public interface IPropertyEditor
	{
		IPropertyEditorParams EditorParams { get; }
		Widget ContainerWidget { get; }
		SimpleText PropertyLabel { get; }
		void DropFiles(IEnumerable<string> files);
		void Submit();
	}
}
