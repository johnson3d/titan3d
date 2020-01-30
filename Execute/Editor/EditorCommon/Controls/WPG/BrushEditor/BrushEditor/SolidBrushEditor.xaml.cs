// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System.Windows;
using System.Windows.Media;

namespace ICSharpCode.WpfDesign.Designer.PropertyGrid.Editors.BrushEditor
{
    public partial class SolidBrushEditor
	{
		public SolidBrushEditor()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register("Color", typeof(Color), typeof(SolidBrushEditor),
			                            new FrameworkPropertyMetadata(new Color(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public Color Color {
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}
	}
}
