// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Windows.Input;

namespace ICSharpCode.WpfDesign.Designer.PropertyGrid.Editors.BrushEditor
{
    public partial class BrushEditorPopup
	{
		public BrushEditorPopup()
		{
			InitializeComponent();
		}

		protected override void OnClosed(EventArgs e)
		{
		    base.OnClosed(e);
		    BrushEditorView.BrushEditor.Commit();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape) IsOpen = false;
		}
	}
}
