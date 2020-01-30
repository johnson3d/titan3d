using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    public sealed partial class ResourceControl
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();
        }

        //public static string GetResourceParams(EditorCommon.Resources.ResourceInfoBase resInfo)
        //{
        //    resInfo.GetSnapshotImage()
        //}

        private void Button_SearchResource_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
