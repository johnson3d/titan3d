using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.Bricks.NodeGraph
{
    public class UNodeGraphStyles
    {
        public static UNodeGraphStyles DefaultStyles = new UNodeGraphStyles();
        public UNodeGraphStyles()
        {
            PinInStyle.Image.TextureName = RName.GetRName(PinConnectedVarImg, RName.ERNameType.Engine);
            PinInStyle.Image.Size = new Vector2(15, 11);
            PinInStyle.DisconnectImage.TextureName = RName.GetRName(PinDisconnectedVarImg, RName.ERNameType.Engine);
            PinInStyle.DisconnectImage.Size = new Vector2(15, 11);

            PinOutStyle.Image.TextureName = RName.GetRName(PinConnectedVarImg, RName.ERNameType.Engine);
            PinOutStyle.Image.Size = new Vector2(15, 11);
            PinOutStyle.DisconnectImage.TextureName = RName.GetRName(PinDisconnectedVarImg, RName.ERNameType.Engine);
            PinOutStyle.DisconnectImage.Size = new Vector2(15, 11);
        }
        public enum EFlowMode
        {
            Horizon,
            Vertical,
        }
        public EFlowMode FlowMode { get; set; } = EFlowMode.Horizon;
        public class PinStyle
        {
            public EGui.UUvAnim Image { get; set; } = new EGui.UUvAnim();
            public EGui.UUvAnim DisconnectImage { get; set; } = new EGui.UUvAnim();
            public float Offset { get; set; } = 3;
            public float TextOffset { get; set; } = 3;
        }
        public PinStyle PinInStyle { get; set; } = new PinStyle();
        public PinStyle PinOutStyle { get; set; } = new PinStyle();
        public float PinSpacing { get; set; } = 5;
        public float PinPadding { get; set; } = 8;
        public Vector2 IconOffset { get; set; } = new Vector2(2, 2);
        public float MinSpaceInOut { get; set; } = 20.0f;
        public UInt32 TitleTextColor { get; set; } = 0xFFFFFFFF;
        public Vector2 TitlePadding = new Vector2(8, 6);
        public UInt32 PinTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 SelectedColor { get; set; } = 0xFFFF00FF;
        public UInt32 LinkerColor { get; set; } = 0xFF00FFFF;
        public float LinkerThinkness { get; set; } = 3;
        public UInt32 HighLightColor { get; set; } = 0xFF0000FF;
        public float BezierPixelPerSegement { get; set; } = 10.0f;

        public UInt32 DefaultTitleColor = 0xFF800000;
        public UInt32 DefaultPinColor = 0xFF69936e;
        public UInt32 ValuePinColor = 0xFF00d037;
        public UInt32 VectorPinColor = 0xFF23c9fe;
        public UInt32 ValueTitleColor = 0xFF69936e;
        public UInt32 EndingTitleColor = 0xFF204020;
        public UInt32 FuncTitleColor = 0xFFff9c00;
        public UInt32 VectorTitleColor = 0xFF23c9fe;

        public UInt32 GridStep = 16;
        public UInt32 GridNormalLineColor = 0xFF353535;
        public UInt32 GridSplitLineColor = 0xFF161616;
        public UInt32 GridBackgroundColor = 0xFF262626;

        public string NodeBodyImg = "uestyle/graph/regularnode_body.srv";
        public string NodeTitleImg = "uestyle/graph/regularnode_title_gloss.srv";
        public string NodeColorSpillImg = "uestyle/graph/regularnode_color_spill.srv";
        public string NodeTitleHighlightImg = "uestyle/graph/regularnode_title_highlight.srv";
        public string NodeShadowImg = "uestyle/graph/regularnode_shadow.srv";
        public string NodeShadowSelectedImg = "uestyle/graph/regularnode_shadow_selected.srv";
        public string PinConnectedVarImg = "uestyle/graph/pin_connected_vara.srv";
        public string PinDisconnectedVarImg = "uestyle/graph/pin_disconnected_vara.srv";
        public string PinConnectedExecImg = "uestyle/graph/execpin_connected.srv";
        public string PinDisconnectedExecImg = "uestyle/graph/execpin_disconnected.srv";
        public string PinHoverCueImg = "uestyle/graph/pin_hover_cue.srv";

    }
}
