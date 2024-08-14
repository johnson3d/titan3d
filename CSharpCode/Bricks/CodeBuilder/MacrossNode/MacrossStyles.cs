using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class MacrossStyles
    {
        public uint BGColor = 0x80808080;

        public static MacrossStyles Instance = new MacrossStyles();
        public EGui.TtUVAnim FunctionIcon = new EGui.TtUVAnim(0xFF00FF00, 25);
        public uint FunctionTitleColor = 0xFF204020;
        public uint FunctionBGColor = 0x80808080;

        public EGui.TtUVAnim MemberIcon = new EGui.TtUVAnim(0xFF0000FF, 25);
        public EGui.TtUVAnim LocalVarIcon = new EGui.TtUVAnim(0xFF0080FF, 25);
        public uint VarTitleColor = 0xFF204040;
        public uint VarBGColor = 0x80808080;

        public uint FlowControlTitleColor = 0xFF299325;

        public LinkDesc NewExecPinDesc()
        {
            var styles = UNodeGraphStyles.DefaultStyles;

            var result = new LinkDesc();
            result.Icon.TextureName = RName.GetRName(styles.PinConnectedExecImg, RName.ERNameType.Engine);
            result.Icon.Size = new Vector2(12, 16);
            result.DisconnectIcon.TextureName = RName.GetRName(styles.PinDisconnectedExecImg, RName.ERNameType.Engine);
            result.DisconnectIcon.Size = new Vector2(12, 16);

            result.ExtPadding = 10;
            result.LineColor = 0xFFFFFFFF;
            result.LineThinkness = 6;
            result.CanLinks.Add("Exec");
            return result;
        }
        public LinkDesc NewInOutPinDesc()
        {
            var styles = UNodeGraphStyles.DefaultStyles;

            var result = new LinkDesc();
            result.Icon.TextureName = RName.GetRName(styles.PinConnectedVarImg, RName.ERNameType.Engine);
            result.Icon.Size = new Vector2(15, 11);
            result.DisconnectIcon.TextureName = RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine);
            result.DisconnectIcon.Size = new Vector2(15, 11);

            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            return result;
        }
    }
}
