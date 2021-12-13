using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class MacrossStyles
    {
        public uint BGColor = 0x80808080;

        public static MacrossStyles Instance = new MacrossStyles();
        public EGui.UUvAnim FunctionIcon = new EGui.UUvAnim(0xFF00FF00, 25);
        public uint FunctionTitleColor = 0xFF204020;
        public uint FunctionBGColor = 0x80808080;

        public EGui.UUvAnim MemberIcon = new EGui.UUvAnim(0xFF0000FF, 25);
        public EGui.UUvAnim LocalVarIcon = new EGui.UUvAnim(0xFF0080FF, 25);
        public uint VarTitleColor = 0xFF204040;
        public uint VarBGColor = 0x80808080;

        public uint FlowControlTitleColor = 0xFF299325;

        public EGui.Controls.NodeGraph.NodePin.LinkDesc NewExecPinDesc()
        {
            var result = new EGui.Controls.NodeGraph.NodePin.LinkDesc();
            result.Icon.Size = new Vector2(25, 25);
            result.ExtPadding = 10;
            result.LineColor = 0xFFFFFFFF;
            result.LineThinkness = 6;
            result.CanLinks.Add("Exec");
            return result;
        }
        public EGui.Controls.NodeGraph.NodePin.LinkDesc NewInOutPinDesc()
        {
            var result = new EGui.Controls.NodeGraph.NodePin.LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            return result;
        }
    }
}
