using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.Bricks.CodeBuilder.MacrossNode.UMacrossEditor;

namespace EngineNS.UI.Editor
{
    public partial class TtUIEditor
    {
        Bricks.CodeBuilder.MacrossNode.UMacrossEditor mMacrossEditor = null;

        void InitMacrossEditor()
        {
            mMacrossEditor = new Bricks.CodeBuilder.MacrossNode.UMacrossEditor();
            mMacrossEditor.AssetName = AssetName;
            mMacrossEditor.DefClass.ClassName = AssetName.PureName;
            mMacrossEditor.DefClass.Namespace = new Bricks.CodeBuilder.UNamespaceDeclaration(IO.TtFileManager.GetParentPathName(AssetName.Name).TrimEnd('/').Replace('/', '.'));
            mMacrossEditor.DefClass.SupperClassNames.Add(typeof(Controls.TtUIElement).FullName);
            mMacrossEditor.SaveClassGraph(AssetName);
            mMacrossEditor.GenerateCode();
            mMacrossEditor.CompileCode();

            mMacrossEditor.DrawToolbarAction = DrawMacrossToolbar;
        }
        STToolButtonData[] mToolBtnDatas = new STToolButtonData[7];
        void DrawMacrossToolbar(ImDrawList drawList)
        {
            int toolBarItemIdx = 0;
            var spacing = EGui.UIProxy.StyleConfig.Instance.ToolbarSeparatorThickness + EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X * 2;
            EGui.UIProxy.Toolbar.BeginToolbar(in drawList);

            if(EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Show Designer"))
            {
                mDrawType = enDrawType.Designer;
            }
            toolBarItemIdx++;
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Save"))
            {
                Save();
            }
            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "GenCode", false, -1, 0, spacing))
            {
                mMacrossEditor.GenerateCode();
                mMacrossEditor.CompileCode();
            }

            toolBarItemIdx++;
            EGui.UIProxy.ToolbarSeparator.DrawSeparator(in drawList, in Support.UAnyPointer.Default);
            if (Macross.UMacrossDebugger.Instance.CurrrentBreak != null)
            {
                if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(in drawList,
                    ref mToolBtnDatas[toolBarItemIdx].IsMouseDown, ref mToolBtnDatas[toolBarItemIdx].IsMouseHover, null, "Run", false, -1, 0, spacing))
                {
                    Macross.UMacrossDebugger.Instance.Run();
                }
            }

            EGui.UIProxy.Toolbar.EndToolbar();
        }
        public void OnDrawMacrossWindow()
        {
            mMacrossEditor.OnDraw();
        }
    }
}
