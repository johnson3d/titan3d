// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System.Windows.Input;

namespace ICSharpCode.WpfDesign.Designer.PropertyGrid.Editors.BrushEditor
{
	public partial class BrushTypeEditor
	{
		public BrushTypeEditor()
		{
			InitializeComponent();
		}

		static BrushEditorPopup brushEditorPopup = new BrushEditorPopup();

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			//brushEditorPopup.BrushEditorView.BrushEditor.Property = DataContext as PropertyNode;
			//brushEditorPopup.PlacementTarget = this;
			//brushEditorPopup.IsOpen = true;
		}
	}
}
